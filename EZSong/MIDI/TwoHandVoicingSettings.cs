using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Constraints for a true two-hand chord voicing. Unlike MIDI export splitting,
/// these ranges participate directly in voicing optimisation.
/// </summary>
public sealed class TwoHandVoicingSettings
{
    public Pitch MinLeftHandNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 1);
    public Pitch MaxLeftHandNote { get; set; } = new(EZSong.Enums.NoteStep.B, EZSong.Enums.Alteration.neutral, 2);
    public Pitch MinRightHandNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 3);
    public Pitch MaxRightHandNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 5);

    public int MinLeftHandVoices { get; set; } = 1;
    public int MaxLeftHandVoices { get; set; } = 3;
    public int MinRightHandVoices { get; set; } = 1;
    public int MaxRightHandVoices { get; set; } = 4;
    public int MaxTotalVoices { get; set; } = 5;

    /// <summary>Policy used when a chord contains more tones than MaxTotalVoices.</summary>
    public ChordTonePriorityPolicy ChordTonePriorityPolicy { get; set; } = ChordTonePriorityPolicy.EssentialsThenExtensions;

    public int MaxVoiceMovement { get; set; } = 11;
    public int CommonNoteWeight { get; set; } = 100;
    public int MovementWeight { get; set; } = 1;
    public int BassMovementWeight { get; set; } = 3;
    public int UserPreferenceWeight { get; set; } = 75;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(MinLeftHandNote);
        ArgumentNullException.ThrowIfNull(MaxLeftHandNote);
        ArgumentNullException.ThrowIfNull(MinRightHandNote);
        ArgumentNullException.ThrowIfNull(MaxRightHandNote);

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
        if (MaxTotalVoices < 2 || MaxTotalVoices > 5)
            throw new ArgumentOutOfRangeException(nameof(MaxTotalVoices));
        if (MaxLeftHandVoices > MaxTotalVoices || MaxRightHandVoices > MaxTotalVoices)
            throw new ArgumentException("Per-hand maximum cannot exceed MaxTotalVoices.");
        if (MinLeftHandVoices + MinRightHandVoices > MaxTotalVoices)
            throw new ArgumentException("Minimum hand voice counts exceed MaxTotalVoices.");
        if (MaxVoiceMovement < 0 || MaxVoiceMovement > 127)
            throw new ArgumentOutOfRangeException(nameof(MaxVoiceMovement));
    }
}
