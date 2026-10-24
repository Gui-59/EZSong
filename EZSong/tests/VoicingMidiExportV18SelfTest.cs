using EZSong.MIDI;

namespace EZSong.Tests;

/// <summary>Regression tests for V18 progression-based MIDI export.</summary>
public static class VoicingMidiExportV18SelfTest
{
    public static void RunAll()
    {
        string oneHandPath = Path.Combine(Path.GetTempPath(), "EZSong-V18-one-hand.mid");
        string twoHandPath = Path.Combine(Path.GetTempPath(), "EZSong-V18-two-hand.mid");
        string voiceTrackPath = Path.Combine(Path.GetTempPath(), "EZSong-V18-voices.mid");

        var parser = new ChordProgressionParser(new ChordVoicingEngine(
            VoicingProfile.ArrangerLeftHand.OneHand));
        VoicingProgression oneHand = parser.ParseToVoicingProgression("""
            | C   4 |
            | G/B 4 |
            | Am  2 |
            | F   2 |
            | G7  4 |
            | C   4 |
            """);

        var settings = new MidiChordExportSettings();
        MidiChordExporter.Export(oneHand, oneHandPath, settings);
        Expect(File.Exists(oneHandPath), "V18 one-hand MIDI file should exist.");

        settings.CreateVoiceTracks = true;
        MidiChordExporter.Export(oneHand, voiceTrackPath, settings);
        Expect(File.Exists(voiceTrackPath), "V18 voice-track MIDI file should exist.");

        var twoHandParser = new ChordProgressionParser();
        TwoHandVoicingProgression twoHand = twoHandParser.ParseToTwoHandVoicingProgression("""
            | C  4 |
            | Am 4 |
            | F  4 |
            """, new TwoHandVoicingEngine(VoicingProfile.TwoHandsBalanced.TwoHand));
        MidiChordExportSettings twoHandSettings = new() { CreateVoiceTracks = true };
        TwoHandMidiExporter.Export(twoHand, twoHandPath, twoHandSettings);
        Expect(File.Exists(twoHandPath), "V18 two-hand MIDI file should exist.");

        File.Delete(oneHandPath);
        File.Delete(twoHandPath);
        File.Delete(voiceTrackPath);
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
