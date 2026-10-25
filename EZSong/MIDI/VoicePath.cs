namespace EZSong.MIDI;

/// <summary>
/// Persistent identity of one musical voice through a complete progression.
/// A null note means that the voice is absent at that chord position.
/// </summary>
public sealed class VoicePath {
    public int VoiceId {
        get;
    }
    public IReadOnlyList<VoicePathPoint> Points {
        get;
    }

    public VoicePath(int voiceId, IEnumerable<VoicePathPoint> points) {
        if (voiceId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(voiceId));
        }

        VoiceId = voiceId;
        Points = points?.ToArray() ?? throw new ArgumentNullException(nameof(points));
    }

    public override string ToString() {
        return $"Voice {VoiceId}: {string.Join(" | ", Points.Select(p => p.Note?.ToString() ?? "-"))}";
    }
}
