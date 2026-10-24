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
        if (voicedChords.Count == 0) {
            throw new ArgumentException("At least one chord is required.", nameof(voicedChords));
        }

        if (velocity < 1 || velocity > 127) {
            throw new ArgumentOutOfRangeException(nameof(velocity));
        }

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
        foreach ((long Tick, int Order, MidiEvent Event) item in events.OrderBy(x => x.Tick).ThenBy(x => x.Order))
        {
            item.Event.DeltaTime = item.Tick - previous;
            previous = item.Tick;
            track.Events.Add(item.Event);
        }
    }

    private static long DurationToTicks(RhythmRationalDuration duration)
    {
        return VoicingMidiExportValidation.DurationToTicks(duration);
    }
    /// <summary>
    /// V18 export from the complete two-hand musical result. No voicing is regenerated.
    /// Optionally creates one track per persistent LH/RH voice identity.
    /// </summary>
    public static void Export(
        TwoHandVoicingProgression progression,
        string outputPath,
        MidiChordExportSettings settings,
        int velocity = 90)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();
        VoicingMidiExportValidation.Validate(progression, outputPath, velocity);

        TrackChunk left = new();
        TrackChunk right = new();
        left.Events.Add(new SequenceTrackNameEvent(settings.LeftHandTrackName));
        right.Events.Add(new SequenceTrackNameEvent(settings.RightHandTrackName));
        List<(long Tick, int Order, MidiEvent Event)> leftEvents = new();
        List<(long Tick, int Order, MidiEvent Event)> rightEvents = new();
        long position = 0;
        int order = 0;

        foreach (TwoHandVoicingProgressionEntry entry in progression.Entries)
        {
            long end = position + VoicingMidiExportValidation.DurationToTicks(entry.Duration.GetEffectiveDuration());
            AddNotes(entry.Voicing.LeftHand, position, end, settings.LeftHandChannel, velocity, leftEvents, ref order);
            AddNotes(entry.Voicing.RightHand, position, end, settings.RightHandChannel, velocity, rightEvents, ref order);
            position = end;
        }

        AddAbsoluteEvents(left, leftEvents);
        AddAbsoluteEvents(right, rightEvents);

        List<TrackChunk> tracks = new() { left, right };
        if (settings.CreateVoiceTracks)
        {
            tracks.AddRange(BuildVoiceTracks(progression.LeftHandVoicePaths, progression.Entries.Count,
                progression.Entries.Select(e => e.Duration.GetEffectiveDuration()).ToArray(),
                $"{settings.LeftHandTrackName} - {settings.VoiceTrackNamePrefix}", settings.VoiceTrackChannel));
            tracks.AddRange(BuildVoiceTracks(progression.RightHandVoicePaths, progression.Entries.Count,
                progression.Entries.Select(e => e.Duration.GetEffectiveDuration()).ToArray(),
                $"{settings.RightHandTrackName} - {settings.VoiceTrackNamePrefix}", settings.VoiceTrackChannel));
        }

        MidiFile midi = new(tracks.ToArray())
        {
            TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote)
        };
        midi.Write(outputPath, overwriteFile: true);
    }

    private static List<TrackChunk> BuildVoiceTracks(
        IReadOnlyList<VoicePath> voicePaths,
        int entryCount,
        IReadOnlyList<RhythmRationalDuration> durations,
        string namePrefix,
        int channel)
    {
        List<TrackChunk> tracks = new();
        foreach (VoicePath path in voicePaths.OrderBy(v => v.VoiceId))
        {
            if (path.Points.Count != entryCount)
                throw new InvalidOperationException($"Voice {path.VoiceId} path length does not match the progression.");
            TrackChunk track = new();
            track.Events.Add(new SequenceTrackNameEvent($"{namePrefix}{path.VoiceId}"));
            List<(long Tick, int Order, MidiEvent Event)> events = new();
            long position = 0;
            int order = 0;
            for (int i = 0; i < entryCount; i++)
            {
                long end = position + VoicingMidiExportValidation.DurationToTicks(durations[i]);
                Pitch? note = path.Points[i].Note;
                if (note is not null)
                {
                    NoteOnEvent on = new((SevenBitNumber)note.ToMidiNoteNumber(), (SevenBitNumber)90)
                    { Channel = (FourBitNumber)channel };
                    NoteOffEvent off = new((SevenBitNumber)note.ToMidiNoteNumber(), (SevenBitNumber)0)
                    { Channel = (FourBitNumber)channel };
                    events.Add((position, order++, on));
                    events.Add((end, order++, off));
                }
                position = end;
            }
            AddAbsoluteEvents(track, events);
            tracks.Add(track);
        }
        return tracks;
    }

}
