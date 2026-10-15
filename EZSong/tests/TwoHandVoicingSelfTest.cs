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

        TwoHandVoicingEngine engine = new(settings);
        IReadOnlyList<TwoHandChordVoicing> result = engine.GenerateProgression(requests);
        Expect(result.Count == 2, "Expected two two-hand voicings.");
        foreach (TwoHandChordVoicing v in result)
        {
            Expect(v.LeftHand.Count >= 1, "Each chord must contain a left-hand note.");
            Expect(v.RightHand.Count >= 1, "Each chord must contain a right-hand note.");
            Expect(v.Pitches.Count == 3, "C and G major should contain three chord tones.");
            Expect(v.LeftHand.All(p => p >= settings.MinLeftHandNote && p <= settings.MaxLeftHandNote), "Left-hand note outside range.");
            Expect(v.RightHand.All(p => p >= settings.MinRightHandNote && p <= settings.MaxRightHandNote), "Right-hand note outside range.");
        }

        MidiChordExportSettings export = new();
        TwoHandMidiExporter.Export(result, outputPath, export);
        MidiFile midi = MidiFile.Read(outputPath);
        Expect(midi.GetTrackChunks().Count() == 2, "Expected two MIDI tracks.");
        Console.WriteLine("TwoHandVoicingSelfTest: ALL OK");
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}
