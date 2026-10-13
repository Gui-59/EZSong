using System.Text.RegularExpressions;
using System.Globalization;
using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Parses a text chord progression (one chord per line) and resolves it to
/// playable voicings using <see cref="ChordVoicingEngine"/>.
///
/// Example:
/// <code>
/// C
/// G/B
/// Am
/// F
/// G7
/// C
/// </code>
/// </summary>
public sealed class ChordProgressionParser {
    private static readonly Regex _chordRootRegex = new(
        @"^\s*(?<root>[A-Ga-g](?:#|b|♯|♭)?)(?<rest>.*?)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _slashBassRegex = new(
        @"/(?<bass>[A-Ga-g](?:#|b|♯|♭)?)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _lineRegex = new(
        @"^\s*\|\s*(?<chord>\S+)\s+(?<duration>\S+)\s*\|\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _durationRegex = new(
        @"^(?<denominator>[1-9][0-9]*)(?<dots>\.{0,2})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ChordVoicingEngine _voicingEngine;

    public ChordProgressionParser(ChordVoicingEngine? voicingEngine = null) {
        _voicingEngine = voicingEngine ?? new ChordVoicingEngine();
    }

    /// <summary>
    /// Parses a progression where every non-empty line is one measure and must
    /// be enclosed by measure bars.
    ///
    /// Example:
    /// <code>
    /// | C   4  |
    /// | G/B 4  |
    /// | Am  2  |
    /// | F   2  |
    /// | G7  4. |
    /// </code>
    ///
    /// The duration notation follows the simple rhythm notation used by
    /// EZSong: 1 = whole note, 2 = half note, 4 = quarter note, 8 = eighth
    /// note, etc.; one or two dots are supported.
    /// </summary>
    public IReadOnlyList<ChordProgressionEntry> ParseEntries(string text) {
        ArgumentNullException.ThrowIfNull(text);

        List<ChordProgressionEntry> entries = new();
        string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        int measureNumber = 0;

        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i].Trim();

            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal)) {
                continue;
            }

            measureNumber++;

            // Every non-empty line is exactly one measure and MUST have
            // exactly one leading '|' and one trailing '|'.
            // This is intentionally stricter than merely looking for a
            // substring matching the expected format.
            if (!line.StartsWith("|", StringComparison.Ordinal) ||
                !line.EndsWith("|", StringComparison.Ordinal)) {
                throw new FormatException(
                    $"Invalid progression line {i + 1}: '{line}'. " +
                    "A measure must start and end with '|', for example '| C 4 |'.");
            }

            if (line.Length >= 2 &&
                (line.StartsWith("||", StringComparison.Ordinal) ||
                 line.EndsWith("||", StringComparison.Ordinal))) {
                throw new FormatException(
                    $"Invalid progression line {i + 1}: '{line}'. " +
                    "A measure must contain exactly one opening and one closing '|'.");
            }

            string content = line[1..^1].Trim();

            if (content.Length == 0) {
                throw new FormatException(
                    $"Invalid progression line {i + 1}: '{line}'. " +
                    "The measure cannot be empty.");
            }

            Match lineMatch = Regex.Match(
                content,
                @"^(?<chord>\S+)\s+(?<duration>\S+)$",
                RegexOptions.CultureInvariant);

            if (!lineMatch.Success) {
                throw new FormatException(
                    $"Invalid progression line {i + 1}: '{line}'. " +
                    "Expected '| CHORD DURATION |', for example '| C 4 |'.");
            }

            string chordText = lineMatch.Groups["chord"].Value;
            string durationText = lineMatch.Groups["duration"].Value;

            try {
                RhythmSimpleElement duration = ParseDuration(durationText);
                Chord chord = ParseChord(chordText);
                chord.Duration = duration.GetEffectiveDuration();
                entries.Add(new ChordProgressionEntry(chord, duration, measureNumber));
            } catch (FormatException ex) {
                throw new FormatException(
                    $"Invalid progression at line {i + 1}: '{line}'. {ex.Message}", ex);
            }
        }

        return entries;
    }



    /// <summary>
    /// Parses the supplied progression into EZSong <see cref="Chord"/> instances.
    /// Bars and durations are mandatory.
    /// </summary>
    public IReadOnlyList<Chord> ParseChords(string text) {
        return ParseEntries(text).Select(entry => entry.Chord).ToArray();
    }

    /// <summary>
    /// Parses the progression and generates one voicing for each chord.
    /// Durations are retained on the returned Chord instances but are ignored
    /// by the voicing optimisation itself.
    /// </summary>
    public IReadOnlyList<ChordVoicing> ParseToVoicings(
        string text,
        IEnumerable<Pitch>? exactFirstVoicing = null) {
        IReadOnlyList<ChordProgressionEntry> entries = ParseEntries(text);

        if (entries.Count == 0) {
            return Array.Empty<ChordVoicing>();
        }

        Pitch[]? exact = exactFirstVoicing?.ToArray();
        List<ChordVoicingRequest> requests = new(entries.Count);

        for (int i = 0; i < entries.Count; i++) {
            requests.Add(i == 0 && exact is not null
                ? ChordVoicingRequest.FromExactVoicing(entries[i].Chord, exact)
                : new ChordVoicingRequest(entries[i].Chord));
        }

        return _voicingEngine.GenerateProgression(requests);
    }

    /// <summary>
    /// Parses a simple duration into the rhythm model used by EZSong.
    /// Examples: 1, 2, 4, 8, 4., 8.. .
    /// </summary>
    public static RhythmSimpleElement ParseDuration(string text) {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        Match match = _durationRegex.Match(text.Trim());
        if (!match.Success) {
            throw new FormatException(
                $"Invalid duration '{text}'. Expected values such as 1, 2, 4, 8, 4., or 8..");
        }

        int denominator = int.Parse(match.Groups["denominator"].Value, CultureInfo.InvariantCulture);
        int dots = match.Groups["dots"].Value.Length;

        if (denominator <= 0) {
            throw new FormatException("The duration denominator must be positive.");
        }

        return new RhythmSimpleElement(
            new RhythmRationalDuration(1, denominator, dots),
            false);
    }

    /// <summary>
    /// Parses a single chord name without a duration or measure bars.
    /// </summary>
    public static Chord ParseChord(string text) {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        string value = text.Trim();
        Match match = _chordRootRegex.Match(value);
        if (!match.Success) {
            throw new FormatException(
                "Expected a chord such as C, Am, G7, Bbmaj7, or G/B.");
        }

        string rootText = match.Groups["root"].Value;
        string rest = match.Groups["rest"].Value.Trim();

        Match bassMatch = _slashBassRegex.Match(rest);
        string suffix;
        string bassText = string.Empty;
        if (bassMatch.Success) {
            suffix = rest[..bassMatch.Index].Trim();
            bassText = bassMatch.Groups["bass"].Value;
        } else {
            suffix = rest;
        }

        (NoteStep rootNote, Alteration rootAlteration) = ParseNoteName(rootText);
        ChordType chordType = ParseChordType(suffix);

        NoteStep? bassNote = null;
        Alteration bassAlteration = Alteration.neutral;

        if (bassText.Length > 0) {
            (bassNote, bassAlteration) = ParseNoteName(bassText);
        }

        return new Chord(
            false,
            new RhythmRationalDuration(1, 4, 0),
            rootNote,
            rootAlteration,
            chordType,
            bassNote,
            bassAlteration);
    }

    private static (NoteStep Note, Alteration Alteration) ParseNoteName(string text) {
        if (string.IsNullOrWhiteSpace(text)) {
            throw new FormatException("Missing note name.");
        }

        NoteStep note = text[0] switch {
            'C' or 'c' => NoteStep.C,
            'D' or 'd' => NoteStep.D,
            'E' or 'e' => NoteStep.E,
            'F' or 'f' => NoteStep.F,
            'G' or 'g' => NoteStep.G,
            'A' or 'a' => NoteStep.A,
            'B' or 'b' => NoteStep.B,
            _ => throw new FormatException($"Unknown note '{text}'.")
        };

        Alteration alteration = Alteration.neutral;

        if (text.Length == 2) {
            alteration = text[1] switch {
                '#' or '♯' => Alteration.sharp,
                'b' or '♭' => Alteration.flat,
                _ => throw new FormatException($"Unknown accidental in '{text}'.")
            };
        } else if (text.Length > 2) {
            throw new FormatException($"Invalid note name '{text}'.");
        }

        return (note, alteration);
    }

    private static ChordType ParseChordType(string suffix) {
        string trimmed = suffix.Trim();

        if (trimmed == "M7") {
            return ChordType.MajorSeventh;
        }

        string normalized = trimmed.ToLowerInvariant();

        return normalized switch {
            "" or "maj" or "major" => ChordType.NoneOrMajor,
            "m" or "min" or "-" => ChordType.Minor,
            "7" => ChordType.Seventh,
            "m7" or "min7" => ChordType.MinorSeventh,
            "maj7" or "major7" => ChordType.MajorSeventh,
            "5" => ChordType.PowerChord,
            "6" => ChordType.Sixth,
            "m6" or "min6" => ChordType.MinorSixth,
            "sus2" => ChordType.SuspendedSecond,
            "sus4" or "sus" => ChordType.SuspendedFourth,
            "dim" or "o" => ChordType.Diminished,
            "aug" or "+" => ChordType.Augmented,
            "dim7" or "o7" => ChordType.DiminishedSeventh,
            "aug7" or "+7" => ChordType.AugmentedSeventh,
            "add2" => ChordType.AddSecond,
            "add4" => ChordType.AddFourth,
            "add6" => ChordType.AddSixth,
            "add9" => ChordType.AddNinth,
            "9" => ChordType.Ninth,
            "m9" or "min9" => ChordType.MinorNinth,
            "maj9" or "major9" => ChordType.MajorNinth,
            "11" => ChordType.Eleventh,
            "m11" or "min11" => ChordType.MinorEleventh,
            "maj11" or "major11" => ChordType.MajorEleventh,
            "13" => ChordType.Thirteenth,
            "m13" or "min13" => ChordType.MinorThirteenth,
            "maj13" or "major13" => ChordType.MajorThirteenth,
            "m(maj7)" or "min(maj7)" => ChordType.MinorMajorSeventh,
            "6/9" => ChordType.SixthNinth,
            "7-5" or "7b5" => ChordType.SeventhMinusFive,
            "7+5" or "7#5" => ChordType.SeventhPlusFive,
            "m7b5" or "min7b5" => ChordType.MinorSeventhFlatFive,
            _ => throw new FormatException($"Unknown chord suffix '{suffix}'.")
        };
    }
}
