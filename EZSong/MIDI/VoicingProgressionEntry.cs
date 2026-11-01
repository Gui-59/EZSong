using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// A class that represents a single entry in a musical voicing progression,
/// </summary>
public sealed class VoicingProgressionEntry {

    /// <summary>
    /// Number of the measure in which this entry occurs, starting from 1.
    /// </summary>
    public int MeasureNumber {
        get;
    }

    /// <summary>
    /// Chord associated with this entry, representing the harmonic content of the measure.
    /// </summary>
    public Chord Chord {
        get;
    }

    /// <summary>
    /// Duration of the chord in this entry, represented as a RhythmSimpleElement, indicating how long the chord is held.
    /// </summary>
    public RhythmSimpleElement Duration {
        get;
    }

    /// <summary>
    /// Chord voicing for this entry, representing the specific arrangement of notes in the chord.
    /// </summary>
    public ChordVoicing Voicing {
        get;
    }

    /// <summary>
    /// List of voice movements associated with this entry, 
    /// representing the transitions of individual voices from the previous chord to the current chord.
    /// </summary>
    public IReadOnlyList<VoiceMovement> VoiceMovements {
        get;
    }

    /// <summary>
    /// List of voice paths associated with this entry, 
    /// representing the paths taken by individual voices through the chord progression.
    /// </summary>
    public IReadOnlyList<VoicePath> VoicePaths {
        get;
    }

    /// <summary>
    /// Constructs a new instance of the VoicingProgressionEntry class with the specified parameters,
    /// </summary>
    /// <param name="measureNumber">Number of the measure in which this entry occurs, starting from 1.  </param>
    /// <param name="chord">Chord associated with this entry, representing the harmonic content of the measure.</param>
    /// <param name="duration">Duration of the chord in this entry, represented as a RhythmSimpleElement, indicating how long the chord is held.</param>
    /// <param name="voicing">Chord voicing for this entry, representing the specific arrangement of notes in the chord.</param>
    /// <param name="voiceMovements">List of voice movements associated with this entry, representing the transitions of individual voices from the previous chord to the current chord.</param>
    /// <param name="voicePaths">List of voice paths associated with this entry, representing the paths taken by individual voices through the chord progression.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public VoicingProgressionEntry(
        int measureNumber,
        Chord chord,
        RhythmSimpleElement duration,
        ChordVoicing voicing,
        IEnumerable<VoiceMovement>? voiceMovements = null,
        IEnumerable<VoicePath>? voicePaths = null) {
        if (measureNumber <= 0) {
            throw new ArgumentOutOfRangeException(nameof(measureNumber));
        }

        MeasureNumber = measureNumber;
        Chord = chord ?? throw new ArgumentNullException(nameof(chord));
        Duration = duration ?? throw new ArgumentNullException(nameof(duration));
        Voicing = voicing ?? throw new ArgumentNullException(nameof(voicing));
        VoiceMovements = voiceMovements?.ToArray() ?? Array.Empty<VoiceMovement>();
        VoicePaths = voicePaths?.ToArray() ?? Array.Empty<VoicePath>();
    }
}

//TODO : move this class to a separate file
public sealed class VoicingProgression {
    public IReadOnlyList<VoicingProgressionEntry> Entries {
        get;
    }
    public IReadOnlyList<VoicePath> VoicePaths {
        get;
    }

    public VoicingProgression(
        IEnumerable<VoicingProgressionEntry> entries,
        IEnumerable<VoicePath>? voicePaths = null) {
        Entries = entries?.ToArray() ?? throw new ArgumentNullException(nameof(entries));
        VoicePaths = voicePaths?.OrderBy(v => v.VoiceId).ToArray() ?? Array.Empty<VoicePath>();
    }
}
