using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>One musical result entry: chord, rhythm, voicing and voice-leading information.</summary>
public sealed class VoicingProgressionEntry
{
    public int MeasureNumber { get; }
    public Chord Chord { get; }
    public RhythmSimpleElement Duration { get; }
    public ChordVoicing Voicing { get; }
    public IReadOnlyList<VoiceMovement> VoiceMovements { get; }
    public IReadOnlyList<VoicePath> VoicePaths { get; }

    public VoicingProgressionEntry(
        int measureNumber,
        Chord chord,
        RhythmSimpleElement duration,
        ChordVoicing voicing,
        IEnumerable<VoiceMovement>? voiceMovements = null,
        IEnumerable<VoicePath>? voicePaths = null)
    {
        if (measureNumber <= 0) throw new ArgumentOutOfRangeException(nameof(measureNumber));
        MeasureNumber = measureNumber;
        Chord = chord ?? throw new ArgumentNullException(nameof(chord));
        Duration = duration ?? throw new ArgumentNullException(nameof(duration));
        Voicing = voicing ?? throw new ArgumentNullException(nameof(voicing));
        VoiceMovements = voiceMovements?.ToArray() ?? Array.Empty<VoiceMovement>();
        VoicePaths = voicePaths?.ToArray() ?? Array.Empty<VoicePath>();
    }
}

/// <summary>Musical voicing result for a complete one-hand chord progression.</summary>
public sealed class VoicingProgression
{
    public IReadOnlyList<VoicingProgressionEntry> Entries { get; }
    public IReadOnlyList<VoicePath> VoicePaths { get; }

    public VoicingProgression(
        IEnumerable<VoicingProgressionEntry> entries,
        IEnumerable<VoicePath>? voicePaths = null)
    {
        Entries = entries?.ToArray() ?? throw new ArgumentNullException(nameof(entries));
        VoicePaths = voicePaths?.OrderBy(v => v.VoiceId).ToArray() ?? Array.Empty<VoicePath>();
    }
}
