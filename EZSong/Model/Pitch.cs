using EZSong.Enums;
using EZSong.EnumsStringifier;
using EZSong.Exporting.Lilypond;
using EZSong.UI.Widgets.WidgetsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    public class Pitch {

        public NoteStep Note {
            get; set;
        }
        public Alteration Alteration {
            get; set;
        }
        public int OctaveOffset {
            get; set;
        }

        private readonly ILilypondConverter _lilypondConverter;


        public Pitch(NoteStep note, Alteration alteration, int octaveOffset = 0) {
            _lilypondConverter = new LilypondConverter();

            Note = note;
            Alteration = alteration;
            OctaveOffset = octaveOffset;
        }

        public string ToLilyPondString() {
            string lilyPondString = string.Empty;

            lilyPondString += _lilypondConverter.NoteStepToLilyPondString(Note);
            lilyPondString += _lilypondConverter.AlterationToLilyPondString(Alteration);

            int sheetOctaveOffset = OctaveOffset + 1; //On augmente d'une octave se mettre naturellement dans la portée

            if (sheetOctaveOffset > 0) {

                for (int i = 0; i < sheetOctaveOffset; i++) {
                    lilyPondString += "'";
                }
                
            } else if (sheetOctaveOffset < 0) {

                for (int i = 0; i < -1 * sheetOctaveOffset; i++) {
                    lilyPondString += ",";
                }

            }

            return lilyPondString;
        }

        internal WidgetPitch ToWidgetPitch() {

            WidgetPitch widgetPitch = new();

            switch (Note) {
                case NoteStep.C:
                    widgetPitch.NoteIndex = 0;
                    break;
                case NoteStep.D:
                    widgetPitch.NoteIndex = 1;
                    break;
                case NoteStep.E:
                    widgetPitch.NoteIndex = 2;
                    break;
                case NoteStep.F:
                    widgetPitch.NoteIndex = 3;
                    break;
                case NoteStep.G:
                    widgetPitch.NoteIndex = 4;
                    break;
                case NoteStep.A:
                    widgetPitch.NoteIndex = 5;
                    break;
                case NoteStep.B:
                    widgetPitch.NoteIndex = 6;
                    break;
            }

            widgetPitch.Alteration = Alteration;
            widgetPitch.OctaveOffset = OctaveOffset;

            return widgetPitch;
        }

        internal PitchDto ToDto() {
            return new PitchDto {
                Note = Note,
                Alteration = Alteration,
                OctaveOffset = OctaveOffset
             };

        }
    }
}
