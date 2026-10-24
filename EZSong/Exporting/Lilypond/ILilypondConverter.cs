using EZSong.Enums;
using EZSong.Model;

namespace EZSong.Exporting.Lilypond {

    /// <summary>
    /// Interface to convert musical elements to LilyPond format
    /// </summary>
    public interface ILilypondConverter {

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="melody"></param>
        /// <param name="measureData"></param>
        /// <returns></returns>
        string MeasureMelodyToLilyPondString(MeasureMelody melody, MeasureData measureData);
        
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="chordSequence"></param>
        /// <returns></returns>
        string ChordSequenceToLilyPondString(ChordSequence chordSequence);
        
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="timeSignature"></param>
        /// <returns></returns>
        string TimeSignatureToLilyPondString(TimeSignature timeSignature);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        string AlterationToLilyPondString(Alteration alteration);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        string NoteStepToLilyPondString(NoteStep note);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        string SongModeToLilyPondString(SongMode mode);
    }
}