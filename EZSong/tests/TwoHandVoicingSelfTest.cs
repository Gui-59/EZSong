using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;
using Melanchall.DryWetMidi.Core;

namespace EZSong.Tests;

public static class TwoHandVoicingSelfTest
{
    public static void Run(string outputPath)
    {
        Chord c = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.C, Alteration.neutral, ChordType.NoneOrMajor);
        Chord g = new(false, new RhythmRationalDuration(1, 1, 0), NoteStep.G, Alteration.neutral, ChordType.NoneOrMajor);
        ChordVoicingRequest[] requests = { new(c), new(g) };

        TwoHandVoicingSettings settings = new()
        {
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
        for (int i = 0; i < result.Count; i++)
        {
            Expect(result[i].Pitches.SequenceEqual(engineResult[i].Pitches),
                "The high-level two-hand generator produced a different progression than the engine.");
        }
        Expect(result.Count == 2, "Expected two two-hand voicings.");
        foreach (TwoHandChordVoicing v in result)
        {
            Expect(v.LeftHand.Count >= 1, "Each chord must contain a left-hand note.");
            Expect(v.RightHand.Count >= 1, "Each chord must contain a right-hand note.");
            Expect(v.Pitches.Count == 3, "C and G major should contain three chord tones.");
            Expect(v.LeftHand.All(p => p >= settings.MinLeftHandNote && p <= settings.MaxLeftHandNote), "Left-hand note outside range.");
            Expect(v.RightHand.All(p => p >= settings.MinRightHandNote && p <= settings.MaxRightHandNote), "Right-hand note outside range.");
        }

        MidiFile midi = MidiFile.Read(outputPath);
        Expect(midi.GetTrackChunks().Count() == 2, "Expected two MIDI tracks.");
        TwoHandVoiceIdentityIsExplicit(result[0], result[1], settings);
        Console.WriteLine("TwoHandVoicingSelfTest: ALL OK");
    }

    private static void TwoHandVoiceIdentityIsExplicit(
        TwoHandChordVoicing first,
        TwoHandChordVoicing second,
        TwoHandVoicingSettings settings)
    {
        TwoHandVoicingEngine engine = new(settings);
        VoicingTransitionAnalysis analysis = engine.AnalyzeTransition(first, second);

        Expect(analysis.IsValid, "Two-hand transition should be valid.");
        Expect(analysis.VoiceMovements.Count == first.Pitches.Count,
            "Expected one explicit movement for each continued voice.");
        Expect(analysis.VoiceMovements.All(v => v.IsContinued),
            "The test progression should keep all voices alive.");
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}
