using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>One note of a chord with its persistent voice identity.</summary>
public sealed class TrackedVoice
{
    public int VoiceId { get; }
    public Pitch Note { get; }

    public TrackedVoice(int voiceId, Pitch note)
    {
        if (voiceId <= 0) throw new ArgumentOutOfRangeException(nameof(voiceId));
        VoiceId = voiceId;
        Note = note;
    }

    public override string ToString() => $"Voice {VoiceId}: {Note}";
}
