using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Writes a sequence of realized chords to a standard MIDI file.
/// Rhythm is deliberately handled here, not by the voicing engine.
/// </summary>
public static class MidiChordExporter
{
    public const int TicksPerQuarterNote = 480;

    public static void Export(
        IReadOnlyList<ChordVoicing> voicedChords,
        string outputPath,
        int velocity = 90)
    {
        ArgumentNullException.ThrowIfNull(voicedChords);
        if (voicedChords.Count == 0) {
            throw new ArgumentException("At least one chord is required.", nameof(voicedChords));
        }

        if (velocity < 1 || velocity > 127) {
            throw new ArgumentOutOfRangeException(nameof(velocity));
        }

        TrackChunk track = new();
        List<(long AbsoluteTick, int Order, MidiEvent Event)> events = new();

        long position = 0;
        int order = 0;
        foreach (ChordVoicing voicedChord in voicedChords)
        {
            long end = position + DurationToTicks(voicedChord.Chord.Duration);
            foreach (Model.Pitch midiNote in voicedChord.Pitches)
            {
                events.Add((position, order++, new NoteOnEvent(
                    (SevenBitNumber)midiNote.ToMidiNoteNumber(),
                    (SevenBitNumber)velocity)));
                events.Add((end, order++, new NoteOffEvent(
                    (SevenBitNumber)midiNote.ToMidiNoteNumber(),
                    (SevenBitNumber)0)));
            }
            position = end;
        }

        long previousAbsolute = 0;
        foreach ((long AbsoluteTick, int Order, MidiEvent Event) positioned in events.OrderBy(e => e.AbsoluteTick).ThenBy(e => e.Order))
        {
            positioned.Event.DeltaTime = positioned.AbsoluteTick - previousAbsolute;
            previousAbsolute = positioned.AbsoluteTick;
            track.Events.Add(positioned.Event);
        }

        MidiFile midiFile = new(track);
        midiFile.TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote);
        midiFile.Write(outputPath, overwriteFile: true);
    }


    private static long DurationToTicks(RhythmRationalDuration duration)
    {
        RhythmRationalDuration normalized = duration.ApplyDots().Normalize();
        return (long)Math.Round(
            TicksPerQuarterNote * 4.0 * normalized.Numerator / normalized.Denominator,
            MidpointRounding.AwayFromZero);
    }
}
