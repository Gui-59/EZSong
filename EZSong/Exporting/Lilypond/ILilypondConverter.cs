using EZSong.Enums;
using EZSong.Serializable;

namespace EZSong.Exporting.Lilypond {
    public interface ILilypondConverter {
        string FormatSong(Song song);
        string FormatMeasureMelody(MeasureMelody melody);
        string FormatChord(Chord chord);
        string FormatChordSequence(ChordSequence chordSequence);
        string FormatPitch(Pitch pitch);
        string FormatTimeSignature(TimeSignature timeSignature);
        string FormatKeySignature(KeySignature keySignature);
        string FormatNoteDuration(NoteDuration noteDuration);

        string AlterationToLilyPondString(Alteration alteration);
        string NoteStepToLilyPondString(NoteStep note);
        string SongModeToLilyPondString(SongMode mode);
        string WholeFractionToLilyPondString(WholeFraction fraction);
    }
}