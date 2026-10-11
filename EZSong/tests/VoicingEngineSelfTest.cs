using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;

namespace EZSong.Tests;

/// <summary>
/// Framework-free musical regression tests for the voicing engine.
/// Call <see cref="RunAll"/> from a small console project.
/// </summary>
public static class VoicingEngineSelfTest
{
    public static void RunAll()
    {
        CToAmKeepsTwoKeys();
        CommonTransitionsAreValid();
        SlashChordKeepsRequestedBass();
        VoiceCountCanIncreaseAndDecrease();
        LargeVoiceMovementIsRejected();
        ExactFirstVoicingIsPreserved();

        Console.WriteLine("VoicingEngineSelfTest: ALL OK");
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

        ChordVoicing next = engine.GenerateNext(new ChordVoicingRequest(am), first);
        ChordVoicing expected = new(am, new[] { P(NoteStep.E, 1), P(NoteStep.A, 1), P(NoteStep.C, 2) });

        Expect(next == expected, $"C -> Am : expected {Format(expected)}, got {Format(next)}");
        PrintTransition(first, next, engine);
    }

    private static void CommonTransitionsAreValid()
    {
        ChordVoicingEngine engine = CreateEngine();
        ChordVoicing current = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                Major(NoteStep.C),
                P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2)));

        (string Name, Chord Chord)[] targets =
        [
            ("F", Major(NoteStep.F)),
            ("G", Major(NoteStep.G)),
            ("Am", Minor(NoteStep.A)),
            ("F", Major(NoteStep.F)),
            ("G7", Seventh(NoteStep.G)),
            ("C", Major(NoteStep.C)),
            ("Dm", Minor(NoteStep.D)),
            ("Am", Minor(NoteStep.A))
        ];

        foreach ((string _, Chord chord) target in targets)
        {
            ChordVoicing next = engine.GenerateNext(new ChordVoicingRequest(target.chord), current);
            VoicingTransitionAnalysis analysis = engine.AnalyzeTransition(
                current,
                new ChordVoicingRequest(target.chord),
                next.Pitches);

            Expect(analysis.IsValid, $"Invalid transition {Format(current)} -> {Format(next)}");
            Expect(next.Pitches.Count is >= 2 and <= 5, $"Invalid voice count: {Format(next)}");
            Expect(next.Pitches.Zip(next.Pitches.Skip(1), (a, b) => b > a).All(x => x),
                $"Voicing is not strictly ascending: {Format(next)}");

            PrintTransition(current, next, engine);
            current = next;
        }
    }

    private static void SlashChordKeepsRequestedBass()
    {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);
        Chord gOverB = Major(NoteStep.G, NoteStep.B);

        ChordVoicing first = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                c, P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2)));

        ChordVoicing next = engine.GenerateNext(new ChordVoicingRequest(gOverB), first);

        Expect(next.Pitches[0].ToMidiNoteNumber() % 12 == 11,
            $"G/B : expected B as the lowest note, got {Format(next)}");
        PrintTransition(first, next, engine);
    }

    private static void VoiceCountCanIncreaseAndDecrease()
    {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);
        Chord am7 = MinorSeventh(NoteStep.A);
        Chord cmaj7 = MajorSeventh(NoteStep.C);

        ChordVoicing first = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                c, P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2)));

        ChordVoicing four = engine.GenerateNext(new ChordVoicingRequest(am7), first);
        Expect(four.Pitches.Count == 4, $"C -> Am7 should produce 4 notes: {Format(four)}");
        PrintTransition(first, four, engine);

        ChordVoicing three = engine.GenerateNext(new ChordVoicingRequest(cmaj7), four);
        Expect(three.Pitches.Count == 4, $"Am7 -> Cmaj7 should produce 4 notes: {Format(three)}");
        PrintTransition(four, three, engine);

        ChordVoicing backToThree = engine.GenerateNext(new ChordVoicingRequest(c), three);
        Expect(backToThree.Pitches.Count == 3, $"Cmaj7 -> C should produce 3 notes: {Format(backToThree)}");
        PrintTransition(three, backToThree, engine);
    }

    private static void LargeVoiceMovementIsRejected()
    {
        VoicingSettings settings = CreateSettings();
        settings.MinChordAreaNote = P(NoteStep.C, 3);
        settings.MaxChordAreaNote = P(NoteStep.C, 5);
        settings.MaxVoiceMovement = 11;

        ChordVoicingEngine engine = new(settings);
        Chord c = Major(NoteStep.C);
        Chord am = Minor(NoteStep.A);

        ChordVoicing first = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                c, P(NoteStep.E, 3), P(NoteStep.G, 3), P(NoteStep.C, 4)));

        ChordVoicing next = engine.GenerateNext(new ChordVoicingRequest(am), first);
        VoicingTransitionAnalysis analysis = engine.AnalyzeTransition(
            first, new ChordVoicingRequest(am), next.Pitches);

        Expect(analysis.IsValid, $"Transition contains a forbidden octave jump: {Format(next)}");
        Expect(analysis.TotalMovement <= 33,
            $"Unexpectedly large total movement: {analysis.TotalMovement} in {Format(next)}");
        PrintTransition(first, next, engine);
    }

    private static void ExactFirstVoicingIsPreserved()
    {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);
        ChordVoicing exact = new(c, new[] { P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2) });
        ChordVoicing first = engine.GenerateFirst(ChordVoicingRequest.FromExactVoicing(exact));

        Expect(first == exact, $"Exact first voicing ({Format(exact)}) was not preserved: {Format(first)}");
    }

    private static void PrintTransition(ChordVoicing from, ChordVoicing to, ChordVoicingEngine engine)
    {
        VoicingTransitionAnalysis analysis = engine.AnalyzeTransition(
            from,
            new ChordVoicingRequest(to.Chord),
            to.Pitches);

        string common = analysis.CommonNotes.Count == 0
            ? "-"
            : string.Join(" ", analysis.CommonNotes.Select(p => p.ToHumanString()));

        Console.WriteLine(
            $"{from.Chord.ToHumanString(),-5} {FormatNotes(from),-16} -> " +
            $"{to.Chord.ToHumanString(),-5} {FormatNotes(to),-16} " +
            $"common=[{common}] move={analysis.TotalMovement} bass={analysis.BassMovement} score={analysis.Score}");
    }

    private static string FormatNotes(ChordVoicing voicing) {
        return "[" + string.Join(" ", voicing.Pitches.Select(p => p.ToHumanString())) + "]";
    }

    private static VoicingSettings CreateSettings() {
        return new() {
            MinChordAreaNote = P(NoteStep.C, 1),
            MaxChordAreaNote = P(NoteStep.C, 3),
            MaxVoiceMovement = 11,
            CommonNoteWeight = 100,
            MovementWeight = 1,
            BassMovementWeight = 3,
            UserPreferenceWeight = 75,
            ChordMaxKeyCount = 5
        };
    }

    private static ChordVoicingEngine CreateEngine() {
        return new(CreateSettings());
    }

    private static Chord Major(NoteStep root, NoteStep? bass = null) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.NoneOrMajor, bass);
    }

    private static Chord Minor(NoteStep root) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.Minor);
    }

    private static Chord Seventh(NoteStep root) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.Seventh);
    }

    private static Chord MinorSeventh(NoteStep root) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.MinorSeventh);
    }

    private static Chord MajorSeventh(NoteStep root) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.MajorSeventh);
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
        return $"{voicing.Chord.ToHumanString()} [{string.Join(" ", voicing.Pitches.Select(p => p.ToHumanString()))}]";
    }
}
