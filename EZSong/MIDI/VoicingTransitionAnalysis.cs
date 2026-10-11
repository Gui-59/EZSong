using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Diagnostic information about a transition between two chord voicings.
/// The values use the musical <see cref="Pitch"/> type so callers do not need
/// to deal with raw MIDI note numbers.
/// </summary>
public sealed class VoicingTransitionAnalysis
{
    public IReadOnlyList<Pitch> CommonNotes { get; }
    public int TotalMovement { get; }
    public int BassMovement { get; }
    public int Score { get; }
    public bool IsValid { get; }

    public VoicingTransitionAnalysis(
        IReadOnlyList<Pitch> commonNotes,
        int totalMovement,
        int bassMovement,
        int score,
        bool isValid)
    {
        CommonNotes = commonNotes;
        TotalMovement = totalMovement;
        BassMovement = bassMovement;
        Score = score;
        IsValid = isValid;
    }
}
