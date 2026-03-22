using EZSong.Enums;
using EZSong.EnumsStringifier;
using EZSong.Exporting.Lilypond;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong {

    [Serializable]
    public class KeySignature {
        
        public NoteStep Note;
        public Alteration Alteration;
        public SongMode Mode;

        private readonly ILilypondConverter _lilypondSerializer;

        public KeySignature() {
            // Constructeur par défaut (requis pour la sérialisation)
            _lilypondSerializer = new LilypondConverter();
        }

        public KeySignature(string keySignatureDropDownId) {

            _lilypondSerializer = new LilypondConverter();

            //Premier caractère de l'id : la note (hors altération)
            switch (keySignatureDropDownId.Substring(0, 1).ToLowerInvariant()) {
                case "c":
                    Note = NoteStep.C;
                    break;
                case "d":
                    Note = NoteStep.D;
                    break;
                case "e":
                    Note = NoteStep.E;
                    break;
                case "f":
                    Note = NoteStep.F;
                    break;
                case "g":
                    Note = NoteStep.G;
                    break;
                case "a":
                    Note = NoteStep.A;
                    break;
                case "b":
                    Note = NoteStep.B;
                    break;
            }

            //Caractère 2 et 3 de l'id : l'altération éventuelle
            switch (keySignatureDropDownId.Substring(1, 2).ToLowerInvariant()) {
                case "es":
                    Alteration = Alteration.flat;
                    break;
                case "is":
                    Alteration = Alteration.sharp;
                    break;
                default:
                    Alteration = Alteration.neutral;
                    break;
            }

            //La chaine d'id se termine en indiquant le mode
            if (keySignatureDropDownId.ToLowerInvariant().EndsWith("\\major")) {
                Mode = SongMode.major;
            } else if (keySignatureDropDownId.ToLowerInvariant().EndsWith("\\minor")) {
                Mode = SongMode.minor;
            } else {
                Mode = SongMode.major; //Par défaut (au cas où)
            }

        }

        public KeySignature(NoteStep note, Alteration alteration, SongMode mode) {
            Note = note; 
            Alteration = alteration;
            Mode = mode;
            _lilypondSerializer = new LilypondConverter();
        }

        public string ToDropDownId() {
            return
                _lilypondSerializer.NoteStepToLilyPondString(Note) 
                + _lilypondSerializer.AlterationToLilyPondString(Alteration) 
                + " "
                + _lilypondSerializer.SongModeToLilyPondString(Mode);
        }

        public string ToDropDownLabel() {
            return 
                NoteStepStringifier.ToHumanString(Note) 
                + AlterationStringifier.ToHumanString(Alteration) 
                + " "
                + SongModeStringifier.ToHumanString(Mode);
        }

    }
}
