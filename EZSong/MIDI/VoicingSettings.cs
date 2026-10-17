namespace EZSong.MIDI;

/// <summary>
/// Constraints and optimisation weights used by <see cref="ChordVoicingEngine"/>.
/// </summary>
public sealed class VoicingSettings : VoicingOptimizationSettings
{
    /// <summary>Lowest note allowed for the one-hand chord area.</summary>
    public Model.Pitch MinChordAreaNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 0);

    /// <summary>Highest note allowed for the one-hand chord area.</summary>
    public Model.Pitch MaxChordAreaNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 3);

    /// <summary>Minimum number of distinct chord tones in a generated voicing.</summary>
    public int ChordMinKeyCount { get; set; } = 2;

    /// <summary>Maximum number of distinct chord tones in a generated voicing.</summary>
    public int ChordMaxKeyCount { get; set; } = 5;

    public void Validate()
    {
        ValidateOptimizationSettings();

        if (MinChordAreaNote < Model.Pitch.MinValue || MinChordAreaNote > Model.Pitch.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(MinChordAreaNote));
        if (MaxChordAreaNote < Model.Pitch.MinValue || MaxChordAreaNote > Model.Pitch.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(MaxChordAreaNote));
        if (MinChordAreaNote > MaxChordAreaNote)
            throw new ArgumentException("MinChordAreaNote must be <= MaxChordAreaNote.");

        if (ChordMinKeyCount < 2 || ChordMinKeyCount > 5)
            throw new ArgumentOutOfRangeException(nameof(ChordMinKeyCount));
        if (ChordMaxKeyCount < ChordMinKeyCount || ChordMaxKeyCount > 5)
            throw new ArgumentOutOfRangeException(nameof(ChordMaxKeyCount));
    }
}
