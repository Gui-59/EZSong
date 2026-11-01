using EZSong.MIDI;

namespace EZSong.Tests;

/// <summary>
/// A self-test class for verifying the functionality of the Voicing MIDI export feature in version 18 of the EZSong application.
/// </summary>
public static class VoicingMidiExportV18SelfTest {

    /// <summary>
    /// Runs all self-tests for the Voicing MIDI export feature, 
    /// including one-hand and two-hand voicing progressions, 
    /// and verifies that the corresponding MIDI files are created successfully.
    /// </summary>
    public static void RunAll() {
        string oneHandPath = Path.Combine(Path.GetTempPath(), "EZSong-V18-one-hand.mid");
        string twoHandPath = Path.Combine(Path.GetTempPath(), "EZSong-V18-two-hand.mid");
        string voiceTrackPath = Path.Combine(Path.GetTempPath(), "EZSong-V18-voices.mid");

        ChordProgressionParser parser = new(new ChordVoicingEngine(VoicingProfile.ArrangerLeftHand.OneHand));
        VoicingProgression oneHand = parser.ParseToVoicingProgression("""
            | C   4 |
            | G/B 4 |
            | Am  2 |
            | F   2 |
            | G7  4 |
            | C   4 |
            """);

        MidiChordExportSettings settings = new();
        MidiChordExporter.Export(oneHand, oneHandPath, settings);
        Expect(File.Exists(oneHandPath), "V18 one-hand MIDI file should exist.");

        settings.CreateVoiceTracks = true;
        MidiChordExporter.Export(oneHand, voiceTrackPath, settings);
        Expect(File.Exists(voiceTrackPath), "V18 voice-track MIDI file should exist.");

        ChordProgressionParser twoHandParser = new();
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

    /// <summary>
    /// A helper method to assert that a condition is true, throwing an InvalidOperationException with a specified message if the condition is false.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="message"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void Expect(bool condition, string message) {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}
