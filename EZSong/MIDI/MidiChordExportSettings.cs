using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Controls how generated chord notes are distributed across MIDI tracks.
/// This is an export concern; it does not alter the voicing optimisation.
/// </summary>
public sealed class MidiChordExportSettings
{
    /// <summary>First MIDI channel used by the left-hand track (0..15).</summary>
    public int LeftHandChannel { get; set; } = 0;

    /// <summary>First MIDI channel used by the right-hand track (0..15).</summary>
    public int RightHandChannel { get; set; } = 1;

    /// <summary>
    /// Notes strictly below this pitch are written to the left-hand track.
    /// The split note itself belongs to the right-hand track.
    /// </summary>
    public Pitch SplitPoint { get; set; } =
        new Pitch(EZSong.Enums.NoteStep.C, EZSong.Enums.Alteration.neutral, 3);

    public string LeftHandTrackName { get; set; } = "Main gauche";
    public string RightHandTrackName { get; set; } = "Main droite";

    /// <summary>
    /// When true, create both hand tracks even when one of them contains no notes.
    /// </summary>
    public bool CreateEmptyTracks { get; set; } = true;

    public void Validate()
    {
        ValidateChannel(LeftHandChannel, nameof(LeftHandChannel));
        ValidateChannel(RightHandChannel, nameof(RightHandChannel));
        ArgumentNullException.ThrowIfNull(SplitPoint);

        if (string.IsNullOrWhiteSpace(LeftHandTrackName))
            {
            throw new ArgumentException("Track name cannot be empty.", nameof(LeftHandTrackName));
        }

        if (string.IsNullOrWhiteSpace(RightHandTrackName))
            {
            throw new ArgumentException("Track name cannot be empty.", nameof(RightHandTrackName));
        }
    }

    private static void ValidateChannel(int channel, string parameterName)
    {
        if (channel < 0 || channel > 15)
            {
            throw new ArgumentOutOfRangeException(parameterName, "MIDI channel must be between 0 and 15.");
        }
    }
}
