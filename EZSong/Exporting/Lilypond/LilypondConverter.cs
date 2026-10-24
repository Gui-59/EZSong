using EZSong.Enums;
using EZSong.Model;

namespace EZSong.Exporting.Lilypond {

    /// <summary>
    /// Convert musical elements to Lilypond format
    /// </summary>
    public class LilypondConverter : ILilypondConverter {

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="melody"></param>
        /// <param name="measureData"></param>
        /// <returns></returns>
        public string MeasureMelodyToLilyPondString(MeasureMelody melody, MeasureData measureData) {
            return melody.ToLilyPondString(measureData);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="chordSequence"></param>
        /// <returns></returns>
        public string ChordSequenceToLilyPondString(ChordSequence chordSequence) {
            return chordSequence.ToLilyPondString();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="timeSignature"></param>
        /// <returns></returns>
        public string TimeSignatureToLilyPondString(TimeSignature timeSignature) {
            return timeSignature.ToLilyPondString();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="alteration"></param>
        /// <returns></returns>
        public string AlterationToLilyPondString(Alteration alteration) {
            switch (alteration) {
                case Alteration.flat:
                    return "es";
                case Alteration.neutral:
                    return "";
                case Alteration.sharp:
                    return "is";
            }
            return "?";
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public string NoteStepToLilyPondString(NoteStep note) {
            switch (note) {
                case NoteStep.C:
                    return "c";
                case NoteStep.D:
                    return "d";
                case NoteStep.E:
                    return "e";
                case NoteStep.F:
                    return "f";
                case NoteStep.G:
                    return "g";
                case NoteStep.A:
                    return "a";
                case NoteStep.B:
                    return "b";
            }
            return "?";
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string SongModeToLilyPondString(SongMode mode) {
            switch (mode) {
                case SongMode.minor:
                    return "\\minor";
                case SongMode.major:
                    return "\\major";
            }
            return "?";
        }

    }
}