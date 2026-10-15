using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Writes realized chords to a standard MIDI file.
/// Rhythm is deliberately handled here, not by the voicing engine.
/// </summary>
public static class MidiChordExporter
{
    public const int TicksPerQuarterNote = 480;

    /// <summary>
    /// Legacy/simple export: all notes are written to one MIDI track.
    /// </summary>
    public static void Export(
        IReadOnlyList<ChordVoicing> voicedChords,
        string outputPath,
        int velocity = 90)
    {
        ExportSingleTrack(voicedChords, outputPath, velocity);
    }

    /// <summary>
    /// Exports the progression to two MIDI tracks, assigning notes below the
    /// split point to the left hand and notes at/above it to the right hand.
    /// The voicings themselves are not changed.
    /// </summary>
    public static void Export(
        IReadOnlyList<ChordVoicing> voicedChords,
        string outputPath,
        MidiChordExportSettings settings,
        int velocity = 90)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();
        ValidateCommonArguments(voicedChords, outputPath, velocity);

        TrackChunk leftTrack = new();
        TrackChunk rightTrack = new();
        leftTrack.Events.Add(new SequenceTrackNameEvent(settings.LeftHandTrackName));
        rightTrack.Events.Add(new SequenceTrackNameEvent(settings.RightHandTrackName));

        List<(long Tick, int Order, MidiEvent Event)> leftEvents = new();
        List<(long Tick, int Order, MidiEvent Event)> rightEvents = new();

        BuildHandEvents(
            voicedChords,
            settings,
            velocity,
            leftEvents,
            rightEvents);

        AddAbsoluteEvents(leftTrack, leftEvents);
        AddAbsoluteEvents(rightTrack, rightEvents);

        List<TrackChunk> tracks = new();
        if (settings.CreateEmptyTracks || leftEvents.Count > 0)
            tracks.Add(leftTrack);
        if (settings.CreateEmptyTracks || rightEvents.Count > 0)
            tracks.Add(rightTrack);

        MidiFile midiFile = new(tracks.ToArray())
        {
            TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote)
        };
        midiFile.Write(outputPath, overwriteFile: true);
    }

    private static void ExportSingleTrack(
        IReadOnlyList<ChordVoicing> voicedChords,
        string outputPath,
        int velocity)
    {
        ValidateCommonArguments(voicedChords, outputPath, velocity);

        TrackChunk track = new();
        List<(long AbsoluteTick, int Order, MidiEvent Event)> events = new();

        long position = 0;
        int order = 0;
        foreach (ChordVoicing voicedChord in voicedChords)
        {
            long end = position + DurationToTicks(voicedChord.Chord.Duration);
            foreach (Pitch midiNote in voicedChord.Pitches)
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

        AddAbsoluteEvents(track, events);

        MidiFile midiFile = new(track)
        {
            TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote)
        };
        midiFile.Write(outputPath, overwriteFile: true);
    }

    private static void BuildHandEvents(
        IReadOnlyList<ChordVoicing> voicedChords,
        MidiChordExportSettings settings,
        int velocity,
        List<(long Tick, int Order, MidiEvent Event)> leftEvents,
        List<(long Tick, int Order, MidiEvent Event)> rightEvents)
    {
        long position = 0;
        int order = 0;
        int splitMidi = settings.SplitPoint.ToMidiNoteNumber();

        foreach (ChordVoicing voicedChord in voicedChords)
        {
            long end = position + DurationToTicks(voicedChord.Chord.Duration);

            foreach (Pitch pitch in voicedChord.Pitches)
            {
                List<(long Tick, int Order, MidiEvent Event)> target =
                    pitch.ToMidiNoteNumber() < splitMidi ? leftEvents : rightEvents;

                int channel = target == leftEvents
                    ? settings.LeftHandChannel
                    : settings.RightHandChannel;

                NoteOnEvent noteOn = new(
                    (SevenBitNumber)pitch.ToMidiNoteNumber(),
                    (SevenBitNumber)velocity)
                {
                    Channel = (FourBitNumber)channel
                };

                NoteOffEvent noteOff = new(
                    (SevenBitNumber)pitch.ToMidiNoteNumber(),
                    (SevenBitNumber)0)
                {
                    Channel = (FourBitNumber)channel
                };

                target.Add((position, order++, noteOn));
                target.Add((end, order++, noteOff));
            }

            position = end;
        }
    }

    private static void AddAbsoluteEvents(
        TrackChunk track,
        IEnumerable<(long Tick, int Order, MidiEvent Event)> events)
    {
        long previousAbsolute = 0;
        foreach ((long Tick, int Order, MidiEvent Event) positioned in
                 events.OrderBy(e => e.Tick).ThenBy(e => e.Order))
        {
            positioned.Event.DeltaTime = positioned.Tick - previousAbsolute;
            previousAbsolute = positioned.Tick;
            track.Events.Add(positioned.Event);
        }
    }

    private static void ValidateCommonArguments(
        IReadOnlyList<ChordVoicing> voicedChords,
        string outputPath,
        int velocity)
    {
        ArgumentNullException.ThrowIfNull(voicedChords);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (voicedChords.Count == 0)
            throw new ArgumentException("At least one chord is required.", nameof(voicedChords));

        if (velocity < 1 || velocity > 127)
            throw new ArgumentOutOfRangeException(nameof(velocity));
    }

    private static long DurationToTicks(RhythmRationalDuration duration)
    {
        RhythmRationalDuration normalized = duration.ApplyDots().Normalize();
        return (long)Math.Round(
            TicksPerQuarterNote * 4.0 * normalized.Numerator / normalized.Denominator,
            MidpointRounding.AwayFromZero);
    }
}
