using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Computes playable one-hand chord voicings with strong preference for keeping
/// the same physical keys between successive chords.
/// </summary>
public sealed class ChordVoicingEngine
{
    private readonly VoicingSettings _settings;
    private readonly VoicingPreferenceStore? _preferences;

    public ChordVoicingEngine(
        VoicingSettings? settings = null,
        VoicingPreferenceStore? preferences = null)
    {
        _settings = settings ?? new VoicingSettings();
        _settings.Validate();
        _preferences = preferences;
    }

    public ChordVoicing GenerateFirst(ChordVoicingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ExactVoicing is not null) {
            return ValidateExactVoicing(request);
        }

        List<int[]> candidates = GenerateCandidates(request);
        if (candidates.Count == 0) {
            throw new InvalidOperationException($"No valid voicing found for {request.Chord.ToHumanString()}.");
        }

        // With no previous voicing, prefer the most compact voicing in the allowed register.
        return candidates
            .OrderBy(v => Span(v))
            .ThenBy(v => v.Max())
            .ThenBy(v => v.Min())
            .Select(v => new ChordVoicing(request.Chord, v.Select(Pitch.FromMidi)))
            .First();
    }

    public ChordVoicing GenerateNext(ChordVoicingRequest request, ChordVoicing previous)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(previous);

        List<int[]> candidates = GenerateCandidates(request);
        if (candidates.Count == 0) {
            throw new InvalidOperationException($"No valid voicing found for {request.Chord.ToHumanString()} after {previous.Chord.ToHumanString()}.");
        }

        int bestScore = int.MaxValue;
        int[]? best = null;

        foreach (int[] candidate in candidates)
        {
            int score = ScoreTransition(previous, request, candidate);
            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        if (best is null) {
            throw new InvalidOperationException("No valid voicing transition found.");
        }

        return new ChordVoicing(request.Chord, best.Select(Pitch.FromMidi));
    }

    private int ScoreTransition(
        ChordVoicing previous,
        ChordVoicingRequest request,
        IReadOnlyList<int> candidate)
    {
        int[] previousNotes = previous.MidiNotes.OrderBy(x => x).ToArray();
        int[] newNotes = candidate.OrderBy(x => x).ToArray();

        int score = 0;

        // Strong preference for keys that remain physically held.
        int common = previousNotes.Intersect(newNotes).Count();
        score -= common * _settings.CommonNoteWeight;

        // Match notes between the two sets so that movement is minimal.
        (int TotalMovement, bool IsValid) matching = MinimumMovementMatching(previousNotes, newNotes, _settings.MaxVoiceMovement);
        if (!matching.IsValid) {
            return int.MaxValue;
        }

        score += matching.TotalMovement * _settings.MovementWeight;

        // The lowest note is especially important for left-hand arranger playing.
        int bassMovement = Math.Abs(newNotes[0] - previousNotes[0]);
        if (bassMovement > _settings.MaxVoiceMovement) {
            return int.MaxValue;
        }

        score += bassMovement * _settings.BassMovementWeight;

        // A user preference is a strong bonus, but never overrides hard constraints.
        if (_preferences is not null)
        {
            string from = previous.Chord.ToHumanString();
            string to = request.Chord.ToHumanString();
            foreach (VoicingPreference preference in _preferences.Find(from, to))
            {
                if (preference.ToMidiNotes.SequenceEqual(newNotes)) {
                    score -= _settings.UserPreferenceWeight;
                }
            }
        }

        return score;
    }

    private List<int[]> GenerateCandidates(ChordVoicingRequest request)
    {
        int[] pitchClasses = GetChordPitchClasses(request.Chord)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        if (pitchClasses.Length < 2) {
            throw new InvalidOperationException("A chord must contain at least two distinct notes.");
        }

        if (pitchClasses.Length > _settings.MaxVoices) {
            pitchClasses = ReduceToMaximumVoices(request.Chord, pitchClasses, _settings.MaxVoices);
        }

        if (request.Bass is not null && !pitchClasses.Contains(request.Bass.Value.ChromaticValue)) {
            throw new InvalidOperationException(
                $"Bass {request.Bass.Value.Note} is not a note of chord {request.Chord.ToHumanString()}.");
        }

        List<int[]> options = new();
        int[] partial = new int[pitchClasses.Length];
        GenerateAscending(0, int.MinValue, pitchClasses, request.Bass, partial, new bool[pitchClasses.Length], options);
        return options;
    }

    private void GenerateAscending(
        int index,
        int previousMidi,
        int[] pitchClasses,
        PitchClass? bass,
        int[] partial,
        bool[] usedPitchClasses,
        List<int[]> result)
    {
        if (index == pitchClasses.Length)
        {
            result.Add(partial.ToArray());
            return;
        }

        for (int pitchClassIndex = 0; pitchClassIndex < pitchClasses.Length; pitchClassIndex++)
        {
            if (usedPitchClasses[pitchClassIndex]) {
                continue;
            }

            // A slash chord constrains only the lowest note of the voicing.
            // Therefore the first generated note must have the requested
            // pitch class; the remaining chord tones remain completely free.
            if (index == 0 &&
                bass is not null &&
                pitchClasses[pitchClassIndex] != bass.Value.ChromaticValue)
            {
                continue;
            }

            foreach (int midi in EnumerateMidiNotes(pitchClasses[pitchClassIndex]))
            {
                // previousMidi is the last note already placed in the
                // ascending candidate. This gives every voicing one canonical
                // representation and prevents duplicate MIDI keys.
                if (midi <= previousMidi) {
                    continue;
                }

                usedPitchClasses[pitchClassIndex] = true;
                partial[index] = midi;

                GenerateAscending(
                    index + 1,
                    midi,
                    pitchClasses,
                    bass,
                    partial,
                    usedPitchClasses,
                    result);

                usedPitchClasses[pitchClassIndex] = false;
            }
        }
    }

    private IEnumerable<int> EnumerateMidiNotes(int pitchClass)
    {
        int first = _settings.MinChordNote.ToMidiNoteNumber() + Mod(pitchClass - _settings.MinChordNote.ToMidiNoteNumber(), 12);
        for (int midi = first; midi <= _settings.MaxChordNote.ToMidiNoteNumber(); midi += 12) {
            yield return midi;
        }
    }

    private ChordVoicing ValidateExactVoicing(ChordVoicingRequest request)
    {
        int[] notes = request.ExactVoicing!.Select(p => p.ToMidiNoteNumber()).OrderBy(x => x).ToArray();
        ValidateVoicingNotes(request, notes);
        return new ChordVoicing(request.Chord, request.ExactVoicing!);
    }

    private void ValidateVoicingNotes(ChordVoicingRequest request, IReadOnlyList<int> notes)
    {
        if (notes.Count < 2 || notes.Count > _settings.MaxVoices) {
            throw new ArgumentException("A voicing must contain between 2 and 5 notes.");
        }

        if (notes.Any(n => n < _settings.MinChordNote.ToMidiNoteNumber() || n > _settings.MaxChordNote.ToMidiNoteNumber())) {
            throw new ArgumentException("The exact voicing is outside the configured register.");
        }

        if (notes.Zip(notes.Skip(1), (a, b) => b - a).Any(d => d <= 0)) {
            throw new ArgumentException("A voicing may not contain duplicated MIDI notes.");
        }

        HashSet<int> chordPitchClasses = GetChordPitchClasses(request.Chord).ToHashSet();
        if (notes.Any(n => !chordPitchClasses.Contains(Mod(n, 12)))) {
            throw new ArgumentException("The exact voicing contains a note that does not belong to the chord.");
        }

        if (request.Bass is not null && Mod(notes[0], 12) != request.Bass.Value.ChromaticValue) {
            throw new ArgumentException("The exact voicing does not respect the requested bass note.");
        }
    }

    private static int[] ReduceToMaximumVoices(Chord chord, int[] pitchClasses, int maxVoices)
    {
        // Keep root, third (when present), seventh (when present), then remaining chord tones.
        // This is deliberately conservative: future versions can expose an explicit omission policy.
        int root = Mod((int)chord.RootNote + (int)chord.RootNoteAlteration, 12);
        int[] ordered = pitchClasses
            .OrderBy(pc => pc == root ? 0 : 1)
            .ThenBy(pc => pc)
            .Take(maxVoices)
            .ToArray();
        return ordered;
    }

    private static IEnumerable<int> GetChordPitchClasses(Chord chord)
    {
        int root = Mod((int)chord.RootNote + (int)chord.RootNoteAlteration, 12);
        int[] intervals = chord.ChordType switch
        {
            ChordType.NoneOrMajor => [0, 4, 7],
            ChordType.Minor => [0, 3, 7],
            ChordType.Seventh => [0, 4, 7, 10],
            ChordType.MinorSeventh => [0, 3, 7, 10],
            ChordType.MajorSeventh => [0, 4, 7, 11],
            ChordType.PowerChord => [0, 7],
            ChordType.Sixth => [0, 4, 7, 9],
            ChordType.MinorSixth => [0, 3, 7, 9],
            ChordType.SuspendedSecond => [0, 2, 7],
            ChordType.SuspendedFourth => [0, 5, 7],
            ChordType.Diminished => [0, 3, 6],
            ChordType.Augmented => [0, 4, 8],
            ChordType.DiminishedSeventh => [0, 3, 6, 9],
            ChordType.AugmentedSeventh => [0, 4, 8, 10],
            ChordType.AddSecond => [0, 2, 4, 7],
            ChordType.AddFourth => [0, 4, 5, 7],
            ChordType.AddSixth => [0, 4, 7, 9],
            ChordType.AddNinth => [0, 4, 7, 2],
            ChordType.Ninth => [0, 4, 7, 10, 2],
            ChordType.MinorNinth => [0, 3, 7, 10, 2],
            ChordType.MajorNinth => [0, 4, 7, 11, 2],
            ChordType.Eleventh => [0, 4, 7, 10, 2, 5],
            ChordType.MinorEleventh => [0, 3, 7, 10, 2, 5],
            ChordType.MajorEleventh => [0, 4, 7, 11, 2, 5],
            ChordType.Thirteenth => [0, 4, 7, 10, 2, 9],
            ChordType.MinorThirteenth => [0, 3, 7, 10, 2, 9],
            ChordType.MajorThirteenth => [0, 4, 7, 11, 2, 9],
            ChordType.MinorMajorSeventh => [0, 3, 7, 11],
            ChordType.SixthNinth => [0, 4, 7, 9, 2],
            ChordType.SeventhMinusFive => [0, 4, 6, 10],
            ChordType.SeventhPlusFive => [0, 4, 8, 10],
            ChordType.MinorSeventhFlatFive => [0, 3, 6, 10],
            _ => throw new ArgumentOutOfRangeException(nameof(chord.ChordType))
        };

        return intervals.Select(interval => Mod(root + interval, 12)).Distinct();
    }

    private static (int TotalMovement, bool IsValid) MinimumMovementMatching(
        IReadOnlyList<int> previous,
        IReadOnlyList<int> current,
        int maxMovement)
    {
        int[,] memo = new int[previous.Count + 1, 1 << current.Count];
        bool[,] seen = new bool[previous.Count + 1, 1 << current.Count];

        int Solve(int index, int usedMask)
        {
            if (index == previous.Count) {
                return 0;
            }

            if (seen[index, usedMask]) {
                return memo[index, usedMask];
            }

            seen[index, usedMask] = true;

            const int Impossible = 1_000_000;
            int best = Impossible;

            // This previous note may disappear when the new chord has fewer tones.
            if (previous.Count > current.Count) {
                best = Math.Min(best, Solve(index + 1, usedMask));
            }

            for (int j = 0; j < current.Count; j++)
            {
                if ((usedMask & (1 << j)) != 0) {
                    continue;
                }

                int distance = Math.Abs(previous[index] - current[j]);
                if (distance > maxMovement) {
                    continue;
                }

                int remainder = Solve(index + 1, usedMask | (1 << j));
                if (remainder < Impossible) {
                    best = Math.Min(best, distance + remainder);
                }
            }

            memo[index, usedMask] = best;
            return best;
        }

        int movement = Solve(0, 0);
        return (movement, movement < 1_000_000);
    }

    private static int Span(IReadOnlyList<int> notes) {
        return notes[^1] - notes[0];
    }

    

    

    private static int Mod(int value, int modulo) {
        return (value % modulo + modulo) % modulo;
    }
}
