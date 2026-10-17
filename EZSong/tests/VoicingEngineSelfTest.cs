using EZSong.Enums;
using EZSong.EnumsStringifier;
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
        CToBWithNoCommonNotesIsValid();
        CommonTransitionsAreValid();
        SlashChordKeepsRequestedBass();
        VoiceCountCanIncreaseAndDecrease();
        LargeVoiceMovementIsRejected();
        ExactFirstVoicingIsPreserved();
        ChordProgressionParserWorks();
        VoicingPreferenceJsonWorks();
        VoiceIdentityIsExplicit();
        IntelligentExtendedChordReduction();
        SlashBassSurvivesExtendedChordReduction();

        Console.WriteLine("VoicingEngineSelfTest: ALL OK");
    }

    private static void IntelligentExtendedChordReduction()
    {
        Chord c13 = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.C, Alteration.neutral, ChordType.Thirteenth);

        ChordVoicingEngine richEngine = new(new VoicingSettings
        {
            MinChordAreaNote = P(NoteStep.C, 1),
            MaxChordAreaNote = P(NoteStep.C, 3),
            ChordMaxKeyCount = 5,
            ChordTonePriorityPolicy = ChordTonePriorityPolicy.EssentialsThenExtensions
        });

        ChordVoicing rich = richEngine.GenerateFirst(new ChordVoicingRequest(c13));
        int[] expectedRich = { 0, 2, 4, 9, 10 };
        Expect(rich.Pitches.Select(p => p.ToMidiNoteNumber() % 12).OrderBy(x => x).SequenceEqual(expectedRich),
            $"C13 intelligent reduction should keep root, 3rd, 7th, 9th and 13th: {Format(rich)}");

        ChordVoicingEngine traditionalEngine = new(new VoicingSettings
        {
            MinChordAreaNote = P(NoteStep.C, 1),
            MaxChordAreaNote = P(NoteStep.C, 3),
            ChordMaxKeyCount = 5,
            ChordTonePriorityPolicy = ChordTonePriorityPolicy.EssentialsThenFifth
        });

        ChordVoicing traditional = traditionalEngine.GenerateFirst(new ChordVoicingRequest(c13));
        int[] expectedTraditional = { 0, 2, 4, 7, 10 };
        Expect(traditional.Pitches.Select(p => p.ToMidiNoteNumber() % 12).OrderBy(x => x).SequenceEqual(expectedTraditional),
            $"C13 traditional reduction should keep the 5th before the 13th: {Format(traditional)}");

        Console.WriteLine("V14: intelligent extended-chord omission policy OK");
    }

    private static void SlashBassSurvivesExtendedChordReduction()
    {
        Chord c13OverG = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.C, Alteration.neutral, ChordType.Thirteenth, NoteStep.G);
        ChordVoicingEngine engine = CreateEngine();
        ChordVoicing voicing = engine.GenerateFirst(new ChordVoicingRequest(c13OverG));

        Expect(voicing.Pitches[0].ToMidiNoteNumber() % 12 == 7,
            $"C13/G must keep G as the lowest note after tone reduction: {Format(voicing)}");
        Expect(voicing.Pitches.Count == 5, "C13/G should still use the configured five voices.");

        Console.WriteLine("V14: slash bass preservation during reduction OK");
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

    private static void VoiceIdentityIsExplicit()
    {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);
        Chord am = Minor(NoteStep.A);

        ChordVoicing first = new(c, new[] { P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2) });
        ChordVoicing next = new(am, new[] { P(NoteStep.E, 1), P(NoteStep.A, 1), P(NoteStep.C, 2) });

        VoicingTransitionAnalysis analysis = engine.AnalyzeTransition(
            first, new ChordVoicingRequest(am), next.Pitches);

        Expect(analysis.IsValid, "C -> Am transition should be valid.");
        Expect(analysis.VoiceMovements.Count == 3, "Expected three explicit voice movements.");
        Expect(analysis.VoiceMovements.Count(v => v.IsContinued) == 3, "All three voices should continue.");
        Expect(analysis.VoiceMovements.Any(v => v.PreviousNote?.Equals(P(NoteStep.E, 1)) == true && v.CurrentNote?.Equals(P(NoteStep.E, 1)) == true),
            "E1 should remain E1.");
        Expect(analysis.VoiceMovements.Any(v => v.PreviousNote?.Equals(P(NoteStep.G, 1)) == true && v.CurrentNote?.Equals(P(NoteStep.A, 1)) == true),
            "G1 should move to A1.");
        Expect(analysis.VoiceMovements.Any(v => v.PreviousNote?.Equals(P(NoteStep.C, 2)) == true && v.CurrentNote?.Equals(P(NoteStep.C, 2)) == true),
            "C2 should remain C2.");
        Console.WriteLine("Voice identity tracking: C -> Am OK");
    }

    private static void ExactFirstVoicingIsPreserved()
    {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);
        ChordVoicing exact = new(c, new[] { P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2) });
        ChordVoicing first = engine.GenerateFirst(ChordVoicingRequest.FromExactVoicing(exact));

        Expect(first == exact, $"Exact first voicing ({Format(exact)}) was not preserved: {Format(first)}");
    }

    private static void VoicingPreferenceJsonWorks()
    {
        string path = Path.Combine(Path.GetTempPath(), $"EZSong-voicing-preferences-{Guid.NewGuid():N}.json");

        try
        {
            Chord c = Major(NoteStep.C);
            Chord f = Major(NoteStep.F);

            VoicingPreferenceStore original = new();
            original.Preferences.Add(new VoicingPreference
            {
                FirstChord = new ChordVoicing(c, new[]
                {
                    P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2)
                }),
                SecondChord = new ChordVoicing(f, new[]
                {
                    P(NoteStep.A, 1), P(NoteStep.C, 2), P(NoteStep.F, 2)
                })
            });

            VoicingPreferenceJson.Save(path, original);
            VoicingPreferenceStore loaded = VoicingPreferenceJson.Load(path);

            Expect(loaded.Preferences.Count == 1, "One voicing preference should survive JSON round-trip.");

            VoicingSettings settings = CreateSettings();
            settings.UserPreferenceWeight = 1000;
            ChordVoicingEngine engine = new(settings, loaded);

            ChordVoicing first = engine.GenerateFirst(
                ChordVoicingRequest.FromExactVoicing(
                    c, P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2)));

            ChordVoicing preferred = engine.GenerateNext(new ChordVoicingRequest(f), first);
            ChordVoicing expected = new(f, new[]
            {
                P(NoteStep.A, 1), P(NoteStep.C, 2), P(NoteStep.F, 2)
            });

            Expect(preferred == expected,
                $"JSON voicing preference should be applied: expected {Format(expected)}, got {Format(preferred)}");

            // Chord instances are parsed independently, so this also verifies
            // that preference matching is structural rather than reference-based.
            Chord parsedC = ChordProgressionParser.ParseChord("C");
            Chord parsedF = ChordProgressionParser.ParseChord("F");
            Expect(loaded.Find(parsedC, parsedF).Any(),
                "Preference lookup should work with independently parsed Chord instances.");

            Console.WriteLine("Voicing preferences: JSON round-trip + structural matching OK");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
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
