namespace EZSong.MIDI;

/// <summary>
/// A class that represents the settings for voicing optimization in a musical context, 
/// including constraints on chord area notes and chord tone priorities.
/// </summary>
public sealed class VoicingSettings : VoicingOptimizationSettings {

    /// <summary>
    /// Highest MIDI note allowed for the chord part.
    /// </summary>
    public Model.Pitch MaxChordAreaNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 3);

    /// <summary>
    /// Lowest MIDI note considered.
    /// </summary>
    public Model.Pitch MinChordAreaNote { get; set; } = new(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 0);

    /// <summary>
    /// Hard limit for the number of chord tones.
    /// </summary>
    public int ChordMaxKeyCount { get; set; } = 5;

    /// <summary>
    /// Policy used when a chord contains more tones than ChordMaxKeyCount.
    /// </summary>
    public ChordTonePriorityPolicy ChordTonePriorityPolicy { get; set; } = ChordTonePriorityPolicy.EssentialsThenExtensions;

    /// <summary>
    /// Validates the current voicing settings, 
    /// ensuring that the minimum chord area note is less than or equal to the maximum chord area note, 
    /// and that the chord maximum key count is within the valid range (2 to 5).
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Validate() {
        ArgumentNullException.ThrowIfNull(MinChordAreaNote);
        ArgumentNullException.ThrowIfNull(MaxChordAreaNote);

        if (MinChordAreaNote > MaxChordAreaNote) {
            throw new ArgumentException("MinChordAreaNote must be <= MaxChordAreaNote.");
        }
        if (ChordMaxKeyCount < 2 || ChordMaxKeyCount > 5) {
            throw new ArgumentOutOfRangeException(nameof(ChordMaxKeyCount));
        }

        ValidateOptimizationSettings();
    }
}
