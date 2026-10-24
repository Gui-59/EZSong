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

    /// <summary>Maximum distance in semitones between the lowest and highest notes. Zero disables this constraint.</summary>
    public int MaxVoicingSpanSemitones { get; set; } = 0;

    /// <summary>Minimum distance in semitones between adjacent notes in a voicing. Zero disables this constraint.</summary>
    public int MinimumNoteSpacingSemitones { get; set; } = 0;

    /// <summary>Whether the first chord should prefer a compact voicing.</summary>
    public bool PreferCompactInitialVoicing { get; set; } = true;

    /// <summary>Returns whether the notes satisfy the configured spacing and total-span constraints.</summary>
    public bool IsVoicingShapeAllowed(IEnumerable<EZSong.Model.Pitch> pitches)
    {
        EZSong.Model.Pitch[] notes = pitches.OrderBy(p => p).ToArray();
        if (notes.Length == 0) return true;
        if (MaxVoicingSpanSemitones > 0 &&
            notes[^1].ToMidiNoteNumber() - notes[0].ToMidiNoteNumber() > MaxVoicingSpanSemitones)
            return false;
        if (MinimumNoteSpacingSemitones > 0 &&
            notes.Zip(notes.Skip(1), (a, b) => b.ToMidiNoteNumber() - a.ToMidiNoteNumber())
                .Any(distance => distance < MinimumNoteSpacingSemitones))
            return false;
        return true;
    }

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
        if (MaxVoicingSpanSemitones < 0 || MaxVoicingSpanSemitones > 127) {
            throw new ArgumentOutOfRangeException(nameof(MaxVoicingSpanSemitones));
        }
        if (MinimumNoteSpacingSemitones < 0 || MinimumNoteSpacingSemitones > 127) {
            throw new ArgumentOutOfRangeException(nameof(MinimumNoteSpacingSemitones));
        }
    }
}
