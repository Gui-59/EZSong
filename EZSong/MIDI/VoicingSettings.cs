namespace EZSong.MIDI;

/// <summary>
/// Constraints and weights used by <see cref="ChordVoicingEngine"/>.
/// </summary>
public sealed class VoicingSettings
{
    /// <summary>Highest MIDI note allowed for the chord part.</summary>
    public Model.Pitch MaxChordAreaNote { get; set; } = new Model.Pitch(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 3);

    /// <summary>
    /// Lowest MIDI note considered. Defaults to C0; the explicit first voicing
    /// is normally what establishes the practical register for a progression.
    /// </summary>
    public Model.Pitch MinChordAreaNote { get; set; } = new Model.Pitch(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 0);

    /// <summary>
    /// Maximum movement of a matched note between two successive voicings.
    /// 11 means that a whole octave jump is forbidden.
    /// </summary>
    public int MaxVoiceMovement { get; set; } = 11;

    /// <summary>Reward for keeping an exact MIDI key pressed between chords.</summary>
    public int CommonNoteWeight { get; set; } = 100;

    /// <summary>Cost per semitone of total movement.</summary>
    public int MovementWeight { get; set; } = 1;

    /// <summary>Additional cost per semitone of bass movement.</summary>
    public int BassMovementWeight { get; set; } = 3;

    /// <summary>Reward for matching a user's preferred transition.</summary>
    public int UserPreferenceWeight { get; set; } = 75;

    /// <summary>Hard limit for the number of chord tones.</summary>
    public int ChordMaxKeyCount { get; set; } = 5;

    /// <summary>Policy used when a chord contains more tones than ChordMaxKeyCount.</summary>
    public ChordTonePriorityPolicy ChordTonePriorityPolicy { get; set; } = ChordTonePriorityPolicy.EssentialsThenExtensions;

    public void Validate()
    {
        if (MinChordAreaNote < Model.Pitch.MinValue || MinChordAreaNote > Model.Pitch.MaxValue) {
            throw new ArgumentOutOfRangeException(nameof(MinChordAreaNote));
        }

        if (MaxChordAreaNote < Model.Pitch.MinValue || MaxChordAreaNote > Model.Pitch.MaxValue) {
            throw new ArgumentOutOfRangeException(nameof(MaxChordAreaNote));
        }

        if (MinChordAreaNote > MaxChordAreaNote) {
            throw new ArgumentException("MinChordAreaNote must be <= MaxChordAreaNote.");
        }

        if (MaxVoiceMovement < 0 || MaxVoiceMovement > 127) {
            throw new ArgumentOutOfRangeException(nameof(MaxVoiceMovement));
        }

        if (ChordMaxKeyCount < 2 || ChordMaxKeyCount > 5) {
            throw new ArgumentOutOfRangeException(nameof(ChordMaxKeyCount));
        }
    }
}
