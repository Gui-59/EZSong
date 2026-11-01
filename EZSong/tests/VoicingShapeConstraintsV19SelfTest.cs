using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;

namespace EZSong.Tests;

/// <summary>
/// A static class that contains self-tests for the VoicingShapeConstraintsV19 functionality, 
/// ensuring that the voicing engine respects the configured constraints for one-hand and two-hand voicings.
/// </summary>
public static class VoicingShapeConstraintsV19SelfTest {

    /// <summary>
    /// Runs all self-tests for the VoicingShapeConstraintsV19 functionality, 
    /// verifying that the voicing engine behaves correctly under various constraint configurations.
    /// </summary>
    public static void RunAll() {
        TestDefaultsPreserveExistingBehavior();
        TestOneHandSpanConstraint();
        TestOneHandSpacingConstraint();
        TestTwoHandSpanConstraint();
    }

    /// <summary>
    /// Tests that the default settings of the voicing engine preserve existing behavior,
    /// </summary>
    private static void TestDefaultsPreserveExistingBehavior() {
        VoicingSettings settings = new();
        Expect(settings.MaxVoicingSpanSemitones == 0, "Maximum span should be disabled by default.");
        Expect(settings.MinimumNoteSpacingSemitones == 0, "Minimum spacing should be disabled by default.");
        _ = new ChordVoicingEngine(settings).GenerateFirst(new ChordVoicingRequest(new Chord()));
    }
    
    /// <summary>
    /// Tests that the one-hand voicing respects the configured maximum span constraint.
    /// </summary>
    private static void TestOneHandSpanConstraint() {
        VoicingSettings settings = new() {
            MinChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
            MaxChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
            MaxVoicingSpanSemitones = 12
        };
        ChordVoicingEngine engine = new(settings);
        ChordVoicing voicing = engine.GenerateFirst(new ChordVoicingRequest(new Chord()));
        int span = voicing.Pitches.Max()!.ToMidiNoteNumber() - voicing.Pitches.Min()!.ToMidiNoteNumber();
        Expect(span <= 12, "One-hand voicing should respect the configured maximum span.");
    }

    /// <summary>
    /// Tests that the one-hand voicing respects the configured minimum spacing constraint between notes.
    /// </summary>
    private static void TestOneHandSpacingConstraint() {
        VoicingSettings settings = new() {
            MinChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
            MaxChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
            MinimumNoteSpacingSemitones = 3
        };
        ChordVoicingEngine engine = new(settings);
        ChordVoicing voicing = engine.GenerateFirst(new ChordVoicingRequest(new Chord()));
        int[] notes = voicing.Pitches.Select(p => p.ToMidiNoteNumber()).OrderBy(n => n).ToArray();
        Expect(notes.Zip(notes.Skip(1), (a, b) => b - a).All(d => d >= 3),
            "One-hand voicing should respect minimum spacing.");
    }

    /// <summary>
    /// Tests that the two-hand voicing respects the configured maximum span and minimum spacing constraints.
    /// </summary>
    private static void TestTwoHandSpanConstraint() {
        TwoHandVoicingSettings settings = new() {
            MaxVoicingSpanSemitones = 24,
            MinimumNoteSpacingSemitones = 2
        };
        TwoHandVoicingEngine engine = new(settings);
        TwoHandChordVoicing voicing = engine.GenerateFirst(new ChordVoicingRequest(new Chord()));
        int[] notes = voicing.Pitches.Select(p => p.ToMidiNoteNumber()).OrderBy(n => n).ToArray();
        Expect(notes[^1] - notes[0] <= 24, "Two-hand voicing should respect maximum span.");
        Expect(notes.Zip(notes.Skip(1), (a, b) => b - a).All(d => d >= 2),
            "Two-hand voicing should respect minimum spacing.");
    }

    /// <summary>
    /// Method to assert that a condition is true, throwing an InvalidOperationException with the provided message if it is not.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The message to include in the exception if the condition is false.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void Expect(bool condition, string message) {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}
