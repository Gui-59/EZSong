using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// A concrete MIDI realization of a chord. Notes are always ordered from low to high.
/// </summary>
public sealed class ChordVoicing : IEquatable<ChordVoicing> {
    public Chord Chord { get; }
    public IReadOnlyList<Pitch> Notes { get; }

    /// <summary>
    /// Creates a new instance of the ChordVoicing class with the specified chord and notes.
    /// </summary>
    /// <param name="chord">The chord to be realized.</param>
    /// <param name="notes">The notes of the chord. </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ChordVoicing(Chord chord, IEnumerable<Pitch> notes) {
        Chord = chord ?? throw new ArgumentNullException(nameof(chord));
        Notes = notes.OrderBy(GetMidiNoteNumber).ToArray();
    }

    /// <summary>
    /// List of pitches in the chord voicing, ordered from low to high. 
    /// This property provides a read-only view of the notes in the chord voicing.
    /// </summary>
    public IReadOnlyList<Model.Pitch> Pitches {
        get {
            return Notes.Select(n => n).ToArray();
        }
    }

    /// <summary>
    /// Converts the ChordVoicing instance to a string representation suitable for logging, 
    /// displaying the chord and its notes in a human-readable format.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        return $"{Chord.ToHumanString()}: {string.Join(" ", Notes.Select(FormatPitch))}";
    }

    private static int GetMidiNoteNumber(Pitch pitch) { //TODO : utiliser la méthode ToMidiNoteNumber() de Pitch à la place
        return pitch.MidiOctave * 12 + (int)pitch.Note + (int)pitch.Alteration;
    }

    private static string FormatPitch(Pitch pitch) { //TODO : utiliser la méthode ToString() de Pitch à la place
        string accidental = pitch.Alteration switch {
            Alteration.sharp => "#",
            Alteration.flat => "b",
            _ => string.Empty
        };
        return $"{pitch.Note}{accidental}{pitch.MidiOctave}";
    }

    public bool Equals(ChordVoicing? other) {
        if (other is null) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        if (!Chord.Equals(other.Chord)) {
            return false;
        }

        if (Notes.Count != other.Notes.Count) {
            return false;
        }

        for (int i = 0; i < Notes.Count; i++) {
            if (!Notes[i].Equals(other.Notes[i])) {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) {
        if (obj is ChordVoicing other) {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode() {
        HashCode hash = new();
        hash.Add(Chord);
        foreach (Pitch pitch in Notes) {
            hash.Add(pitch);
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(ChordVoicing left, ChordVoicing right) {
        if (ReferenceEquals(left, right)) {
            return true;
        }

        if (left is null || right is null) {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(ChordVoicing left, ChordVoicing right) {
        return !(left == right);
    }
}
