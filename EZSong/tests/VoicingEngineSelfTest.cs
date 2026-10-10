using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;

namespace EZSong.Tests;

/// <summary>
/// Framework-free smoke tests for the V1 voicing engine.
/// Call <see cref="Run"/> from a temporary console entry point while tuning
/// the musical scoring rules.
/// </summary>
public static class VoicingEngineSelfTest
{
    public static void RunAll() {
        CToAmKeepsTwoKeys();
        SlashChordKeepsRequestedBass();
        LargeVoiceMovementIsRejected();
        ExactFirstVoicingIsPreserved();

        Console.WriteLine("VoicingEngineSelfTest: OK");
    }

    private static void CToAmKeepsTwoKeys()
    {
        ChordVoicingEngine engine = CreateEngine();

        Chord c = Major(NoteStep.C);
        Chord am = Minor(NoteStep.A);

        ChordVoicing first = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                c,
                P(NoteStep.E, 1),
                P(NoteStep.G, 1),
                P(NoteStep.C, 2)));

        ChordVoicing next = engine.GenerateNext(
            new ChordVoicingRequest(am),
            first);

        Expect(
            next.MidiNotes.OrderBy(x => x).SequenceEqual(new[] { 28, 33, 36 }),
            $"C -> Am expected E1 A1 C2, got {Format(next)}");
    }

    private static void SlashChordKeepsRequestedBass()
    {
        ChordVoicingEngine engine = CreateEngine();

        Chord c = Major(NoteStep.C);
        Chord gOverB = new(
            false,
            new RhythmRationalDuration(1, 1, 0),
            NoteStep.G,
            Alteration.neutral,
            ChordType.NoneOrMajor,
            NoteStep.B);

        ChordVoicing first = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                c,
                P(NoteStep.E, 1),
                P(NoteStep.G, 1),
                P(NoteStep.C, 2)));

        ChordVoicing next = engine.GenerateNext(
            new ChordVoicingRequest(gOverB),
            first);

        Expect(
            next.MidiNotes.Min() % 12 == 11,
            $"G/B expected B as the lowest note, got {Format(next)}");
    }

    private static void LargeVoiceMovementIsRejected()
    {
        VoicingSettings settings = CreateSettings();
        settings.MinChordNote = new Model.Pitch(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 3);
        settings.MaxChordNote = new Model.Pitch(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 5);
        settings.MaxVoiceMovement = 11;

        ChordVoicingEngine engine = new(settings);

        Chord c = Major(NoteStep.C);
        Chord am = Minor(NoteStep.A);

        // This test is deliberately structural: the engine must never return
        // a transition in which an existing voice has to move an octave or more.
        // The returned voicing is checked against the previous keys.
        ChordVoicing first = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                c,
                P(NoteStep.E, 3),
                P(NoteStep.G, 3),
                P(NoteStep.C, 4)));

        ChordVoicing next = engine.GenerateNext(
            new ChordVoicingRequest(am),
            first);

        foreach (int previous in first.MidiNotes)
        {
            bool hasCloseSuccessor = next.MidiNotes.Any(current =>
                Math.Abs(previous - current) <= settings.MaxVoiceMovement);

            Expect(
                hasCloseSuccessor,
                $"Voice at MIDI {previous} has no successor within one octave: {Format(next)}");
        }
    }

    private static void ExactFirstVoicingIsPreserved()
    {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);

        ChordVoicing first = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                c,
                P(NoteStep.E, 1),
                P(NoteStep.G, 1),
                P(NoteStep.C, 2)));

        Expect(
            first.MidiNotes.OrderBy(x => x).SequenceEqual(new[] { 28, 31, 36 }),
            $"Exact first voicing was not preserved: {Format(first)}");
    }

    private static VoicingSettings CreateSettings() {
        return new() {
            MinChordNote = new Model.Pitch(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 1), 
            MaxChordNote = new Model.Pitch(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 3),
            MaxVoiceMovement = 11,
            CommonNoteWeight = 100,
            MovementWeight = 1,
            BassMovementWeight = 3,
            UserPreferenceWeight = 75,
            MaxVoices = 5
        };
    }

    private static ChordVoicingEngine CreateEngine() {
        return new(CreateSettings());
    }

    /// <summary>
    /// Just a convenience method to create a major chord with the given root note.
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    private static Chord Major(NoteStep root) {
        return new(
        false,
        new RhythmRationalDuration(1, 1, 0),
        root,
        Alteration.neutral,
        ChordType.NoneOrMajor);
    }

    /// <summary>
    /// Just a convenience method to create a minor chord with the given root note.
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    private static Chord Minor(NoteStep root) {
        return new(
        false,
        new RhythmRationalDuration(1, 1, 0),
        root,
        Alteration.neutral,
        ChordType.Minor);
    }

    private static Pitch P(NoteStep note, int octave) {
        return new(note, Alteration.neutral, octave);
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }

    private static string Format(ChordVoicing voicing) {
        return $"{voicing.Chord.ToHumanString()} [{string.Join(", ", voicing.MidiNotes.OrderBy(x => x))}]";
    }
}
