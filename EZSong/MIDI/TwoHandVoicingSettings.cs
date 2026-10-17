using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Constraints for a true two-hand chord voicing. Unlike MIDI export splitting,
/// these ranges participate directly in voicing optimisation.
/// </summary>
public sealed class TwoHandVoicingSettings : VoicingOptimizationSettings
{
    public Pitch MinLeftHandNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 1);
    public Pitch MaxLeftHandNote { get; set; } = new(EZSong.Enums.NoteStep.B, EZSong.Enums.Alteration.neutral, 2);
    public Pitch MinRightHandNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 3);
    public Pitch MaxRightHandNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 5);

    public int MinLeftHandVoices { get; set; } = 1;
    public int MaxLeftHandVoices { get; set; } = 3;
    public int MinRightHandVoices { get; set; } = 1;
    public int MaxRightHandVoices { get; set; } = 4;

    /// <summary>Minimum total number of distinct notes in a generated voicing.</summary>
    public int MinTotalVoices { get; set; } = 2;

    /// <summary>Maximum total number of distinct notes in a generated voicing.</summary>
    public int MaxTotalVoices { get; set; } = 5;

    public void Validate()
    {
        ValidateOptimizationSettings();

        if (MinLeftHandNote > MaxLeftHandNote)
            throw new ArgumentException("MinLeftHandNote must be <= MaxLeftHandNote.");
        if (MinRightHandNote > MaxRightHandNote)
            throw new ArgumentException("MinRightHandNote must be <= MaxRightHandNote.");
        if (MaxLeftHandNote >= MinRightHandNote)
            throw new ArgumentException("The left-hand range must end below the right-hand range.");

        if (MinLeftHandVoices < 0 || MaxLeftHandVoices < MinLeftHandVoices)
            throw new ArgumentException("Invalid left-hand voice count range.");
        if (MinRightHandVoices < 0 || MaxRightHandVoices < MinRightHandVoices)
            throw new ArgumentException("Invalid right-hand voice count range.");

        if (MinTotalVoices < 2 || MinTotalVoices > 5)
            throw new ArgumentOutOfRangeException(nameof(MinTotalVoices));
        if (MaxTotalVoices < MinTotalVoices || MaxTotalVoices > 5)
            throw new ArgumentOutOfRangeException(nameof(MaxTotalVoices));
        if (MaxLeftHandVoices > MaxTotalVoices || MaxRightHandVoices > MaxTotalVoices)
            throw new ArgumentException("Per-hand maximum cannot exceed MaxTotalVoices.");
        if (MinLeftHandVoices + MinRightHandVoices > MaxTotalVoices)
            throw new ArgumentException("Minimum hand voice counts exceed MaxTotalVoices.");
    }
}
