using EZSong.MIDI;
using EZSong.Model;

namespace EZSong.Tests;

/// <summary>
/// A static class that contains self-tests for the VoicingProgression functionality, 
/// including one-hand and two-hand voicing tests.
/// </summary>
public static class VoicingProgressionSelfTest {

    /// <summary>
    /// Runs all self-tests for the VoicingProgression functionality,
    /// </summary>
    public static void RunAll()    {
        TestOneHandResult();
        TestTwoHandResult();
    }

    /// <summary>
    /// Tests the one-hand voicing progression result by parsing a chord progression and verifying the expected properties of the resulting VoicingProgression.
    /// </summary>
    private static void TestOneHandResult() {
        ChordProgressionParser parser = new(new ChordVoicingEngine(
            VoicingProfile.ArrangerLeftHand.OneHand));

        VoicingProgression result = parser.ParseToVoicingProgression("""
            | C   4 |
            | G/B 4 |
            | Am  2 |
            | F   2 |
            | G7  4 |
            | C   4 |
            """);

        Expect(result.Entries.Count == 6, "V17 one-hand result should contain six entries.");
        Expect(result.Entries[0].MeasureNumber == 1, "First entry should be measure 1.");
        Expect(result.Entries[0].Duration.GetEffectiveDuration().Equals(new RhythmRationalDuration(1, 4, 0)), "First duration should be a quarter note.");
        Expect(result.Entries[0].VoiceMovements.Count == 0, "First entry should have no incoming voice movement.");
        Expect(result.VoicePaths.Count > 0, "One-hand result should expose persistent voice paths.");
        Expect(result.Entries[2].VoicePaths.Count == result.VoicePaths.Count, "Entries should expose the same progression voice paths.");
    }

    /// <summary>
    /// Tests the two-hand voicing progression result by parsing a chord progression 
    /// and verifying the expected properties of the resulting TwoHandVoicingProgression.
    /// </summary>
    private static void TestTwoHandResult()    {
        TwoHandVoicingSettings settings = VoicingProfile.TwoHandsBalanced.TwoHand;
        TwoHandVoicingEngine engine = new(settings);
        ChordProgressionParser parser = new();

        TwoHandVoicingProgression result = parser.ParseToTwoHandVoicingProgression("""
            | C  4 |
            | Am 4 |
            | F  4 |
            """, engine);

        Expect(result.Entries.Count == 3, "V17 two-hand result should contain three entries.");
        Expect(result.Entries[0].LeftHandVoiceMovements.Count == 0, "First two-hand entry should have no incoming LH movement.");
        Expect(result.Entries[0].RightHandVoiceMovements.Count == 0, "First two-hand entry should have no incoming RH movement.");
        Expect(result.LeftHandVoicePaths.Count > 0, "Two-hand result should expose LH voice paths.");
        Expect(result.RightHandVoicePaths.Count > 0, "Two-hand result should expose RH voice paths.");
    }

    /// <summary>
    /// Method to assert a condition and throw an InvalidOperationException with a specified message if the condition is false.
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
