using EZSong.Enums;
using EZSong.EnumsStringifier;
using EZSong.MIDI;
using EZSong.Model;

namespace EZSong.Tests;

/// <summary>
/// A static class that runs a series of self-tests for the VoicingEngine, 
/// verifying its functionality and correctness.
/// </summary>
public static class VoicingEngineSelfTest {

    /// <summary>
    /// Runs all self-tests for the VoicingEngine, 
    /// checking various chord transitions, voicing preferences, and other functionalities.
    /// </summary>
    public static void RunAll() {
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
        PersistentVoiceIdentityAcrossProgression();
        VoicingProfilesProvideIndependentPresets();

        Console.WriteLine("VoicingEngineSelfTest: ALL OK");
    }

    private static void IntelligentExtendedChordReduction() {
        Chord c13 = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.C, Alteration.neutral, ChordType.Thirteenth);

        ChordVoicingEngine richEngine = new(new VoicingSettings {
            MinChordAreaNote = P(NoteStep.C, 1),
            MaxChordAreaNote = P(NoteStep.C, 3),
            ChordMaxKeyCount = 5,
            ChordTonePriorityPolicy = ChordTonePriorityPolicy.EssentialsThenExtensions
        });

        ChordVoicing rich = richEngine.GenerateFirst(new ChordVoicingRequest(c13));
        int[] expectedRich = { 0, 2, 4, 9, 10 };
        Expect(rich.Pitches.Select(p => p.ToMidiNoteNumber() % 12).OrderBy(x => x).SequenceEqual(expectedRich),
            $"C13 intelligent reduction should keep root, 3rd, 7th, 9th and 13th: {Format(rich)}");

        ChordVoicingEngine traditionalEngine = new(new VoicingSettings {
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

    /// <summary>
    /// Tests that a slash chord with a specified bass note retains that bass note after the chord is reduced to fit within the configured number of voices.
    /// </summary>
    private static void SlashBassSurvivesExtendedChordReduction() {
        Chord c13OverG = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.C, Alteration.neutral, ChordType.Thirteenth, NoteStep.G);
        ChordVoicingEngine engine = CreateEngine();
        ChordVoicing voicing = engine.GenerateFirst(new ChordVoicingRequest(c13OverG));

        Expect(voicing.Pitches[0].ToMidiNoteNumber() % 12 == 7,
            $"C13/G must keep G as the lowest note after tone reduction: {Format(voicing)}");
        Expect(voicing.Pitches.Count == 5, "C13/G should still use the configured five voices.");

        Console.WriteLine("V14: slash bass preservation during reduction OK");
    }

    /// <summary>
    /// Tests that voice identities are preserved across a progression of chords, even when new voices are added.
    /// </summary>
    private static void PersistentVoiceIdentityAcrossProgression() {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);
        Chord am = Minor(NoteStep.A);
        Chord am7 = MinorSeventh(NoteStep.A);

        IReadOnlyList<ChordVoicing> progression = engine.GenerateProgression(new[]
        {
            ChordVoicingRequest.FromExactVoicing(c, P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2)),
            new ChordVoicingRequest(am),
            new ChordVoicingRequest(am7)
        });

        VoiceTrackingResult tracking = engine.TrackVoices(progression);
        Expect(tracking.Voicings.Count == 3, "V15 should track every chord in the progression.");
        Expect(tracking.Voicings[0].Voices.Select(v => v.VoiceId).SequenceEqual(new[] { 1, 2, 3 }),
            "The first chord should establish voices 1, 2 and 3.");

        int eVoice = tracking.Voicings[0].Voices.Single(v => v.Note == P(NoteStep.E, 1)).VoiceId;
        int cVoice = tracking.Voicings[0].Voices.Single(v => v.Note == P(NoteStep.C, 2)).VoiceId;
        Expect(tracking.Voicings[1].Voices.Single(v => v.Note == P(NoteStep.E, 1)).VoiceId == eVoice,
            "E1 must keep its voice identity from C to Am.");
        Expect(tracking.Voicings[1].Voices.Single(v => v.Note == P(NoteStep.C, 2)).VoiceId == cVoice,
            "C2 must keep its voice identity from C to Am.");

        Expect(tracking.Voicings[2].Voices.Count == 4, "Am7 should add a fourth voice.");
        int newVoiceCount = tracking.Voicings[2].Voices.Count(v => v.VoiceId > 3);
        Expect(newVoiceCount == 1, "The new Am7 note must receive a new persistent voice ID.");

        Expect(tracking.VoicePaths.Count == 4, "The progression should expose four voice paths after the added note.");
        Expect(tracking.VoicePaths.All(path => path.Points.Count == 3),
            "Every voice path must contain one point per chord position.");

        Console.WriteLine("V15: persistent voice identity across progression OK");
    }

    /// <summary>
    /// Tests that the built-in voicing profiles provide independent settings instances, 
    /// ensuring that changes to one profile do not affect another.
    /// </summary>
    private static void VoicingProfilesProvideIndependentPresets() {
        VoicingProfile first = VoicingProfile.ArrangerLeftHand;
        VoicingProfile second = VoicingProfile.ArrangerLeftHand;

        Expect(first.Name == "Arranger Left Hand", "The arranger profile should expose its display name.");
        Expect(first.OneHand.MinChordAreaNote == P(NoteStep.C, 1),
            "Arranger left-hand profile should start at C1.");
        Expect(first.OneHand.MaxChordAreaNote == P(NoteStep.C, 3),
            "Arranger left-hand profile should end at C3.");
        Expect(first.OneHand.ChordMaxKeyCount == 5,
            "Arranger left-hand profile should allow up to five tones.");
        Expect(first.TwoHand.MaxTotalVoices == 4,
            "Arranger profile should cap the true two-hand version at four voices.");

        first.OneHand.ChordMaxKeyCount = 3;
        Expect(second.OneHand.ChordMaxKeyCount == 5,
            "Built-in profiles must return independent settings instances.");

        VoicingProfile compact = VoicingProfile.LeftHandCompact;
        VoicingProfile open = VoicingProfile.LeftHandOpen;
        Expect(compact.OneHand.ChordMaxKeyCount == 4,
            "Compact left-hand profile should use a four-tone ceiling.");
        Expect(compact.OneHand.PreferCompactInitialVoicing,
            "Compact left-hand profile should prefer compact initial voicings.");
        Expect(!open.OneHand.PreferCompactInitialVoicing,
            "Open left-hand profile should not force compact initial voicings.");

        _ = new ChordVoicingEngine(VoicingProfile.Piano.OneHand);
        _ = new TwoHandVoicingEngine(VoicingProfile.TwoHandsBalanced.TwoHand);

        Console.WriteLine("V16: named voicing profiles OK");
    }

    /// <summary>
    /// Tests that transitioning from a C major chord to an A minor chord retains two of the original keys, 
    /// ensuring smooth voice leading.
    /// </summary>
    private static void CToAmKeepsTwoKeys() {
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

    /// <summary>
    /// Tests that transitioning from a C major chord to a B major chord is valid, 
    /// even when there are no common notes between the two chords.
    /// </summary>
    private static void CToBWithNoCommonNotesIsValid()     {
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

    /// <summary>
    /// Tests a series of common chord transitions to ensure that they are valid,
    /// </summary>
    private static void CommonTransitionsAreValid() {
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

        foreach ((string _, Chord chord) target in targets) {
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

    /// <summary>
    /// Tests that a slash chord (e.g., G/B) retains the requested bass note (B) as the lowest note in the voicing, 
    /// even after generating the next chord voicing.
    /// </summary>
    private static void SlashChordKeepsRequestedBass() {
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

    /// <summary>
    /// Tests that the number of voices in a chord voicing can increase or decrease appropriately when transitioning between chords,
    /// </summary>
    private static void VoiceCountCanIncreaseAndDecrease()  {
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

    /// <summary>
    /// Tests that a transition with a large voice movement (greater than the configured maximum) is rejected,
    /// </summary>
    private static void LargeVoiceMovementIsRejected() {
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

    /// <summary>
    /// Tests the ChordProgressionParser to ensure it correctly parses a chord progression with measure bars and durations,
    /// </summary>
    private static void ChordProgressionParserWorks() {
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

    /// <summary>
    /// Tests that the voice identity tracking is explicit and correctly identifies continued voices across a chord transition.
    /// </summary>
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

    /// <summary>
    /// Tests that when an exact first voicing is provided, it is preserved by the voicing engine without alteration.
    /// </summary>
    private static void ExactFirstVoicingIsPreserved()
    {
        ChordVoicingEngine engine = CreateEngine();
        Chord c = Major(NoteStep.C);
        ChordVoicing exact = new(c, new[] { P(NoteStep.E, 1), P(NoteStep.G, 1), P(NoteStep.C, 2) });
        ChordVoicing first = engine.GenerateFirst(ChordVoicingRequest.FromExactVoicing(exact));

        Expect(first == exact, $"Exact first voicing ({Format(exact)}) was not preserved: {Format(first)}");
    }

    /// <summary>
    /// Tests that voicing preferences can be serialized to and deserialized from JSON, 
    /// and that the preferences are correctly applied when generating chord voicings.
    /// </summary>
    private static void VoicingPreferenceJsonWorks() {
        string path = Path.Combine(Path.GetTempPath(), $"EZSong-voicing-preferences-{Guid.NewGuid():N}.json");

        try {
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
        } finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// Helper method to print the details of a chord transition, 
    /// including the chords, their voicings, common notes, and movement analysis.
    /// </summary>
    /// <param name="from">The starting chord voicing.</param>
    /// <param name="to">The ending chord voicing.</param>
    /// <param name="engine">The chord voicing engine used for analysis.</param>
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

    /// <summary>
    /// Helper method to format the pitches of a chord voicing into a human-readable string.
    /// </summary>
    /// <param name="voicing">The chord voicing to format.</param>
    /// <returns>A human-readable string representation of the chord voicing's pitches.</returns>
    private static string FormatNotes(ChordVoicing voicing) {
        return "[" + string.Join(" ", voicing.Pitches.Select(p => p.ToHumanString())) + "]";
    }

    /// <summary>
    /// Helper method to create a default VoicingSettings instance for testing purposes.
    /// </summary>
    /// <returns>A default VoicingSettings instance.</returns>
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

    /// <summary>
    /// Helper method to create a ChordVoicingEngine instance with default settings for testing purposes.
    /// </summary>
    /// <returns>A ChordVoicingEngine instance with default settings.</returns>
    private static ChordVoicingEngine CreateEngine() {
        return new(CreateSettings());
    }

    /// <summary>
    /// Helper method to create a major chord with an optional bass note for testing purposes.
    /// </summary>
    /// <param name="root">The root note of the chord.</param>
    /// <param name="bass">The optional bass note of the chord.</param>
    /// <returns>A major chord with the specified root and optional bass note.</returns>
    private static Chord Major(NoteStep root, NoteStep? bass = null) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.NoneOrMajor, bass);
    }

    /// <summary>
    /// Helper method to create a minor chord for testing purposes.
    /// </summary>
    /// <param name="root">The root note of the chord.</param>
    /// <returns>A minor chord with the specified root note.</returns>
    private static Chord Minor(NoteStep root) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.Minor);
    }

    /// <summary>
    ///Helper method to create a seventh chord for testing purposes.
    /// </summary>
    /// <param name="root">The root note of the chord.</param>
    /// <returns>A seventh chord with the specified root note.</returns>
    private static Chord Seventh(NoteStep root) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.Seventh);
    }

    /// <summary>
    /// Helper method to create a minor seventh chord for testing purposes.
    /// </summary>
    /// <param name="root">The root note of the chord.</param>
    /// <returns>A minor seventh chord with the specified root note.</returns>
    private static Chord MinorSeventh(NoteStep root) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.MinorSeventh);
    }


    /// <summary>
    /// Helper method to create a major seventh chord for testing purposes.
    /// </summary>
    /// <param name="root">The root note of the chord.</param>
    /// <returns>A major seventh chord with the specified root note.</returns>
    private static Chord MajorSeventh(NoteStep root) {
        return new(
        false, new RhythmRationalDuration(1, 1, 0), root, Alteration.neutral,
        ChordType.MajorSeventh);
    }
    
    /// <summary>
    /// Helper method to create a pitch with the specified note and octave for testing purposes.
    /// </summary>
    /// <param name="note">The note of the pitch.</param>
    /// <param name="octave">The octave of the pitch.</param>
    /// <returns>A pitch with the specified note and octave.</returns>
    private static Pitch P(NoteStep note, int octave) {
        return new(note, Alteration.neutral, octave);
    }

    /// <summary>
    /// Asserts a condition and throw an InvalidOperationException with a message if the condition is false.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The message to include in the exception if the condition is false.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void Expect(bool condition, string message)
    {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Helper method to format a ChordVoicing into a human-readable string for testing output.
    /// </summary>
    /// <param name="voicing">The ChordVoicing to format.</param>
    /// <returns>A human-readable string representation of the ChordVoicing.</returns>
    private static string Format(ChordVoicing voicing) {
        return $"{voicing.Chord.ToHumanString()} [{string.Join(" ", voicing.Pitches.Select(p => p.ToHumanString()))}]";
    }
}
