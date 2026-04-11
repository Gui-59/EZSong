using EZSong.Enums;
using EZSong.Model;

namespace EZSong.Exporting.Lilypond {
    public class LilypondConverter : ILilypondConverter {

        public string FormatMeasureMelody(MeasureMelody melody, MeasureData measureData) {
            return melody.ToLilyPondString(measureData);
        }

        public string FormatChordSequence(ChordSequence chordSequence) {
            return chordSequence.ToLilyPondString();
        }

        public string FormatTimeSignature(TimeSignature timeSignature) {
            return timeSignature.ToLilyPondString();
        }

        public string AlterationToLilyPondString(Alteration alteration) {
            switch (alteration) {
                case Alteration.flat:
                    return "es";
                case Alteration.flatflat:
                    return "esee";
                case Alteration.neutral:
                    return "";
                case Alteration.sharp:
                    return "is";
                case Alteration.sharpsharp:
                    return "isis";
            }
            return "?";
        }


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