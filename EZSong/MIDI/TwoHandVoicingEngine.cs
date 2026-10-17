using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Generates and globally optimises actual two-hand voicings. The hand ranges
/// are constraints of the voicing search itself, not a post-export split.
/// </summary>
public sealed class TwoHandVoicingEngine
{
    private readonly TwoHandVoicingSettings _settings;
    private readonly VoicingPreferenceStore? _preferences;

    public TwoHandVoicingEngine(
        TwoHandVoicingSettings? settings = null,
        VoicingPreferenceStore? preferences = null)
    {
        _settings = settings ?? new TwoHandVoicingSettings();
        _settings.Validate();
        _preferences = preferences;
    }

    public IReadOnlyList<TwoHandChordVoicing> GenerateProgression(
        IReadOnlyList<ChordVoicingRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);
        if (requests.Count == 0) {
            return Array.Empty<TwoHandChordVoicing>();
        }

        List<TwoHandChordVoicing>[] layers = new List<TwoHandChordVoicing>[requests.Count];
        for (int i = 0; i < requests.Count; i++)
        {
            layers[i] = GenerateCandidates(requests[i]);
            if (layers[i].Count == 0) {
                throw new InvalidOperationException($"No valid two-hand voicing found for {requests[i].Chord.ToHumanString()}.");
            }
        }

        const long Impossible = long.MaxValue / 4;
        long[] previous = new long[layers[0].Count];
        Array.Fill(previous, 0L);
        int[][] predecessors = new int[requests.Count][];
        predecessors[0] = Enumerable.Repeat(-1, layers[0].Count).ToArray();

        for (int layer = 1; layer < requests.Count; layer++)
        {
            long[] current = new long[layers[layer].Count];
            Array.Fill(current, Impossible);
            predecessors[layer] = Enumerable.Repeat(-1, layers[layer].Count).ToArray();

            for (int c = 0; c < layers[layer].Count; c++)
            {
                for (int p = 0; p < layers[layer - 1].Count; p++)
                {
                    if (previous[p] >= Impossible) {
                        continue;
                    }
                    if (!TryScoreTransition(layers[layer - 1][p], layers[layer][c], out int score)) {
                        continue;
                    }

                    long cost = previous[p] + score;
                    if (cost < current[c])
                    {
                        current[c] = cost;
                        predecessors[layer][c] = p;
                    }
                }
            }

            if (!current.Any(x => x < Impossible)) {
                throw new InvalidOperationException($"No valid two-hand transition found from {requests[layer - 1].Chord.ToHumanString()} to {requests[layer].Chord.ToHumanString()}.");
            }

            previous = current;
        }

        int best = Array.IndexOf(previous, previous.Min());
        int[] selected = new int[requests.Count];
        selected[^1] = best;
        for (int i = requests.Count - 1; i > 0; i--) {
            selected[i - 1] = predecessors[i][selected[i]];
        }

        return selected.Select((index, layer) => layers[layer][index]).ToArray();
    }

    public TwoHandChordVoicing GenerateFirst(ChordVoicingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GenerateCandidates(request).OrderBy(v => Span(v.Pitches)).ThenBy(v => v.Pitches.Max()).First();
    }

    private List<TwoHandChordVoicing> GenerateCandidates(ChordVoicingRequest request)
    {
        ChordForCurrentChord = request.Chord;
        Model.Pitch[] classes = GetChordPitchClasses(request.Chord).Distinct().OrderBy(x => x).ToArray();
        if (classes.Length < 2) {
            throw new InvalidOperationException("A chord must contain at least two distinct notes.");
        }

        if (classes.Length > _settings.MaxTotalVoices) {
            classes = ReduceToMaximumVoices(request.Chord, classes, _settings.MaxTotalVoices);
        }

        if (request.ExactVoicing is not null)
            {
            return ValidateExact(request);
        }

        List<TwoHandChordVoicing> result = new();
        Model.Pitch[] notes = new Model.Pitch[classes.Length];
        bool[] used = new bool[classes.Length];
        GenerateNotes(0, Pitch.MinValue, classes, request.Bass, notes, used, result);
        return result;
    }

    private void GenerateNotes(int index, Model.Pitch previous, Model.Pitch[] classes,
        PitchClass? bass, Model.Pitch[] notes, bool[] used, List<TwoHandChordVoicing> result)
    {
        if (index == classes.Length)
        {
            int leftCount = notes.Count(IsLeft);
            int rightCount = notes.Length - leftCount;
            if (leftCount < _settings.MinLeftHandVoices || leftCount > _settings.MaxLeftHandVoices ||
                rightCount < _settings.MinRightHandVoices || rightCount > _settings.MaxRightHandVoices)
                {
                return;
            }

            if (bass is not null && notes[0] % 12 != bass.Value.ChromaticPitch) {
                return;
            }

            result.Add(new TwoHandChordVoicing(
                ChordForCurrentChord, notes.Where(IsLeft), notes.Where(IsRight)));
            return;
        }

        for (int i = 0; i < classes.Length; i++)
        {
            if (used[i]) {
                continue;
            }
            if (index == 0 && bass is not null && classes[i] != bass.Value.ChromaticPitch) {
                continue;
            }

            foreach (Model.Pitch pitch in EnumerateHandNotes(classes[i]))
            {
                if (pitch <= previous) {
                    continue;
                }
                used[i] = true;
                notes[index] = pitch;
                GenerateNotes(index + 1, pitch, classes, bass, notes, used, result);
                used[i] = false;
            }
        }
    }

    // The recursive candidate builder is scoped to one request; this field avoids
    // threading the Chord through every recursive call.
    private Chord ChordForCurrentChord { get; set; } = null!;

    private IEnumerable<Model.Pitch> EnumerateHandNotes(Model.Pitch pitchClass)
    {
        Model.Pitch first = _settings.MinLeftHandNote + ((pitchClass - _settings.MinLeftHandNote) % 12 + 12) % 12;
        for (Model.Pitch p = first; p <= _settings.MaxLeftHandNote; p += 12) {
            yield return p;
        }

        first = _settings.MinRightHandNote + ((pitchClass - _settings.MinRightHandNote) % 12 + 12) % 12;
        for (Model.Pitch p = first; p <= _settings.MaxRightHandNote; p += 12) {
            yield return p;
        }
    }

    private bool IsLeft(Model.Pitch p) {
        return p <= _settings.MaxLeftHandNote;
    }

    private bool IsRight(Model.Pitch p) {
        return p >= _settings.MinRightHandNote;
    }

    private List<TwoHandChordVoicing> ValidateExact(ChordVoicingRequest request)
    {
        Model.Pitch[] notes = request.ExactVoicing!.OrderBy(x => x).ToArray();
        if (notes.Length < 2 || notes.Length > _settings.MaxTotalVoices)
            {
            throw new ArgumentException("The exact voicing has an invalid number of notes.");
        }
        if (notes.Zip(notes.Skip(1), (a, b) => b - a).Any(d => d <= 0))
            {
            throw new ArgumentException("An exact voicing may not contain duplicate MIDI notes.");
        }
        if (notes.Any(p => !IsLeft(p) && !IsRight(p)))
            {
            throw new ArgumentException("The exact voicing contains a note outside both hand ranges.");
        }
        if (request.Bass is not null && notes[0] % 12 != request.Bass.Value.ChromaticPitch)
            {
            throw new ArgumentException("The exact voicing does not respect the requested bass note.");
        }
        if (notes.Count(IsLeft) < _settings.MinLeftHandVoices || notes.Count(IsLeft) > _settings.MaxLeftHandVoices)
            {
            throw new ArgumentException("The exact voicing violates the left-hand voice count.");
        }
        if (notes.Count(IsRight) < _settings.MinRightHandVoices || notes.Count(IsRight) > _settings.MaxRightHandVoices)
            {
            throw new ArgumentException("The exact voicing violates the right-hand voice count.");
        }

        return new() { new TwoHandChordVoicing(request.Chord, notes.Where(IsLeft), notes.Where(IsRight)) };
    }

    public VoicingTransitionAnalysis AnalyzeTransition(
        TwoHandChordVoicing previous,
        TwoHandChordVoicing current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        Pitch[] previousNotes = previous.Pitches.OrderBy(x => x).ToArray();
        Pitch[] currentNotes = current.Pitches.OrderBy(x => x).ToArray();
        int common = previousNotes.Intersect(currentNotes).Count();

        VoiceLeadingMatch matching = VoiceLeadingMatcher.Match(
            previousNotes, currentNotes, _settings.MaxVoiceMovement);

        if (!matching.IsValid)
        {
            return new VoicingTransitionAnalysis(
                previousNotes.Intersect(currentNotes).OrderBy(x => x).ToArray(),
                0, 0, int.MaxValue, false);
        }

        int bassMovement = Math.Abs(currentNotes[0] - previousNotes[0]);
        if (bassMovement > _settings.MaxVoiceMovement)
        {
            return new VoicingTransitionAnalysis(
                previousNotes.Intersect(currentNotes).OrderBy(x => x).ToArray(),
                matching.TotalMovement, bassMovement, int.MaxValue, false, matching.Movements);
        }

        int score =
            -common * _settings.CommonNoteWeight
            + matching.TotalMovement * _settings.MovementWeight
            + bassMovement * _settings.BassMovementWeight;

        if (_preferences is not null)
        {
            foreach (VoicingPreference preference in _preferences.Find(previous.Chord, current.Chord))
            {
                if (preference.FirstChord is not null && preference.SecondChord is not null &&
                    preference.FirstChord.Pitches.SequenceEqual(previousNotes) &&
                    preference.SecondChord.Pitches.SequenceEqual(currentNotes))
                {
                    score -= _settings.UserPreferenceWeight;
                    break;
                }
            }
        }

        return new VoicingTransitionAnalysis(
            previousNotes.Intersect(currentNotes).OrderBy(x => x).ToArray(),
            matching.TotalMovement, bassMovement, score, true, matching.Movements);
    }

    private bool TryScoreTransition(TwoHandChordVoicing previous, TwoHandChordVoicing current, out int score)
    {
        VoicingTransitionAnalysis analysis = AnalyzeTransition(previous, current);
        score = analysis.Score;
        return analysis.IsValid;
    }

    private static int Span(IReadOnlyList<Model.Pitch> notes) {
        Pitch? min = notes.Min();
        Pitch? max = notes.Max();
        if (min is null || max is null) {
            throw new ArgumentException("The notes collection cannot be empty.");
        }
        return max - min;
    }

    private static Model.Pitch[] ReduceToMaximumVoices(Chord chord, Model.Pitch[] pitchClasses, int maxVoices)
    {
        Model.Pitch root = Pitch.FromMidi(((int)chord.RootNote + (int)chord.RootNoteAlteration) % 12);
        return pitchClasses.OrderBy(pc => pc == root ? 0 : 1).ThenBy(pc => pc).Take(maxVoices).ToArray();
    }

    private static IEnumerable<Model.Pitch> GetChordPitchClasses(Chord chord)
    {
        int root = ((int)chord.RootNote + (int)chord.RootNoteAlteration) % 12;
        int[] intervals = chord.ChordType switch
        {
            ChordType.NoneOrMajor => [0,4,7], ChordType.Minor => [0,3,7], ChordType.Seventh => [0,4,7,10],
            ChordType.MinorSeventh => [0,3,7,10], ChordType.MajorSeventh => [0,4,7,11], ChordType.PowerChord => [0,7],
            ChordType.Sixth => [0,4,7,9], ChordType.MinorSixth => [0,3,7,9], ChordType.SuspendedSecond => [0,2,7],
            ChordType.SuspendedFourth => [0,5,7], ChordType.Diminished => [0,3,6], ChordType.Augmented => [0,4,8],
            ChordType.DiminishedSeventh => [0,3,6,9], ChordType.AugmentedSeventh => [0,4,8,10], ChordType.AddSecond => [0,2,4,7],
            ChordType.AddFourth => [0,4,5,7], ChordType.AddSixth => [0,4,7,9], ChordType.AddNinth => [0,4,7,2],
            ChordType.Ninth => [0,4,7,10,2], ChordType.MinorNinth => [0,3,7,10,2], ChordType.MajorNinth => [0,4,7,11,2],
            ChordType.Eleventh => [0,4,7,10,2,5], ChordType.MinorEleventh => [0,3,7,10,2,5], ChordType.MajorEleventh => [0,4,7,11,2,5],
            ChordType.Thirteenth => [0,4,7,10,2,9], ChordType.MinorThirteenth => [0,3,7,10,2,9], ChordType.MajorThirteenth => [0,4,7,11,2,9],
            ChordType.MinorMajorSeventh => [0,3,7,11], ChordType.SixthNinth => [0,4,7,9,2], ChordType.SeventhMinusFive => [0,4,6,10],
            ChordType.SeventhPlusFive => [0,4,8,10], ChordType.MinorSeventhFlatFive => [0,3,6,10],
            _ => throw new ArgumentOutOfRangeException(nameof(chord.ChordType))
        };
        return intervals.Select(i => (root + i) % 12).Select(Pitch.FromMidi);
    }
}
