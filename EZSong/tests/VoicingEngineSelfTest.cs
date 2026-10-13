using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;
using Melanchall.DryWetMidi.Core;

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
        CToBWithNoCommonNotesIsValid();
        CommonTransitionsAreValid();
        SlashChordKeepsRequestedBass();
        VoiceCountCanIncreaseAndDecrease();
        LargeVoiceMovementIsRejected();
        ExactFirstVoicingIsPreserved();
        ChordProgressionParserWorks();
        ChordMidiGeneratorExportsGlobalVoicingAndRhythm();

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

    private static void CToBWithNoCommonNotesIsValid()
    {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);
        Chord b = Major(NoteStep.B);

        ChordVoicing first = engine.GenerateFirst(
            ChordVoicingRequest.FromExactVoicing(
                c,
                P(NoteStep.E, 1),
                P(NoteStep.G, 1),
                P(NoteStep.C, 2)));

        ChordVoicing next = engine.GenerateNext(new ChordVoicingRequest(b), first);
        VoicingTransitionAnalysis analysis = engine.AnalyzeTransition(
            first,
            new ChordVoicingRequest(b),
            next.Pitches);

        Expect(analysis.IsValid, $"C -> B should be valid even with no common notes: {Format(next)}");
        Expect(analysis.CommonNotes.Count == 0,
            $"C -> B should have no common notes: {Format(next)}");
        Expect(analysis.TotalMovement <= 33,
            $"C -> B has an unexpectedly large total movement: {analysis.TotalMovement}");

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

    private static void ChordProgressionParserWorks()
    {
        ChordVoicingEngine engine = CreateEngine();
        ChordProgressionParser parser = new(engine);

        IReadOnlyList<ChordVoicing> voicings = parser.ParseToVoicings("""
| C   4 |
| G/B 4 |
| Am  4 |
| F   4 |
| G7  4 |
| C   4 |
""");

        Expect(voicings.Count == 6, $"Expected 6 voicings, got {voicings.Count}.");
        Expect(voicings[0].Chord.RootNote == NoteStep.C, "First chord should be C.");
        Expect(voicings[1].Chord.BassNote == NoteStep.B, "G/B should have B as bass.");
        Expect(voicings[1].Pitches[0].ToMidiNoteNumber() % 12 == 11, $"G/B should start on B: {Format(voicings[1])}");
        Expect(voicings[2].Chord.ChordType == ChordType.Minor, "Am should be parsed as minor.");
        Expect(voicings[4].Chord.ChordType == ChordType.Seventh, "G7 should be parsed as seventh.");
        Expect(voicings[0].Chord.Duration.Equals(new RhythmRationalDuration(1, 4, 0)), "C duration should be a quarter note.");
        Expect(voicings[1].Chord.Duration.Equals(new RhythmRationalDuration(1, 4, 0)), "G/B duration should be a quarter note.");

        RhythmSimpleElement dotted = ChordProgressionParser.ParseDuration("4.");
        Expect(dotted.GetEffectiveDuration().Equals(new RhythmRationalDuration(3, 8, 0)), "4. should represent a dotted quarter note.");

        bool missingBarsRejected = false;
        try
        {
            _ = parser.ParseToVoicings("C 4");
        }
        catch (FormatException)
        {
            missingBarsRejected = true;
        }
        Expect(missingBarsRejected, "A progression without measure bars should be rejected.");

        Console.WriteLine("Parser: bars + durations + C -> G/B -> Am -> F -> G7 -> C OK");

        Chord c69 = ChordProgressionParser.ParseChord("C6/9");
        Expect(c69.ChordType == ChordType.SixthNinth, "C6/9 should be parsed as sixth-ninth.");

        Chord bb = ChordProgressionParser.ParseChord("Bbmaj7");
        Expect(bb.RootNote == NoteStep.B && bb.RootNoteAlteration == Alteration.flat,
            "Bbmaj7 should parse the flat root.");

        Chord m7Upper = ChordProgressionParser.ParseChord("CM7");
        Expect(m7Upper.ChordType == ChordType.MajorSeventh, "CM7 should be parsed as major seventh.");

        Pitch[] exact = { P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2) };
        IReadOnlyList<ChordVoicing> exactProgression = parser.ParseToVoicings("|C 4 |\n|Am 4 |", exact);
        Expect(exactProgression[0].Pitches.SequenceEqual(exact), "Exact first voicing should be preserved by parser.");
    }

    private static void ChordMidiGeneratorExportsGlobalVoicingAndRhythm()
    {
        string outputPath = Path.Combine(
            Path.GetTempPath(),
            $"EZSong-voicing-test-{Guid.NewGuid():N}.mid");

        try
        {
            VoicingSettings settings = CreateSettings();
            ChordVoicingEngine engine = new(settings);
            ChordMidiGenerator generator = new(settings);

            Chord c = Major(NoteStep.C);
            Chord gOverB = Major(NoteStep.G, NoteStep.B);
            Chord am = Minor(NoteStep.A);

            c.Duration = new RhythmRationalDuration(1, 4, 0);
            gOverB.Duration = new RhythmRationalDuration(1, 2, 0);
            am.Duration = new RhythmRationalDuration(3, 8, 0);

            ChordVoicingRequest first = ChordVoicingRequest.FromExactVoicing(
                c,
                P(NoteStep.E, 1),
                P(NoteStep.G, 1),
                P(NoteStep.C, 2));

            IReadOnlyList<ChordVoicing> expected = engine.GenerateProgression(
            [
                first,
                new ChordVoicingRequest(gOverB),
                new ChordVoicingRequest(am)
            ]);

            IReadOnlyList<ChordVoicing> generated = generator.Generate(
                first,
                [
                    new ChordVoicingRequest(gOverB),
                    new ChordVoicingRequest(am)
                ],
                outputPath);

            Expect(generated.Count == 3, $"Expected 3 generated voicings, got {generated.Count}.");
            Expect(generated.SequenceEqual(expected),
                "ChordMidiGenerator should use the same global progression optimisation as ChordVoicingEngine.");

            Expect(File.Exists(outputPath), "MIDI output file was not created.");

            MidiFile midiFile = MidiFile.Read(outputPath);
            TrackChunk track = midiFile.GetTrackChunks().Single();
            List<(long Tick, bool On)> notes = new();
            long absoluteTick = 0;

            foreach (MidiEvent midiEvent in track.Events)
            {
                absoluteTick += midiEvent.DeltaTime;

                if (midiEvent is NoteOnEvent noteOn)
                {
                    notes.Add((absoluteTick, noteOn.Velocity > 0));
                }
                else if (midiEvent is NoteOffEvent noteOff)
                {
                    notes.Add((absoluteTick, false));
                }
            }

            int expectedNoteCount = generated.Sum(v => v.Pitches.Count);
            Expect(notes.Count == expectedNoteCount * 2,
                $"Expected {expectedNoteCount * 2} note events, got {notes.Count}.");

            int firstChordNoteCount = generated[0].Pitches.Count;
            int secondChordNoteCount = generated[1].Pitches.Count;
            int thirdChordNoteCount = generated[2].Pitches.Count;

            long firstEnd = MidiChordExporter.TicksPerQuarterNote;
            long secondEnd = firstEnd + MidiChordExporter.TicksPerQuarterNote * 2L;
            long thirdEnd = secondEnd + MidiChordExporter.TicksPerQuarterNote * 3L / 2L;

            int firstStarts = notes.Count(n => n.On && n.Tick == 0);
            int secondStarts = notes.Count(n => n.On && n.Tick == firstEnd);
            int thirdStarts = notes.Count(n => n.On && n.Tick == secondEnd);

            Expect(firstStarts == firstChordNoteCount,
                $"Expected {firstChordNoteCount} note-ons at tick 0, got {firstStarts}.");
            Expect(secondStarts == secondChordNoteCount,
                $"Expected {secondChordNoteCount} note-ons at tick {firstEnd}, got {secondStarts}.");
            Expect(thirdStarts == thirdChordNoteCount,
                $"Expected {thirdChordNoteCount} note-ons at tick {secondEnd}, got {thirdStarts}.");

            Expect(notes.Count(n => !n.On && n.Tick == firstEnd) == firstChordNoteCount,
                "The first chord should end after one quarter note.");
            Expect(notes.Count(n => !n.On && n.Tick == secondEnd) == secondChordNoteCount,
                "The second chord should end after two quarter notes.");
            Expect(notes.Count(n => !n.On && n.Tick == thirdEnd) == thirdChordNoteCount,
                "The third chord should end after a dotted quarter note.");

            Console.WriteLine("MIDI: global voicing + durations + note timing OK");
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
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
