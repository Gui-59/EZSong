namespace EZSong.MIDI;

/// <summary>
/// Shared optimisation rules used by one-hand and true two-hand voicing engines.
/// A weight of zero disables the corresponding preference without changing any
/// hard musical constraint.
/// </summary>
public abstract class VoicingOptimizationSettings
{
    /// <summary>Maximum movement allowed for a matched voice in semitones.</summary>
    public int MaxVoiceMovement { get; set; } = 11;

    /// <summary>Reward for keeping an exact MIDI key pressed between chords.</summary>
    public int CommonNoteWeight { get; set; } = 100;

    /// <summary>Cost per semitone of total matched movement.</summary>
    public int MovementWeight { get; set; } = 1;

    /// <summary>Additional cost per semitone of lowest-note (bass) movement.</summary>
    public int BassMovementWeight { get; set; } = 3;

    /// <summary>Reward for matching a user's preferred transition.</summary>
    public int UserPreferenceWeight { get; set; } = 75;

    /// <summary>Whether the first chord should prefer a compact voicing.</summary>
    public bool PreferCompactInitialVoicing { get; set; } = true;

    protected void ValidateOptimizationSettings()
    {
        if (MaxVoiceMovement < 0 || MaxVoiceMovement > 127) {
            throw new ArgumentOutOfRangeException(nameof(MaxVoiceMovement));
        }

        if (CommonNoteWeight < 0) {
            throw new ArgumentOutOfRangeException(nameof(CommonNoteWeight));
        }

        if (MovementWeight < 0) {
            throw new ArgumentOutOfRangeException(nameof(MovementWeight));
        }

        if (BassMovementWeight < 0) {
            throw new ArgumentOutOfRangeException(nameof(BassMovementWeight));
        }

        if (UserPreferenceWeight < 0) {
            throw new ArgumentOutOfRangeException(nameof(UserPreferenceWeight));
        }
    }
}
