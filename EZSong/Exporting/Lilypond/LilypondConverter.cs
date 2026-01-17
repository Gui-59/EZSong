using EZSong.Enums;
using EZSong.Serializable;

namespace EZSong.Exporting.Lilypond {
    public class LilypondConverter : ILilypondConverter {
        public string FormatSong(Song song) {
            // Pour l'instant, délégation au Song/LilypondFileBuilder existant si besoin.
            return string.Empty;
        }

        public string FormatMeasureMelody(MeasureMelody melody) {
            return melody.ToLilyPondString();
        }

        public string FormatChord(Chord chord) {
            return chord.ToLilyPondString();
        }

        public string FormatChordSequence(ChordSequence chordSequence) {
            return chordSequence.ToLilyPondString();
        }

        public string FormatPitch(Pitch pitch) {
            return pitch.ToLilyPondString();
        }

        public string FormatTimeSignature(TimeSignature timeSignature) {
            return timeSignature.ToLilyPondString();
        }

        public string FormatKeySignature(KeySignature keySignature) {
            // Laisser ici la logique si vous préférez centraliser plus tard.
            // On peut appeler une méthode dédiée ici quand on aura extrait la logique.
            return string.Empty;
        }

        public string FormatNoteDuration(NoteDuration noteDuration) {
            return noteDuration.ToLilyPondString();
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

        public string WholeFractionToLilyPondString(WholeFraction fraction) {
            switch ((int)fraction) {
                case 1:
                    return "1";
                case 2:
                    return "2";
                case 4:
                    return "4";
                case 8:
                    return "8";
                case 16:
                    return "16";
                case 32:
                    return "32";
                default:
                    return "?";
            }

        }
    }
}