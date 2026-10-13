using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// One line of a chord progression: a chord, its parsed rhythmic duration,
/// and the measure number in which it occurs.
/// </summary>
public sealed record ChordProgressionEntry(
    Chord Chord,
    RhythmSimpleElement Duration,
    int MeasureNumber);
