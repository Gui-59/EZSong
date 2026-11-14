using Melanchall.DryWetMidi.Core;
using System.Text;

namespace EZSong.MIDI {

    /// <summary>
    /// Extracts karaoke lyrics from MIDI tracks, independently of the tracks
    /// selected for transcription.
    ///
    /// Lyrics may be stored as LYRIC meta events (FF 05) or as TEXT meta
    /// events (FF 01) following the KAR conventions: a payload starting with
    /// '\' or '/' is a lyric line, while '\@' or '@' introduces metadata
    /// (title, author, language...) and is ignored. When a file only contains
    /// plain TEXT events, the richest track is used as a fallback.
    /// </summary>
    public static class MidiLyricsExtractor {

        /// <summary>A lyric event expressed in absolute MIDI ticks.</summary>
        public readonly record struct Lyric(long Time, string Text);

        /// <summary>
        /// Extracts the lyrics of every track. Duplicated lyric tracks are
        /// merged without repeating events, while complementary tracks (for
        /// example a verse and a chorus stored separately) are combined.
        /// </summary>
        /// <param name="tracks">MIDI tracks of the file.</param>
        /// <param name="timeTolerance">
        /// Maximum tick distance used to consider two events from different
        /// tracks as duplicates.
        /// </param>
        public static List<Lyric> Extract(IEnumerable<TrackChunk> tracks, long timeTolerance = 0) {
            if (timeTolerance < 0) {
                throw new ArgumentOutOfRangeException(nameof(timeTolerance));
            }

            List<TrackLyrics> streams = tracks
                .Select(ReadTrack)
                .Where(stream => stream.Lyrics.Count > 0)
                .ToList();
            if (streams.Count == 0) {
                return new List<Lyric>();
            }

            List<TrackLyrics> candidates;
            if (streams.Any(stream => stream.HasExplicitLyrics)) {
                candidates = streams.Where(stream => stream.HasExplicitLyrics).ToList();
            } else {
                // Sans événement LYRIC ni marqueur KAR, on utilise la piste
                // texte la plus fournie pour éviter d'ajouter des annotations
                // (intro, solo...) présentes dans d'autres pistes.
                candidates = new List<TrackLyrics> {
                    streams.OrderByDescending(stream => stream.Lyrics.Count).First()
                };
            }

            List<TrackLyrics> uniqueStreams = RemoveCoveredStreams(candidates, timeTolerance);
            return MergeStreams(uniqueStreams, timeTolerance);
        }

        private static TrackLyrics ReadTrack(TrackChunk track) {
            long time = 0;
            bool pendingLineBreak = false;
            List<Lyric> explicitLyrics = new();
            List<Lyric> plainLyrics = new();
            foreach (MidiEvent midiEvent in track.Events) {
                time += midiEvent.DeltaTime;
                if (midiEvent is LyricEvent lyric) {
                    string text = CleanText(lyric.Text);
                    if (text.Length == 0) {
                        pendingLineBreak = pendingLineBreak || !string.IsNullOrEmpty(lyric.Text);
                        continue;
                    }

                    explicitLyrics.Add(new Lyric(time, ApplyPendingLineBreak(text, ref pendingLineBreak)));
                } else if (midiEvent is TextEvent textEvent) {
                    string rawContent = textEvent.Text ?? string.Empty;
                    string trimmedContent = rawContent.Trim();
                    if (trimmedContent.Length == 0) {
                        // A control-character-only event is a KAR line break.
                        pendingLineBreak = pendingLineBreak || rawContent.Length > 0;
                        continue;
                    }
                    if (trimmedContent.StartsWith('@') ||
                        trimmedContent.StartsWith("\\@", StringComparison.Ordinal) ||
                        trimmedContent.StartsWith("/@", StringComparison.Ordinal)) {
                        continue;
                    }

                    bool isKarLyric = trimmedContent.StartsWith('\\') || trimmedContent.StartsWith('/');
                    string content = rawContent;
                    if (isKarLyric) {
                        // Drop the KAR line marker but keep the spacing that
                        // follows it: it carries word boundaries.
                        content = rawContent.TrimStart()[1..];
                    }

                    string text = CleanText(content);
                    if (text.Length == 0) {
                        pendingLineBreak = true;
                        continue;
                    }

                    text = ApplyPendingLineBreak(text, ref pendingLineBreak);
                    if (isKarLyric) {
                        explicitLyrics.Add(new Lyric(time, text));
                    } else {
                        plainLyrics.Add(new Lyric(time, text));
                    }
                }
            }

            if (explicitLyrics.Count == 0) {
                return new TrackLyrics(plainLyrics, false);
            }

            // Dans une piste contenant de vrais événements LYRIC, les TEXT
            // events non marqués prolongent souvent une ligne de paroles
            // ("Hel" puis "lo") : on les conserve. La piste étant clairement
            // une piste de paroles, ils ne peuvent pas être des annotations
            // provenant d'une autre piste.
            List<Lyric> combined = explicitLyrics;
            foreach (Lyric lyric in plainLyrics) {
                string signature = Signature(lyric.Text);
                bool duplicate = combined.Any(existing =>
                    existing.Time == lyric.Time && Signature(existing.Text) == signature);
                if (!duplicate) {
                    combined.Add(lyric);
                }
            }
            combined.Sort((left, right) => left.Time.CompareTo(right.Time));
            return new TrackLyrics(combined, true);
        }

        private static string ApplyPendingLineBreak(string text, ref bool pendingLineBreak) {
            if (!pendingLineBreak) {
                return text;
            }

            pendingLineBreak = false;
            return char.IsWhiteSpace(text[0]) ? text : " " + text;
        }

        /// <summary>
        /// Removes streams that are already fully present in a richer stream.
        /// This handles files where the same lyrics are duplicated on several
        /// tracks (for example a dedicated lyric track and the melody track).
        /// </summary>
        private static List<TrackLyrics> RemoveCoveredStreams(List<TrackLyrics> streams, long timeTolerance) {
            List<TrackLyrics> ordered = streams
                .OrderByDescending(stream => stream.Lyrics.Count)
                .ToList();
            List<TrackLyrics> unique = new();
            foreach (TrackLyrics stream in ordered) {
                bool covered = unique.Any(existing => IsCoveredBy(stream, existing, timeTolerance));
                if (!covered) {
                    unique.Add(stream);
                }
            }
            return unique;
        }

        private static bool IsCoveredBy(TrackLyrics stream, TrackLyrics other, long timeTolerance) {
            if (stream.Lyrics.Count > other.Lyrics.Count) {
                return false;
            }

            foreach (Lyric lyric in stream.Lyrics) {
                string signature = Signature(lyric.Text);
                bool found = other.Lyrics.Any(candidate =>
                    Signature(candidate.Text) == signature &&
                    Math.Abs(candidate.Time - lyric.Time) <= timeTolerance);
                if (!found) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Merges complementary streams and drops events duplicated by another
        /// stream. Events belonging to the same stream are never removed, so a
        /// repeated word remains repeated.
        /// </summary>
        private static List<Lyric> MergeStreams(List<TrackLyrics> streams, long timeTolerance) {
            List<Lyric> merged = new();
            foreach (TrackLyrics stream in streams) {
                int previouslyMergedCount = merged.Count;
                foreach (Lyric lyric in stream.Lyrics) {
                    string signature = Signature(lyric.Text);
                    bool duplicate = false;
                    for (int i = 0; i < previouslyMergedCount; i++) {
                        if (Signature(merged[i].Text) == signature &&
                            Math.Abs(merged[i].Time - lyric.Time) <= timeTolerance) {
                            duplicate = true;
                            break;
                        }
                    }
                    if (!duplicate) {
                        merged.Add(lyric);
                    }
                }
            }

            return merged
                .OrderBy(lyric => lyric.Time)
                .ThenBy(lyric => lyric.Text, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Builds the display text of a group of lyrics (typically one
        /// measure). KAR text events carry significant leading/trailing
        /// spaces: when they do, texts are concatenated as-is so that the
        /// syllables of a word stay together ("VE" + "CU " gives "VECU").
        /// Otherwise texts are separated by spaces.
        /// </summary>
        public static string Format(IEnumerable<Lyric> lyrics) {
            List<string> texts = new();
            bool hasSpacingCues = false;
            foreach (Lyric lyric in lyrics) {
                if (lyric.Text.Length == 0) {
                    continue;
                }
                if (char.IsWhiteSpace(lyric.Text[0]) || char.IsWhiteSpace(lyric.Text[^1])) {
                    hasSpacingCues = true;
                }
                texts.Add(lyric.Text);
            }

            if (texts.Count == 0) {
                return string.Empty;
            }

            string text = hasSpacingCues ? string.Concat(texts) : string.Join(" ", texts);
            return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Groups syllable-level lyric events into words. When the source uses
        /// KAR spacing, a word is dated by its first syllable so that it is
        /// not split when a bar line falls inside the word. Files without any
        /// spacing cue already store one word per event.
        /// </summary>
        public static List<Lyric> GroupWords(IEnumerable<Lyric> lyrics) {
            List<Lyric> source = lyrics
                .Where(lyric => lyric.Text.Length > 0)
                .ToList();
            bool hasSpacingCues = source.Any(lyric =>
                char.IsWhiteSpace(lyric.Text[0]) || char.IsWhiteSpace(lyric.Text[^1]));
            if (!hasSpacingCues) {
                return source
                    .Select(lyric => new Lyric(lyric.Time, lyric.Text.Trim()))
                    .Where(lyric => lyric.Text.Length > 0)
                    .ToList();
            }

            List<Lyric> words = new();
            StringBuilder currentWord = new();
            long wordTime = 0;
            bool hasCurrentWord = false;
            foreach (Lyric lyric in source) {
                bool leadingSpace = char.IsWhiteSpace(lyric.Text[0]);
                bool trailingSpace = char.IsWhiteSpace(lyric.Text[^1]);
                string text = lyric.Text.Trim();
                if (text.Length == 0) {
                    continue;
                }

                if (leadingSpace && hasCurrentWord) {
                    words.Add(new Lyric(wordTime, currentWord.ToString()));
                    currentWord.Clear();
                    hasCurrentWord = false;
                }
                if (!hasCurrentWord) {
                    wordTime = lyric.Time;
                    hasCurrentWord = true;
                }
                currentWord.Append(text);

                if (trailingSpace) {
                    words.Add(new Lyric(wordTime, currentWord.ToString()));
                    currentWord.Clear();
                    hasCurrentWord = false;
                }
            }

            if (hasCurrentWord) {
                words.Add(new Lyric(wordTime, currentWord.ToString()));
            }
            return words;
        }

        /// <summary>
        /// Collapses whitespace and removes control characters while keeping
        /// a single leading or trailing space, as KAR lyric events use them to
        /// separate words. Whitespace-only events are discarded.
        /// </summary>
        private static string CleanText(string? text) {
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }

            StringBuilder builder = new(text.Length);
            bool pendingSpace = false;
            bool hasContent = false;
            foreach (char character in text) {
                if (character == '\uFEFF') {
                    continue;
                }
                if (char.IsControl(character) || char.IsWhiteSpace(character)) {
                    pendingSpace = true;
                    continue;
                }
                if (pendingSpace) {
                    builder.Append(' ');
                    pendingSpace = false;
                }
                builder.Append(character);
                hasContent = true;
            }

            if (pendingSpace && hasContent) {
                builder.Append(' ');
            }
            return builder.ToString();
        }

        private static string Signature(string text) {
            StringBuilder builder = new(text.Length);
            foreach (char character in text) {
                if (!char.IsWhiteSpace(character)) {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }
            return builder.ToString();
        }

        private sealed class TrackLyrics {

            public List<Lyric> Lyrics {
                get;
            }

            public bool HasExplicitLyrics {
                get;
            }

            public TrackLyrics(List<Lyric> lyrics, bool hasExplicitLyrics) {
                Lyrics = lyrics;
                HasExplicitLyrics = hasExplicitLyrics;
            }
        }
    }
}
