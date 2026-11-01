using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;
using Melanchall.DryWetMidi.Core;

namespace EZSong.Tests;

/// <summary>
/// A self-test class for the TwoHandVoicingEngine and related functionality, 
/// verifying that two-hand chord voicings are generated correctly and adhere to specified settings.
/// </summary>
public static class TwoHandVoicingSelfTest {

    /// <summary>
    /// Runs the self-test for two-hand chord voicing generation, 
    /// comparing the results of the high-level API with the underlying engine and validating the generated MIDI file.
    /// </summary>
    /// <param name="outputPath"></param>
    public static void Run(string outputPath) {
        Chord c = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.C, Alteration.neutral, ChordType.NoneOrMajor);
        Chord g = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.G, Alteration.neutral, ChordType.NoneOrMajor);
        ChordVoicingRequest[] requests = { new(c), new(g) };

        TwoHandVoicingSettings settings = new() {
            MinLeftHandNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
            MaxLeftHandNote = new Pitch(NoteStep.B, Alteration.neutral, 2),
            MinRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
            MaxRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 4),
            MinLeftHandVoices = 1,
            MaxLeftHandVoices = 2,
            MinRightHandVoices = 1,
            MaxRightHandVoices = 2,
            MaxTotalVoices = 3
        };

        // Exercise the public high-level API as well as the underlying engine.
        // This is important: GenerateTwoHands must be part of the tested path,
        // not just a dead API that happens to compile.
        TwoHandVoicingEngine engine = new(settings);
        IReadOnlyList<TwoHandChordVoicing> engineResult = engine.GenerateProgression(requests);

        ChordMidiGenerator generator = new();
        MidiChordExportSettings export = new();
        IReadOnlyList<TwoHandChordVoicing> result = generator.GenerateTwoHands(
            requests[0],
            requests.Skip(1),
            outputPath,
            settings,
            export);

        Expect(result.Count == engineResult.Count,
            "The high-level two-hand generator returned an unexpected number of voicings.");
        for (int i = 0; i < result.Count; i++) {
            Expect(result[i].Pitches.SequenceEqual(engineResult[i].Pitches),
                "The high-level two-hand generator produced a different progression than the engine.");
        }
        Expect(result.Count == 2, "Expected two two-hand voicings.");
        foreach (TwoHandChordVoicing v in result) {
            Expect(v.LeftHand.Count >= 1, "Each chord must contain a left-hand note.");
            Expect(v.RightHand.Count >= 1, "Each chord must contain a right-hand note.");
            Expect(v.Pitches.Count == 3, "C and G major should contain three chord tones.");
            Expect(v.LeftHand.All(p => p >= settings.MinLeftHandNote && p <= settings.MaxLeftHandNote), "Left-hand note outside range.");
            Expect(v.RightHand.All(p => p >= settings.MinRightHandNote && p <= settings.MaxRightHandNote), "Right-hand note outside range.");
        }

        MidiFile midi = MidiFile.Read(outputPath);
        Expect(midi.GetTrackChunks().Count() == 2, "Expected two MIDI tracks.");
        TwoHandVoiceIdentityIsExplicit(result[0], result[1], settings);
        TwoHandExactVoicingRejectsNonChordTone();
        TwoHandExtendedChordReduction();
        Console.WriteLine("TwoHandVoicingSelfTest: ALL OK");
    }

    /// <summary>
    /// Tests that the two-hand exact voicing generation correctly rejects notes that do not belong to the specified chord.
    /// </summary>
    private static void TwoHandExactVoicingRejectsNonChordTone() {
        Chord c = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.C, Alteration.neutral, ChordType.NoneOrMajor);
        TwoHandVoicingSettings settings = new() {
            MinLeftHandNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
            MaxLeftHandNote = new Pitch(NoteStep.B, Alteration.neutral, 2),
            MinRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
            MaxRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 4),
            MinLeftHandVoices = 1,
            MaxLeftHandVoices = 2,
            MinRightHandVoices = 1,
            MaxRightHandVoices = 2,
            MaxTotalVoices = 4
        };

        bool rejected = false;
        try {
            _ = new TwoHandVoicingEngine(settings).GenerateFirst(
                ChordVoicingRequest.FromExactVoicing(
                    c,
                    new Pitch(NoteStep.C, Alteration.neutral, 2),
                    new Pitch(NoteStep.F, Alteration.neutral, 3)));
        } catch (ArgumentException) {
            rejected = true;
        }

        Expect(rejected, "A two-hand exact voicing must reject notes that do not belong to the chord.");
    }


    /// <summary>
    /// Tests that the two-hand voicing engine correctly reduces extended chords (like C13) to a manageable number of voices while respecting chord tone priority.
    /// </summary>
    private static void TwoHandExtendedChordReduction() {
        Chord c13 = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.C, Alteration.neutral, ChordType.Thirteenth);
        TwoHandVoicingSettings settings = new() {
            MinLeftHandNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
            MaxLeftHandNote = new Pitch(NoteStep.B, Alteration.neutral, 2),
            MinRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
            MaxRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 4),
            MinLeftHandVoices = 1,
            MaxLeftHandVoices = 2,
            MinRightHandVoices = 1,
            MaxRightHandVoices = 3,
            MaxTotalVoices = 5,
            ChordTonePriorityPolicy = ChordTonePriorityPolicy.EssentialsThenExtensions
        };

        TwoHandChordVoicing voicing = new TwoHandVoicingEngine(settings)
            .GenerateFirst(new ChordVoicingRequest(c13));

        int[] expected = { 0, 2, 4, 9, 10 };
        Expect(voicing.Pitches.Select(p => p.ToMidiNoteNumber() % 12).OrderBy(x => x).SequenceEqual(expected),
            "Two-hand C13 should use the same intelligent tone-priority policy as one-hand voicing.");
    }

    /// <summary>
    /// Tests that the two-hand voicing engine correctly identifies explicit voice movements between two chord voicings, ensuring that all voices are continued and valid.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="settings"></param>
    private static void TwoHandVoiceIdentityIsExplicit(
        TwoHandChordVoicing first,
        TwoHandChordVoicing second,
        TwoHandVoicingSettings settings) {
        TwoHandVoicingEngine engine = new(settings);
        VoicingTransitionAnalysis analysis = engine.AnalyzeTransition(first, second);

        Expect(analysis.IsValid, "Two-hand transition should be valid.");
        Expect(analysis.VoiceMovements.Count == first.Pitches.Count,
            "Expected one explicit movement for each continued voice.");
        Expect(analysis.VoiceMovements.All(v => v.IsContinued),
            "The test progression should keep all voices alive.");
    }


    /// <summary>
    /// A helper method to assert a condition and throw an InvalidOperationException with a specified message if the condition is false.
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
