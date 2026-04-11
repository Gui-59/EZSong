using EZSong.Enums;
using EZSong.Model;

namespace EZSong.Exporting.Lilypond {
    public interface ILilypondConverter {

        string FormatMeasureMelody(MeasureMelody melody, MeasureData measureData);
        string FormatChordSequence(ChordSequence chordSequence);
        string FormatTimeSignature(TimeSignature timeSignature);

        string AlterationToLilyPondString(Alteration alteration);
        string NoteStepToLilyPondString(NoteStep note);
        string SongModeToLilyPondString(SongMode mode);
    }
}