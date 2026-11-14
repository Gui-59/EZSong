using EZSong.MIDI;
using Melanchall.DryWetMidi.Core;

namespace EZSong.Tests;

/// <summary>
/// Self-tests for karaoke lyric extraction: LYRIC meta events, KAR text
/// markers, metadata filtering, duplicated tracks and plain text fallback.
/// </summary>
public static class MidiLyricsSelfTest {

    private const long TimeTolerance = 60;

    public static void RunAll() {
        TestKarTextEventsAreReadWithoutSelection();
        TestLyricEventsAreRead();
        TestMetadataIsIgnored();
        TestDuplicateTracksAreMerged();
        TestComplementaryTracksAreMerged();
        TestPlainTextFallback();
        TestKarSpacingPreservesWords();
        TestWordsAreNotSplitAtBarLines();
        TestWhitespaceAndControlCharactersAreCleaned();
        Console.WriteLine("MidiLyricsSelfTest: ALL OK");
    }

    private static void TestKarTextEventsAreReadWithoutSelection() {
        // The extractor only receives the tracks, never the user selection:
        // lyrics are therefore available even when their track is not chosen
        // as a melody staff (here the track contains no note at all).
        TrackChunk lyricsTrack = new(
            new TextEvent("\\@KMIDI KARAOKE FILE"),
            new TextEvent("\\@TMy song"),
            new TextEvent("\\Hel"),
            new TextEvent("lo ") { DeltaTime = 480 },
            new TextEvent("/world") { DeltaTime = 240 }
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(new[] { lyricsTrack });

        Expect(lyrics.Count == 3, "KAR text events should produce three lyrics.");
        Expect(lyrics[0].Time == 0 && lyrics[0].Text == "Hel",
            "The first KAR lyric should keep its absolute time.");
        Expect(lyrics[1].Time == 480 && lyrics[1].Text.Trim() == "lo",
            "Delta times should accumulate within the lyric track.");
        Expect(lyrics[2].Time == 720 && lyrics[2].Text == "world",
            "The '/' line marker should be stripped from the lyric.");
        Expect(MidiLyricsExtractor.Format(lyrics) == "Hello world",
            "The KAR spacing should keep the syllables of a word together.");
    }

    private static void TestLyricEventsAreRead() {
        TrackChunk track = new(
            new LyricEvent("Hello"),
            new LyricEvent("world") { DeltaTime = 480 }
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(new[] { track });

        Expect(lyrics.Count == 2, "LYRIC meta events should be read.");
        Expect(lyrics[0].Time == 0 && lyrics[0].Text == "Hello",
            "The first LYRIC event should be at the start of the file.");
        Expect(lyrics[1].Time == 480 && lyrics[1].Text == "world",
            "The second LYRIC event should keep its delta time.");
        Expect(MidiLyricsExtractor.Format(lyrics) == "Hello world",
            "LYRIC events without spacing cues should be separated by spaces.");

        List<MidiLyricsExtractor.Lyric> words = MidiLyricsExtractor.GroupWords(lyrics);
        Expect(words.Count == 2 && words[0].Text == "Hello" && words[1].Text == "world",
            "Without spacing cues, every LYRIC event is already a word.");
    }

    private static void TestMetadataIsIgnored() {
        TrackChunk track = new(
            new TextEvent("\\@KMIDI KARAOKE FILE"),
            new TextEvent("\\@TTitle"),
            new TextEvent("\\@IArtist"),
            new TextEvent("@LENGL"),
            new TextEvent(" /@ignored"),
            new TextEvent("\\word") { DeltaTime = 480 }
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(new[] { track });

        Expect(lyrics.Count == 1, "KAR metadata must not be imported as lyrics.");
        Expect(lyrics[0].Text == "word", "Only the actual lyric should remain.");

        TrackChunk metadataOnly = new(
            new TextEvent("\\@KMIDI KARAOKE FILE"),
            new TextEvent("@TTitle")
        );
        Expect(MidiLyricsExtractor.Extract(new[] { metadataOnly }).Count == 0,
            "A track containing only KAR metadata has no lyric.");
    }

    private static void TestDuplicateTracksAreMerged() {
        // Some files store the same lyrics on a dedicated track and on the
        // melody track, sometimes with a few ticks of difference.
        TrackChunk first = new(
            new LyricEvent("Hello"),
            new LyricEvent("world") { DeltaTime = 480 }
        );
        TrackChunk second = new(
            new LyricEvent("Hello") { DeltaTime = 30 },
            new LyricEvent("world") { DeltaTime = 480 }
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(
            new[] { first, second }, TimeTolerance);

        Expect(lyrics.Count == 2, "Duplicated lyric tracks must not repeat the words.");
        Expect(lyrics[0].Text == "Hello" && lyrics[1].Text == "world",
            "The first duplicated track should be kept.");
    }

    private static void TestComplementaryTracksAreMerged() {
        // Lyrics split over several tracks (for example verse and chorus on
        // two different staves) must all be recovered.
        TrackChunk verse = new(
            new LyricEvent("Verse")
        );
        TrackChunk chorus = new(
            new LyricEvent("Chorus") { DeltaTime = 1920 }
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(
            new[] { verse, chorus }, TimeTolerance);

        Expect(lyrics.Count == 2, "Complementary lyric tracks must be combined.");
        Expect(lyrics[0].Time == 0 && lyrics[0].Text == "Verse",
            "The verse should keep its time.");
        Expect(lyrics[1].Time == 1920 && lyrics[1].Text == "Chorus",
            "The chorus should keep its time.");
    }

    private static void TestPlainTextFallback() {
        // Without LYRIC events or KAR markers, the richest text track is used
        // so that annotations from other tracks are not mixed with lyrics.
        TrackChunk lyricsTrack = new(
            new TextEvent("Hello"),
            new TextEvent("world") { DeltaTime = 480 }
        );
        TrackChunk markerTrack = new(
            new TextEvent("Intro")
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(
            new[] { lyricsTrack, markerTrack });

        Expect(lyrics.Count == 2, "The richest plain text track should be used.");
        Expect(lyrics[0].Text == "Hello" && lyrics[1].Text == "world",
            "Plain text lyrics should be recovered as a fallback.");
        Expect(MidiLyricsExtractor.Format(lyrics) == "Hello world",
            "Plain text lyrics should be joined into readable text.");
    }

    private static void TestKarSpacingPreservesWords() {
        // Real KAR files often split words into syllables and use trailing
        // spaces to mark word boundaries. Control characters are line breaks.
        TrackChunk track = new(
            new TextEvent("J'AI "),
            new TextEvent("VE") { DeltaTime = 1 },
            new TextEvent("CU ") { DeltaTime = 1 },
            new TextEvent("SOUS ") { DeltaTime = 1 },
            new TextEvent("DES ") { DeltaTime = 1 },
            new TextEvent("POS") { DeltaTime = 1 },
            new TextEvent("TERS ") { DeltaTime = 1 },
            new TextEvent("\r") { DeltaTime = 1 },
            new TextEvent("A ") { DeltaTime = 1 },
            new TextEvent("ME ") { DeltaTime = 1 }
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(new[] { track });

        Expect(lyrics.Count == 9, "Line-break-only events must be discarded.");
        Expect(MidiLyricsExtractor.Format(lyrics) == "J'AI VECU SOUS DES POSTERS A ME",
            "Syllables must stay attached while words remain separated.");

        List<MidiLyricsExtractor.Lyric> words = MidiLyricsExtractor.GroupWords(lyrics);
        Expect(words.Count == 7, "Syllables should be grouped into seven words.");
        Expect(words[1].Text == "VECU" && words[1].Time == 1,
            "A word should be dated by its first syllable.");
        Expect(words[4].Text == "POSTERS" && words[4].Time == 5,
            "Multi-syllable words should be rebuilt before measure assignment.");
    }

    private static void TestWordsAreNotSplitAtBarLines() {
        // A word crossing a measure boundary must be assigned to a single
        // measure, otherwise it would read as two partial words.
        TrackChunk track = new(
            new TextEvent("AP"),
            new TextEvent("PA") { DeltaTime = 480 },
            new TextEvent("RAITRE ") { DeltaTime = 480 },
            new TextEvent("SUIVANT ") { DeltaTime = 480 }
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(new[] { track });
        List<MidiLyricsExtractor.Lyric> words = MidiLyricsExtractor.GroupWords(lyrics);

        Expect(words.Count == 2, "Two words are expected.");
        Expect(words[0].Text == "APPARAITRE" && words[0].Time == 0,
            "A word crossing a bar line must stay whole and keep its attack time.");
        Expect(words[1].Text == "SUIVANT" && words[1].Time == 1440,
            "The following word should keep its own time.");
    }

    private static void TestWhitespaceAndControlCharactersAreCleaned() {
        TrackChunk track = new(
            new LyricEvent("  Hello\r\n"),
            new LyricEvent("   ") { DeltaTime = 480 },
            new LyricEvent("wor\tld\u00A0") { DeltaTime = 480 }
        );

        List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(new[] { track });

        Expect(lyrics.Count == 2, "Empty lyric events must be discarded.");
        Expect(lyrics[0].Text.Trim() == "Hello", "Control characters should be removed.");
        Expect(lyrics[1].Text.Trim() == "wor ld", "Inner whitespace should be collapsed.");
        Expect(MidiLyricsExtractor.Format(lyrics) == "Hello wor ld",
            "The formatted lyric should be trimmed and readable.");
    }

    private static void Expect(bool condition, string message) {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}
