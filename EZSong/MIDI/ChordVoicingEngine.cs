using EZSong.Enums;
using EZSong.Model;
using System.Collections.Generic;

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

        List<Model.Pitch[]> candidates = GenerateCandidates(request);
        if (candidates.Count == 0) {
            throw new InvalidOperationException($"No valid voicing found for {request.Chord.ToHumanString()}.");
        }

        // With no previous voicing, prefer the most compact voicing in the allowed register.
        return candidates
            .OrderBy(v => Span(v))
            .ThenBy(v => v.Max())
            .ThenBy(v => v.Min())
            .Select(v => new ChordVoicing(request.Chord, v))
            .First();
    }

    /// <summary>
    /// Generates a globally coherent voicing path for a complete chord progression.
    /// Unlike calling <see cref="GenerateFirst"/> followed by repeated
    /// <see cref="GenerateNext"/> calls, this method uses look-ahead over the
    /// complete progression. This is important when a locally compact first
    /// voicing would make a later transition impossible under MaxVoiceMovement.
    /// </summary>
    public IReadOnlyList<ChordVoicing> GenerateProgression(
        IReadOnlyList<ChordVoicingRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        if (requests.Count == 0) {
            return Array.Empty<ChordVoicing>();
        }

        List<Model.Pitch[]>[] layers = new List<Model.Pitch[]>[requests.Count];

        for (int i = 0; i < requests.Count; i++)
        {
            if (i == 0 && requests[i].ExactVoicing is not null)
            {
                layers[i] = new List<Model.Pitch[]>
                {
                    ValidateExactVoicing(requests[i]).Pitches.ToArray()
                };
            }
            else
            {
                layers[i] = GenerateCandidates(requests[i]);
            }

            if (layers[i].Count == 0)
            {
                throw new InvalidOperationException(
                    $"No valid voicing found for {requests[i].Chord.ToHumanString()}.");
            }
        }

        const long Impossible = long.MaxValue / 4;
        long[] previousCosts = new long[layers[0].Count];
        Array.Fill(previousCosts, 0L);

        // For every candidate in every layer, remember the best candidate
        // index from the preceding chord.
        int[][] predecessors = new int[requests.Count][];
        predecessors[0] = Enumerable.Repeat(-1, layers[0].Count).ToArray();

        for (int layer = 1; layer < requests.Count; layer++)
        {
            long[] currentCosts = new long[layers[layer].Count];
            Array.Fill(currentCosts, Impossible);
            predecessors[layer] = Enumerable.Repeat(-1, layers[layer].Count).ToArray();

            for (int currentIndex = 0; currentIndex < layers[layer].Count; currentIndex++)
            {
                Model.Pitch[] current = layers[layer][currentIndex];

                for (int previousIndex = 0; previousIndex < layers[layer - 1].Count; previousIndex++)
                {
                    if (previousCosts[previousIndex] >= Impossible) {
                        continue;
                    }

                    ChordVoicing previous = new(
                        requests[layer - 1].Chord,
                        layers[layer - 1][previousIndex]);

                    VoicingTransitionAnalysis analysis = AnalyzeTransition(
                        previous,
                        requests[layer],
                        current);

                    if (!analysis.IsValid) {
                        continue;
                    }

                    long cost = previousCosts[previousIndex] + analysis.Score;
                    if (cost < currentCosts[currentIndex])
                    {
                        currentCosts[currentIndex] = cost;
                        predecessors[layer][currentIndex] = previousIndex;
                    }
                }
            }

            bool hasValidPath = currentCosts.Any(cost => cost < Impossible);
            if (!hasValidPath)
            {
                throw new InvalidOperationException(
                    $"No valid voicing transition found from " +
                    $"{requests[layer - 1].Chord.ToHumanString()} to " +
                    $"{requests[layer].Chord.ToHumanString()}. " +
                    "The complete progression has no path satisfying the configured voice-leading constraints.");
            }

            previousCosts = currentCosts;
        }

        int bestFinalIndex = 0;
        for (int i = 1; i < previousCosts.Length; i++)
        {
            if (previousCosts[i] < previousCosts[bestFinalIndex]) {
                bestFinalIndex = i;
            }
        }

        int[] selectedIndexes = new int[requests.Count];
        selectedIndexes[^1] = bestFinalIndex;

        for (int layer = requests.Count - 1; layer > 0; layer--)
        {
            selectedIndexes[layer - 1] = predecessors[layer][selectedIndexes[layer]];
        }

        List<ChordVoicing> result = new(requests.Count);
        for (int layer = 0; layer < requests.Count; layer++)
        {
            result.Add(new ChordVoicing(
                requests[layer].Chord,
                layers[layer][selectedIndexes[layer]]));
        }

        return result;
    }

    public ChordVoicing GenerateNext(ChordVoicingRequest request, ChordVoicing previous)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(previous);

        List<Model.Pitch[]> candidates = GenerateCandidates(request);
        if (candidates.Count == 0) {
            throw new InvalidOperationException($"No valid voicing found for {request.Chord.ToHumanString()} after {previous.Chord.ToHumanString()}.");
        }

        int bestScore = int.MaxValue;
        Model.Pitch[]? best = null;

        foreach (Model.Pitch[] candidate in candidates)
        {
            VoicingTransitionAnalysis analysis = AnalyzeTransition(previous, request, candidate);
            if (!analysis.IsValid) {
                continue;
            }

            if (analysis.Score < bestScore)
            {
                bestScore = analysis.Score;
                best = candidate;
            }
        }

        if (best is null) {
            throw new InvalidOperationException("No valid voicing transition found.");
        }

        return new ChordVoicing(request.Chord, best);
    }

    public VoicingTransitionAnalysis AnalyzeTransition(
        ChordVoicing previous,
        ChordVoicingRequest request,
        IReadOnlyList<Model.Pitch> candidate)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(candidate);

        Model.Pitch[] previousNotes = previous.Pitches.OrderBy(x => x).ToArray();
        Model.Pitch[] newNotes = candidate.OrderBy(x => x).ToArray();

        int nbCommon = previousNotes.Intersect(newNotes).Count();
        (int TotalMovement, bool IsValid) matching =
            MinimumMovementMatching(previousNotes, newNotes, _settings.MaxVoiceMovement);

        if (!matching.IsValid)
        {
            return new VoicingTransitionAnalysis(
                previousNotes.Intersect(newNotes).OrderBy(x => x).ToArray(),
                0,
                0,
                int.MaxValue,
                false);
        }

        int bassMovement = Math.Abs(newNotes[0] - previousNotes[0]);
        if (bassMovement > _settings.MaxVoiceMovement)
        {
            return new VoicingTransitionAnalysis(
                previousNotes.Intersect(newNotes).OrderBy(x => x).ToArray(),
                matching.TotalMovement,
                bassMovement,
                int.MaxValue,
                false);
        }

        int score =
            -nbCommon * _settings.CommonNoteWeight
            + matching.TotalMovement * _settings.MovementWeight
            + bassMovement * _settings.BassMovementWeight;

        if (_preferences is not null)
        {
            foreach (VoicingPreference preference in _preferences.Find(previous.Chord, request.Chord))
            {
                if (preference.FirstChord is not null &&
                    preference.SecondChord is not null &&
                    preference.FirstChord.Pitches.SequenceEqual(previousNotes) &&
                    preference.SecondChord.Pitches.SequenceEqual(newNotes))
                {
                    score -= _settings.UserPreferenceWeight;
                    break;
                }
            }
        }

        return new VoicingTransitionAnalysis(
            previousNotes.Intersect(newNotes).OrderBy(x => x).ToArray(),
            matching.TotalMovement,
            bassMovement,
            score,
            true);
    }

    private int ScoreTransition(
        ChordVoicing previous,
        ChordVoicingRequest request,
        IReadOnlyList<Model.Pitch> candidate)
    {
        return AnalyzeTransition(previous, request, candidate).Score;
    }

    private List<Model.Pitch[]> GenerateCandidates(ChordVoicingRequest request)
    {
        Model.Pitch[] pitchClasses = GetChordPitchClasses(request.Chord)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        if (pitchClasses.Length < 2) {
            throw new InvalidOperationException("A chord must contain at least two distinct notes.");
        }

        if (pitchClasses.Length > _settings.ChordMaxKeyCount) {
            pitchClasses = ReduceToMaximumVoices(request.Chord, pitchClasses, _settings.ChordMaxKeyCount);
        }

        if (request.Bass is not null && !pitchClasses.Contains(request.Bass.Value.ChromaticPitch)) {
            throw new InvalidOperationException(
                $"Bass {request.Bass.Value.Note} is not a note of chord {request.Chord.ToHumanString()}.");
        }

        List<Model.Pitch[]> options = new();
        Model.Pitch[] partial = new Model.Pitch[pitchClasses.Length];
        GenerateAscending(0, Pitch.MinValue, pitchClasses, request.Bass, partial, new bool[pitchClasses.Length], options);
        return options;
    }

    private void GenerateAscending(
        int index,
        Model.Pitch previousMidi,
        Model.Pitch[] pitchClasses,
        PitchClass? bass,
        Model.Pitch[] partial,
        bool[] usedPitchClasses,
        List<Model.Pitch[]> result)
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
                pitchClasses[pitchClassIndex] != bass.Value.ChromaticPitch)
            {
                continue;
            }

            foreach (Model.Pitch midi in EnumerateMidiNotes(pitchClasses[pitchClassIndex]))
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

    private IEnumerable<Model.Pitch> EnumerateMidiNotes(Model.Pitch pitchClass)
    {
        Model.Pitch first = _settings.MinChordAreaNote + ((pitchClass - _settings.MinChordAreaNote) % 12 + 12) % 12;
        for (Model.Pitch midi = first; midi <= _settings.MaxChordAreaNote; midi += 12) {
            yield return midi;
        }
    }

    private ChordVoicing ValidateExactVoicing(ChordVoicingRequest request)
    {
        Model.Pitch[] notes = request.ExactVoicing!.Select(p => p).OrderBy(x => x).ToArray();
        ValidateVoicingNotes(request, notes);
        return new ChordVoicing(request.Chord, request.ExactVoicing!);
    }

    private void ValidateVoicingNotes(ChordVoicingRequest request, IReadOnlyList<Model.Pitch> notes)
    {
        if (notes.Count < 2 || notes.Count > _settings.ChordMaxKeyCount) {
            throw new ArgumentException("A voicing must contain between 2 and 5 notes.");
        }

        if (notes.Any(n => n < _settings.MinChordAreaNote || n > _settings.MaxChordAreaNote)) {
            throw new ArgumentException("The exact voicing is outside the configured register.");
        }

        if (notes.Zip(notes.Skip(1), (a, b) => b - a).Any(d => d <= 0)) {
            throw new ArgumentException("A voicing may not contain duplicated MIDI notes.");
        }

        HashSet<Model.Pitch> chordPitchClasses = GetChordPitchClasses(request.Chord).ToHashSet();
        if (notes.Any(n => !chordPitchClasses.Contains(n % 12))) {
            throw new ArgumentException("The exact voicing contains a note that does not belong to the chord.");
        }

        if (request.Bass is not null && notes[0] % 12 != request.Bass.Value.ChromaticPitch) {
            throw new ArgumentException("The exact voicing does not respect the requested bass note.");
        }
    }

    private static Model.Pitch[] ReduceToMaximumVoices(Chord chord, Model.Pitch[] pitchClasses, int maxVoices)
    {
        // Keep root, third (when present), seventh (when present), then remaining chord tones.
        // This is deliberately conservative: future versions can expose an explicit omission policy.
        Model.Pitch root = Pitch.FromMidi(((int)chord.RootNote + (int)chord.RootNoteAlteration) % 12);
        Model.Pitch[] ordered = pitchClasses
            .OrderBy(pc => pc == root ? 0 : 1)
            .ThenBy(pc => pc)
            .Take(maxVoices)
            .ToArray();
        return ordered;
    }

    private static IEnumerable<Model.Pitch> GetChordPitchClasses(Chord chord)
    {
        int root = ((int)chord.RootNote + (int)chord.RootNoteAlteration) %  12;
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

        IEnumerable <Model.Pitch> chordPitchClasses = intervals.Select(interval => (root + interval) % 12).Select(Pitch.FromMidi);

        return chordPitchClasses;
    }

    private static (int TotalMovement, bool IsValid) MinimumMovementMatching(
        IReadOnlyList<Model.Pitch> previous,
        IReadOnlyList<Model.Pitch> current,
        int maxMovement)
    {
        if (previous.Count == 0 || current.Count == 0) {
            return (0, false);
        }

        // Notes that disappear do not have to be close to a note in the new
        // voicing: a voice is allowed to disappear when the chord has fewer
        // tones. Likewise, when a new voice is added, it does not have to be
        // one of the voices matched below; it only has to be reachable from
        // the previous hand position. This is deliberately independent from
        // the number of common pitch classes.
        if (current.Count > previous.Count)
        {
            foreach (Model.Pitch added in current)
            {
                if (!previous.Any(old => Math.Abs(old - added) <= maxMovement)) {
                    return (0, false);
                }
            }
        }

        int[,] memo = new int[previous.Count + 1, 1 << current.Count];
        bool[,] seen = new bool[previous.Count + 1, 1 << current.Count];

        const int Impossible = 1_000_000;

        int Solve(int index, int usedMask)
        {
            if (index == previous.Count) {
                return 0;
            }

            if (seen[index, usedMask]) {
                return memo[index, usedMask];
            }

            seen[index, usedMask] = true;
            int best = Impossible;

            // A previous voice may disappear when the new chord has fewer
            // voices. This is not a failure: preserving common keys is a
            // preference, not an absolute requirement.
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
        return (movement, movement < Impossible);
    }

    private static int Span(IReadOnlyList<Model.Pitch> notes) {
        return notes[^1] - notes[0];
    }
}
