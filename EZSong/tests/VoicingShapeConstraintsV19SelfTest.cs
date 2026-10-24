using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;

namespace EZSong.Tests;

/// <summary>Regression tests for optional V19 voicing-shape constraints.</summary>
public static class VoicingShapeConstraintsV19SelfTest
{
    public static void RunAll()
    {
        TestDefaultsPreserveExistingBehavior();
        TestOneHandSpanConstraint();
        TestOneHandSpacingConstraint();
        TestTwoHandSpanConstraint();
    }

    private static void TestDefaultsPreserveExistingBehavior()
    {
        var settings = new VoicingSettings();
        Expect(settings.MaxVoicingSpanSemitones == 0, "Maximum span should be disabled by default.");
        Expect(settings.MinimumNoteSpacingSemitones == 0, "Minimum spacing should be disabled by default.");
        _ = new ChordVoicingEngine(settings).GenerateFirst(new ChordVoicingRequest(new Chord()));
    }

    private static void TestOneHandSpanConstraint()
    {
        var settings = new VoicingSettings
        {
            MinChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
            MaxChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
            MaxVoicingSpanSemitones = 12
        };
        var engine = new ChordVoicingEngine(settings);
        var voicing = engine.GenerateFirst(new ChordVoicingRequest(new Chord()));
        int span = voicing.Pitches.Max()!.ToMidiNoteNumber() - voicing.Pitches.Min()!.ToMidiNoteNumber();
        Expect(span <= 12, "One-hand voicing should respect the configured maximum span.");
    }

    private static void TestOneHandSpacingConstraint()
    {
        var settings = new VoicingSettings
        {
            MinChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
            MaxChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
            MinimumNoteSpacingSemitones = 3
        };
        var engine = new ChordVoicingEngine(settings);
        var voicing = engine.GenerateFirst(new ChordVoicingRequest(new Chord()));
        int[] notes = voicing.Pitches.Select(p => p.ToMidiNoteNumber()).OrderBy(n => n).ToArray();
        Expect(notes.Zip(notes.Skip(1), (a, b) => b - a).All(d => d >= 3),
            "One-hand voicing should respect minimum spacing.");
    }

    private static void TestTwoHandSpanConstraint()
    {
        var settings = new TwoHandVoicingSettings
        {
            MaxVoicingSpanSemitones = 24,
            MinimumNoteSpacingSemitones = 2
        };
        var engine = new TwoHandVoicingEngine(settings);
        var voicing = engine.GenerateFirst(new ChordVoicingRequest(new Chord()));
        int[] notes = voicing.Pitches.Select(p => p.ToMidiNoteNumber()).OrderBy(n => n).ToArray();
        Expect(notes[^1] - notes[0] <= 24, "Two-hand voicing should respect maximum span.");
        Expect(notes.Zip(notes.Skip(1), (a, b) => b - a).All(d => d >= 2),
            "Two-hand voicing should respect minimum spacing.");
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
