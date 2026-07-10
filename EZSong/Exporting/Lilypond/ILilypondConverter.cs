using EZSong.Enums;
using EZSong.Model;

namespace EZSong.Exporting.Lilypond {
    public interface ILilypondConverter {

        string MeasureMelodyToLilyPondString(MeasureMelody melody, MeasureData measureData);
        string ChordSequenceToLilyPondString(ChordSequence chordSequence);
        string TimeSignatureToLilyPondString(TimeSignature timeSignature);

        string AlterationToLilyPondString(Alteration alteration);
        string NoteStepToLilyPondString(NoteStep note);
        string SongModeToLilyPondString(SongMode mode);
    }
}