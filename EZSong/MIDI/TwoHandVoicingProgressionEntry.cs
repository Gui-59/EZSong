using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>One musical result entry for a two-hand voicing.</summary>
public sealed class TwoHandVoicingProgressionEntry
{
    public int MeasureNumber { get; }
    public Chord Chord { get; }
    public RhythmSimpleElement Duration { get; }
    public TwoHandChordVoicing Voicing { get; }
    public IReadOnlyList<VoiceMovement> LeftHandVoiceMovements { get; }
    public IReadOnlyList<VoiceMovement> RightHandVoiceMovements { get; }
    public IReadOnlyList<VoicePath> LeftHandVoicePaths { get; }
    public IReadOnlyList<VoicePath> RightHandVoicePaths { get; }

    public TwoHandVoicingProgressionEntry(
        int measureNumber,
        Chord chord,
        RhythmSimpleElement duration,
        TwoHandChordVoicing voicing,
        IEnumerable<VoiceMovement>? leftHandVoiceMovements = null,
        IEnumerable<VoiceMovement>? rightHandVoiceMovements = null,
        IEnumerable<VoicePath>? leftHandVoicePaths = null,
        IEnumerable<VoicePath>? rightHandVoicePaths = null)
    {
        if (measureNumber <= 0) throw new ArgumentOutOfRangeException(nameof(measureNumber));
        MeasureNumber = measureNumber;
        Chord = chord ?? throw new ArgumentNullException(nameof(chord));
        Duration = duration ?? throw new ArgumentNullException(nameof(duration));
        Voicing = voicing ?? throw new ArgumentNullException(nameof(voicing));
        LeftHandVoiceMovements = leftHandVoiceMovements?.ToArray() ?? Array.Empty<VoiceMovement>();
        RightHandVoiceMovements = rightHandVoiceMovements?.ToArray() ?? Array.Empty<VoiceMovement>();
        LeftHandVoicePaths = leftHandVoicePaths?.ToArray() ?? Array.Empty<VoicePath>();
        RightHandVoicePaths = rightHandVoicePaths?.ToArray() ?? Array.Empty<VoicePath>();
    }
}

/// <summary>Musical voicing result for a complete two-hand progression.</summary>
public sealed class TwoHandVoicingProgression
{
    public IReadOnlyList<TwoHandVoicingProgressionEntry> Entries { get; }
    public IReadOnlyList<VoicePath> LeftHandVoicePaths { get; }
    public IReadOnlyList<VoicePath> RightHandVoicePaths { get; }

    public TwoHandVoicingProgression(
        IEnumerable<TwoHandVoicingProgressionEntry> entries,
        IEnumerable<VoicePath>? leftHandVoicePaths = null,
        IEnumerable<VoicePath>? rightHandVoicePaths = null)
    {
        Entries = entries?.ToArray() ?? throw new ArgumentNullException(nameof(entries));
        LeftHandVoicePaths = leftHandVoicePaths?.OrderBy(v => v.VoiceId).ToArray() ?? Array.Empty<VoicePath>();
        RightHandVoicePaths = rightHandVoicePaths?.OrderBy(v => v.VoiceId).ToArray() ?? Array.Empty<VoicePath>();
    }
}
