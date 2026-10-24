using EZSong.MIDI;
using EZSong.Model;

namespace EZSong.Tests;

/// <summary>Small regression tests for the V17 musical result layer.</summary>
public static class VoicingProgressionSelfTest
{
    public static void RunAll()
    {
        TestOneHandResult();
        TestTwoHandResult();
    }

    private static void TestOneHandResult()
    {
        var parser = new ChordProgressionParser(new ChordVoicingEngine(
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

    private static void TestTwoHandResult()
    {
        var settings = VoicingProfile.TwoHandsBalanced.TwoHand;
        var engine = new TwoHandVoicingEngine(settings);
        var parser = new ChordProgressionParser();

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

    private static void Expect(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
