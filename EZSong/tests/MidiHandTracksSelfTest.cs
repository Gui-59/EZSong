using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;
using Melanchall.DryWetMidi.Core;

namespace EZSong.Tests;

/// <summary>
/// Regression test for two-track MIDI export. Hand-thesia is deliberately not
/// referenced: this validates only the MIDI produced by EZSong.
/// </summary>
public static class MidiHandTracksSelfTest
{
    public static void Run(string outputPath)
    {
        Chord chord = new(
            false,
            new RhythmRationalDuration(1, 1, 0),
            NoteStep.C,
            Alteration.neutral,
            ChordType.NoneOrMajor);

        ChordVoicing voicing = new(chord, new[]
        {
            new Pitch(NoteStep.E, Alteration.neutral, 2),
            new Pitch(NoteStep.G, Alteration.neutral, 2),
            new Pitch(NoteStep.C, Alteration.neutral, 3),
            new Pitch(NoteStep.E, Alteration.neutral, 3)
        });

        MidiChordExportSettings settings = new()
        {
            SplitPoint = new Pitch(NoteStep.C, Alteration.neutral, 3),
            LeftHandChannel = 0,
            RightHandChannel = 1
        };

        MidiChordExporter.Export(new[] { voicing }, outputPath, settings);

        MidiFile midi = MidiFile.Read(outputPath);
        TrackChunk[] tracks = midi.GetTrackChunks().ToArray();
        Expect(tracks.Length == 2, "Expected two MIDI tracks.");
        Expect(tracks[0].Events.OfType<NoteOnEvent>().Count() == 2,
            "Expected two left-hand notes.");
        Expect(tracks[1].Events.OfType<NoteOnEvent>().Count() == 2,
            "Expected two right-hand notes.");

        Console.WriteLine("MidiHandTracksSelfTest: ALL OK");
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}
