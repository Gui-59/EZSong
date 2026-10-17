namespace EZSong.MIDI;

/// <summary>
/// Determines which chord tones survive when a chord contains more distinct
/// tones than the configured maximum number of voices.
/// </summary>
public enum ChordTonePriorityPolicy
{
    /// <summary>
    /// Keep the harmonic essentials first, then extensions, and omit the fifth
    /// before useful extensions on extended chords. Slash bass is always kept.
    /// </summary>
    EssentialsThenExtensions,

    /// <summary>
    /// Prefer the fifth before extensions. This is useful when a fuller,
    /// traditional chord spelling is desired.
    /// </summary>
    EssentialsThenFifth,
}
