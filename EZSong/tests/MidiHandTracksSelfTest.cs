using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;
using Melanchall.DryWetMidi.Core;

namespace EZSong.Tests;

/// <summary>
/// A self-test class for verifying the correct functionality of MIDI hand track generation and export.
/// </summary>
public static class MidiHandTracksSelfTest {

    /// <summary>
    /// Runs the self-tests for MIDI hand track generation and export. 
    /// It creates a chord voicing, exports it to a MIDI file, and verifies that the resulting MIDI file contains the expected number of tracks and notes.
    /// </summary>
    /// <param name="outputPath">The path to the output MIDI file.</param>
    public static void Run(string outputPath) {
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

        MidiChordExportSettings settings = new() {
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


    /// <summary>
    /// A helper method that checks a condition 
    /// and throws an InvalidOperationException with a specified message if the condition is false.
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
