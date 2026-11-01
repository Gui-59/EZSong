using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Diagnostic information about a transition between two chord voicings.
/// The values use the musical <see cref="Pitch"/> type so callers do not need
/// to deal with raw MIDI note numbers.
/// </summary>
public sealed class VoicingTransitionAnalysis {
    public IReadOnlyList<Pitch> CommonNotes {
        get;
    }

    /// <summary>
    /// Count of semitones moved by all voices in the transition, including the bass voice.
    /// </summary>
    public int TotalMovement {
        get;
    }

    /// <summary>
    /// Count of semitones moved by the bass voice in the transition.
    /// </summary>
    public int BassMovement {
        get;
    }

    /// <summary>
    /// Score of the transition, where a higher score indicates a better transition. 
    /// The scoring system is based on musical theory and voice leading principles.
    /// </summary>
    public int Score {
        get;
    }
    public bool IsValid {
        get;
    }
    public IReadOnlyList<VoiceMovement> VoiceMovements {
        get;
    }

    public VoicingTransitionAnalysis(
        IReadOnlyList<Pitch> commonNotes,
        int totalMovement,
        int bassMovement,
        int score,
        bool isValid,
        IReadOnlyList<VoiceMovement>? voiceMovements = null) {
        CommonNotes = commonNotes;
        TotalMovement = totalMovement;
        BassMovement = bassMovement;
        Score = score;
        IsValid = isValid;
        VoiceMovements = voiceMovements ?? Array.Empty<VoiceMovement>();
    }
}
