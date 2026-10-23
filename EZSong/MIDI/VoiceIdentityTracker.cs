using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Assigns persistent voice IDs to a complete one-hand voicing progression.
/// IDs survive chord changes when VoiceLeadingMatcher considers the voice continued.
/// A removed voice is not resurrected later; a later added note gets a new ID.
/// </summary>
public static class VoiceIdentityTracker
{
    public static VoiceTrackingResult TrackProgression(
        IReadOnlyList<ChordVoicing> progression,
        int maxVoiceMovement = 11)
    {
        ArgumentNullException.ThrowIfNull(progression);
        if (maxVoiceMovement < 0 || maxVoiceMovement > 127)
            {
            throw new ArgumentOutOfRangeException(nameof(maxVoiceMovement));
        }
        if (progression.Count == 0) {
            return new VoiceTrackingResult(Array.Empty<VoiceTrackedChordVoicing>(), Array.Empty<VoicePath>());
        }

        Dictionary<int, VoicePathBuilder> builders = new();
        Dictionary<Pitch, int> activeIds = new();
        int nextVoiceId = 1;
        List<VoiceTrackedChordVoicing> tracked = new(progression.Count);

        // First chord: every note starts a voice.
        List<TrackedVoice> firstVoices = new();
        foreach (Pitch note in progression[0].Pitches.OrderBy(x => x.ToMidiNoteNumber()))
        {
            int id = nextVoiceId++;
            activeIds[note] = id;
            VoicePathBuilder builder = new(id, progression.Count);
            builders.Add(id, builder);
            builder.Set(0, progression[0].Chord, note);
            firstVoices.Add(new TrackedVoice(id, note));
        }
        tracked.Add(new VoiceTrackedChordVoicing(progression[0], firstVoices));

        for (int chordIndex = 1; chordIndex < progression.Count; chordIndex++)
        {
            ChordVoicing previous = progression[chordIndex - 1];
            ChordVoicing current = progression[chordIndex];
            VoiceLeadingMatch match = VoiceLeadingMatcher.Match(
                previous.Pitches,
                current.Pitches,
                maxVoiceMovement);

            if (!match.IsValid)
                {
                throw new InvalidOperationException($"Cannot track voices between chord {chordIndex - 1} ({previous.Chord.ToHumanString()}) " + $"and chord {chordIndex} ({current.Chord.ToHumanString()}).");
            }

            Dictionary<Pitch, int> nextActive = new();
            List<TrackedVoice> currentVoices = new();

            foreach (Pitch note in current.Pitches.OrderBy(x => x.ToMidiNoteNumber()))
            {
                VoiceMovement? continuation = match.Movements.FirstOrDefault(m =>
                    m.IsContinued && m.CurrentNote!.Equals(note));

                int id;
                if (continuation is not null)
                {
                    if (!activeIds.TryGetValue(continuation.PreviousNote!, out id))
                        {
                        throw new InvalidOperationException("Voice-leading matcher referenced an inactive voice.");
                    }
                }
                else
                {
                    id = nextVoiceId++;
                    VoicePathBuilder builder = new(id, progression.Count);
                    builders.Add(id, builder);
                }

                builders[id].Set(chordIndex, current.Chord, note);
                nextActive[note] = id;
                currentVoices.Add(new TrackedVoice(id, note));
            }

            // Removed voices simply remain null from this chord onward.
            activeIds = nextActive;
            tracked.Add(new VoiceTrackedChordVoicing(current, currentVoices));
        }

        return new VoiceTrackingResult(
            tracked,
            builders.Values.Select(b => b.Build(progression.Select(p => p.Chord).ToArray())).OrderBy(v => v.VoiceId).ToArray());
    }

    private sealed class VoicePathBuilder
    {
        private readonly VoicePathPoint?[] _points;
        public int VoiceId { get; }

        public VoicePathBuilder(int voiceId, int count)
        {
            VoiceId = voiceId;
            _points = new VoicePathPoint?[count];
        }

        public void Set(int chordIndex, Chord chord, Pitch note) {
            _points[chordIndex] = new VoicePathPoint(chordIndex, chord, note);
        }

        public VoicePath Build(IReadOnlyList<Chord> chords)
        {
            VoicePathPoint[] points = new VoicePathPoint[_points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = _points[i] ?? new VoicePathPoint(i, chords[i], null);
            }
            return new VoicePath(VoiceId, points);
        }
    }
}
