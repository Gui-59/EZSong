namespace EZSong.MIDI;

/// <summary>Complete persistent voice identity information for a progression.</summary>
public sealed class VoiceTrackingResult
{
    public IReadOnlyList<VoiceTrackedChordVoicing> Voicings { get; }
    public IReadOnlyList<VoicePath> VoicePaths { get; }

    public VoiceTrackingResult(
        IEnumerable<VoiceTrackedChordVoicing> voicings,
        IEnumerable<VoicePath> voicePaths)
    {
        Voicings = voicings?.ToArray() ?? throw new ArgumentNullException(nameof(voicings));
        VoicePaths = voicePaths?.OrderBy(v => v.VoiceId).ToArray()
            ?? throw new ArgumentNullException(nameof(voicePaths));
    }
}
