using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Generates and globally optimises actual two-hand voicings. The hand ranges
/// are constraints of the voicing search itself, not a post-export split.
/// </summary>
public sealed class TwoHandVoicingEngine {
    private readonly TwoHandVoicingSettings _settings;
    private readonly VoicingPreferenceStore? _preferences;

    public TwoHandVoicingEngine(
        TwoHandVoicingSettings? settings = null,
        VoicingPreferenceStore? preferences = null) {
        _settings = settings ?? new TwoHandVoicingSettings();
        _settings.Validate();
        _preferences = preferences;
    }

    public int MaxVoiceMovement {
        get {
            return _settings.MaxVoiceMovement;
        }
    }

    public TwoHandVoicingProgression GenerateVoicingProgression(
        IReadOnlyList<ChordProgressionEntry> entries,
        IReadOnlyList<ChordVoicingRequest> requests) {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(requests);
        if (entries.Count != requests.Count) {
            throw new ArgumentException("The progression entry count must match the request count.");
        }

        IReadOnlyList<TwoHandChordVoicing> voicings = GenerateProgression(requests);
        IReadOnlyList<ChordVoicing> left = voicings
            .Select(v => new ChordVoicing(v.Chord, v.LeftHand))
            .ToArray();
        IReadOnlyList<ChordVoicing> right = voicings
            .Select(v => new ChordVoicing(v.Chord, v.RightHand))
            .ToArray();

        VoiceTrackingResult leftTracking = VoiceIdentityTracker.TrackProgression(left, _settings.MaxVoiceMovement);
        VoiceTrackingResult rightTracking = VoiceIdentityTracker.TrackProgression(right, _settings.MaxVoiceMovement);

        return VoicingProgressionBuilder.BuildTwoHands(
            entries,
            voicings,
            leftTracking.VoicePaths,
            rightTracking.VoicePaths,
            _settings.MaxVoiceMovement);
    }

    public IReadOnlyList<TwoHandChordVoicing> GenerateProgression(
        IReadOnlyList<ChordVoicingRequest> requests) {
        ArgumentNullException.ThrowIfNull(requests);
        if (requests.Count == 0) {
            return Array.Empty<TwoHandChordVoicing>();
        }

        List<TwoHandChordVoicing>[] layers = new List<TwoHandChordVoicing>[requests.Count];
        for (int i = 0; i < requests.Count; i++) {
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

        for (int layer = 1; layer < requests.Count; layer++) {
            long[] current = new long[layers[layer].Count];
            Array.Fill(current, Impossible);
            predecessors[layer] = Enumerable.Repeat(-1, layers[layer].Count).ToArray();

            for (int c = 0; c < layers[layer].Count; c++) {
                for (int p = 0; p < layers[layer - 1].Count; p++) {
                    if (previous[p] >= Impossible) {
                        continue;
                    }
                    VoicingTransitionAnalysis analysis = AnalyzeTransition(
                        layers[layer - 1][p],
                        layers[layer][c]);
                    if (!analysis.IsValid) {
                        continue;
                    }

                    long cost = previous[p] + analysis.Score;
                    if (cost < current[c]) {
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

    public TwoHandChordVoicing GenerateFirst(ChordVoicingRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        List<TwoHandChordVoicing> candidates = GenerateCandidates(request);
        if (candidates.Count == 0) {
            throw new InvalidOperationException($"No valid two-hand voicing found for {request.Chord.ToHumanString()}.");
        }

        return _settings.PreferCompactInitialVoicing
            ? candidates.OrderBy(v => Span(v.Pitches)).ThenBy(v => v.Pitches.Max()).First()
            : candidates[0];
    }

    private List<TwoHandChordVoicing> GenerateCandidates(ChordVoicingRequest request) {
        Model.Pitch[] classes = ChordPitchClassProvider.Get(request.Chord).ToArray();
        if (classes.Length < 2) {
            throw new InvalidOperationException("A chord must contain at least two distinct notes.");
        }

        if (classes.Length > _settings.MaxTotalVoices) {
            classes = ChordToneSelector.Select(
                request.Chord,
                classes,
                _settings.MaxTotalVoices,
                request.Bass,
                _settings.ChordTonePriorityPolicy);
        }

        if (request.ExactVoicing is not null) {
            return ValidateExact(request);
        }

        List<TwoHandChordVoicing> result = new();
        Model.Pitch[] notes = new Model.Pitch[classes.Length];
        bool[] used = new bool[classes.Length];
        GenerateNotes(request.Chord, 0, Pitch.MinValue, classes, request.Bass, notes, used, result);
        return result.Where(v => _settings.IsVoicingShapeAllowed(v.Pitches)).ToList();
    }

    private void GenerateNotes(Chord chord, int index, Model.Pitch previous, Model.Pitch[] classes,
        PitchClass? bass, Model.Pitch[] notes, bool[] used, List<TwoHandChordVoicing> result) {
        if (index == classes.Length) {
            int leftCount = notes.Count(IsLeft);
            int rightCount = notes.Length - leftCount;
            if (leftCount < _settings.MinLeftHandVoices || leftCount > _settings.MaxLeftHandVoices ||
                rightCount < _settings.MinRightHandVoices || rightCount > _settings.MaxRightHandVoices) {
                return;
            }

            if (bass is not null && notes[0] % 12 != bass.Value.ChromaticPitch) {
                return;
            }

            result.Add(new TwoHandChordVoicing(
                chord, notes.Where(IsLeft), notes.Where(IsRight)));
            return;
        }

        for (int i = 0; i < classes.Length; i++) {
            if (used[i]) {
                continue;
            }
            if (index == 0 && bass is not null && classes[i] != bass.Value.ChromaticPitch) {
                continue;
            }

            foreach (Model.Pitch pitch in EnumerateHandNotes(classes[i])) {
                if (pitch <= previous) {
                    continue;
                }
                used[i] = true;
                notes[index] = pitch;
                GenerateNotes(chord, index + 1, pitch, classes, bass, notes, used, result);
                used[i] = false;
            }
        }
    }

    private IEnumerable<Model.Pitch> EnumerateHandNotes(Model.Pitch pitchClass) {
        Model.Pitch first = _settings.MinLeftHandNote + ((pitchClass - _settings.MinLeftHandNote) % 12 + 12) % 12;
        for (Model.Pitch p = first; p <= _settings.MaxLeftHandNote; p += 12) {
            yield return p;
        }

        first = _settings.MinRightHandNote + ((pitchClass - _settings.MinRightHandNote) % 12 + 12) % 12;
        for (Model.Pitch p = first; p <= _settings.MaxRightHandNote; p += 12) {
            yield return p;
        }
    }

    /// <summary>
    /// Determines if a given pitch is within the left-hand range based on the configured settings.
    /// </summary>
    /// <param name="p">The pitch to check.</param>
    /// <returns>True if the pitch is within the left-hand range; otherwise, false.</returns>
    private bool IsLeft(Model.Pitch p) {
        return p <= _settings.MaxLeftHandNote;
    }

    /// <summary>
    /// Determines if a given pitch is within the right-hand range based on the configured settings.
    /// </summary>
    /// <param name="p">The pitch to check.</param>
    /// <returns>True if the pitch is within the right-hand range; otherwise, false.</returns>
    private bool IsRight(Model.Pitch p) {
        return p >= _settings.MinRightHandNote;
    }

    private List<TwoHandChordVoicing> ValidateExact(ChordVoicingRequest request) {
        Model.Pitch[] notes = request.ExactVoicing!.OrderBy(x => x).ToArray();
        if (notes.Length < 2 || notes.Length > _settings.MaxTotalVoices) {
            throw new ArgumentException("The exact voicing has an invalid number of notes.");
        }
        if (notes.Zip(notes.Skip(1), (a, b) => b - a).Any(d => d <= 0)) {
            throw new ArgumentException("An exact voicing may not contain duplicate MIDI notes.");
        }
        if (notes.Any(p => !IsLeft(p) && !IsRight(p))) {
            throw new ArgumentException("The exact voicing contains a note outside both hand ranges.");
        }
        HashSet<Model.Pitch> chordPitchClasses = ChordPitchClassProvider.Get(request.Chord).ToHashSet();
        if (notes.Any(note => !chordPitchClasses.Contains(note % 12))) {
            throw new ArgumentException("The exact voicing contains a note that does not belong to the chord.");
        }
        if (request.Bass is not null && notes[0] % 12 != request.Bass.Value.ChromaticPitch) {
            throw new ArgumentException("The exact voicing does not respect the requested bass note.");
        }
        if (!_settings.IsVoicingShapeAllowed(notes)) {
            throw new ArgumentException("The exact voicing violates the configured span or note-spacing constraints.");
        }
        if (notes.Count(IsLeft) < _settings.MinLeftHandVoices || notes.Count(IsLeft) > _settings.MaxLeftHandVoices) {
            throw new ArgumentException("The exact voicing violates the left-hand voice count.");
        }
        if (notes.Count(IsRight) < _settings.MinRightHandVoices || notes.Count(IsRight) > _settings.MaxRightHandVoices) {
            throw new ArgumentException("The exact voicing violates the right-hand voice count.");
        }

        return new() { new TwoHandChordVoicing(request.Chord, notes.Where(IsLeft), notes.Where(IsRight)) };
    }

    public VoicingTransitionAnalysis AnalyzeTransition(
        TwoHandChordVoicing previous,
        TwoHandChordVoicing current) {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        return VoicingTransitionScorer.Analyze(
            previous.Chord,
            previous.Pitches,
            current.Chord,
            current.Pitches,
            _settings,
            _preferences);
    }

    /// <summary>
    /// Computes the span of a collection of notes, defined as the difference between the highest and lowest MIDI note numbers.
    /// </summary>
    /// <param name="notes">The collection of notes.</param>
    /// <returns>The span of the notes.</returns>
    /// <exception cref="ArgumentException">Thrown if the notes collection is empty.</exception>
    private static int Span(IReadOnlyList<Model.Pitch> notes) {
        Pitch? min = notes.Min();
        Pitch? max = notes.Max();
        if (min is null || max is null) {
            throw new ArgumentException("The notes collection cannot be empty.");
        }
        return max - min;
    }

}
