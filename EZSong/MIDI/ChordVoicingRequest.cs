using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Optional constraints for the realization of one chord.
/// </summary>
public sealed class ChordVoicingRequest {
    public Chord Chord {
        get;
    }

    /// <summary>
    /// If set, this note must be the lowest note of the resulting voicing.
    /// This is how slash chords are represented by the voicing engine.
    /// </summary>
    public PitchClass? Bass {
        get;
    }

    /// <summary>
    /// Optional exact starting voicing. When supplied for the first chord,
    /// these notes are used as-is after validation.
    /// </summary>
    public IReadOnlyList<Pitch>? ExactVoicing {
        get;
    }

    public ChordVoicingRequest(
        Chord chord,
        PitchClass? bass = null,
        IEnumerable<Pitch>? exactVoicing = null) {
        Chord = chord ?? throw new ArgumentNullException(nameof(chord));
        Bass = bass ?? (chord.BassNote is null
            ? null
            : new PitchClass(chord.BassNote.Value, chord.BassNoteAlteration));
        ExactVoicing = exactVoicing?.ToArray();
    }

    public static ChordVoicingRequest FromExactVoicing(
        ChordVoicing voicing) {
        return new(voicing.Chord, exactVoicing: voicing.Pitches);
    }

    public static ChordVoicingRequest FromExactVoicing(
        Chord chord,
        params Pitch[] notes) {
        return new(chord, exactVoicing: notes);
    }

    public static ChordVoicingRequest FromBass(
        Chord chord,
        NoteStep note,
        Alteration alteration = Alteration.neutral) {
        return new(chord, new PitchClass(note, alteration));
    }
}

public readonly record struct PitchClass(NoteStep Note, Alteration Alteration) {
    public Pitch ChromaticPitch {
        get {
            return Pitch.FromMidi(((int)Note + (int)Alteration + 12) % 12);
        }
    }
}
