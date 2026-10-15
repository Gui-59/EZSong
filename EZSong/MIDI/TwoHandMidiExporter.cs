using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>Exports true two-hand voicings without applying a post-export split.</summary>
public static class TwoHandMidiExporter
{
    public const int TicksPerQuarterNote = MidiChordExporter.TicksPerQuarterNote;

    public static void Export(
        IReadOnlyList<TwoHandChordVoicing> voicedChords,
        string outputPath,
        MidiChordExportSettings settings,
        int velocity = 90)
    {
        ArgumentNullException.ThrowIfNull(voicedChords);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        settings.Validate();
        if (voicedChords.Count == 0) throw new ArgumentException("At least one chord is required.", nameof(voicedChords));
        if (velocity < 1 || velocity > 127) throw new ArgumentOutOfRangeException(nameof(velocity));

        TrackChunk left = new();
        TrackChunk right = new();
        left.Events.Add(new SequenceTrackNameEvent(settings.LeftHandTrackName));
        right.Events.Add(new SequenceTrackNameEvent(settings.RightHandTrackName));

        List<(long Tick, int Order, MidiEvent Event)> leftEvents = new();
        List<(long Tick, int Order, MidiEvent Event)> rightEvents = new();
        long position = 0;
        int order = 0;

        foreach (TwoHandChordVoicing chord in voicedChords)
        {
            long end = position + DurationToTicks(chord.Chord.Duration);
            AddNotes(chord.LeftHand, position, end, settings.LeftHandChannel, velocity, leftEvents, ref order);
            AddNotes(chord.RightHand, position, end, settings.RightHandChannel, velocity, rightEvents, ref order);
            position = end;
        }

        AddAbsoluteEvents(left, leftEvents);
        AddAbsoluteEvents(right, rightEvents);

        MidiFile midi = new(left, right)
        {
            TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote)
        };
        midi.Write(outputPath, overwriteFile: true);
    }

    private static void AddNotes(
        IEnumerable<Model.Pitch> notes,
        long start,
        long end,
        int channel,
        int velocity,
        List<(long Tick, int Order, MidiEvent Event)> events,
        ref int order)
    {
        foreach (Model.Pitch pitch in notes)
        {
            NoteOnEvent on = new((SevenBitNumber)pitch.ToMidiNoteNumber(), (SevenBitNumber)velocity)
            { Channel = (FourBitNumber)channel };
            NoteOffEvent off = new((SevenBitNumber)pitch.ToMidiNoteNumber(), (SevenBitNumber)0)
            { Channel = (FourBitNumber)channel };
            events.Add((start, order++, on));
            events.Add((end, order++, off));
        }
    }

    private static void AddAbsoluteEvents(TrackChunk track, IEnumerable<(long Tick, int Order, MidiEvent Event)> events)
    {
        long previous = 0;
        foreach (var item in events.OrderBy(x => x.Tick).ThenBy(x => x.Order))
        {
            item.Event.DeltaTime = item.Tick - previous;
            previous = item.Tick;
            track.Events.Add(item.Event);
        }
    }

    private static long DurationToTicks(RhythmRationalDuration duration)
    {
        RhythmRationalDuration normalized = duration.ApplyDots().Normalize();
        return (long)Math.Round(
            TicksPerQuarterNote * 4.0 * normalized.Numerator / normalized.Denominator,
            MidpointRounding.AwayFromZero);
    }
}
