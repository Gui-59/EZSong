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
            get; 
            set;
        }
        public Alteration Alteration {
            get;
            set;
        }
        public int MidiOctave {
            get;
            set;
        }

        private readonly ILilypondConverter _lilypondConverter;

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public Pitch() {
            Note = NoteStep.C;
            Alteration = Alteration.neutral;
            MidiOctave = 5; //Octave par défaut pour le pitch C4 (C5 en MIDI)
            _lilypondConverter = new LilypondConverter();
        }

        public Pitch(NoteStep note, Alteration alteration, int midiOctave = 0) {

            if (midiOctave < 0 || midiOctave > 10) {
                throw new ArgumentOutOfRangeException("MidiOctave must be between 0 and 10.");
            }

            _lilypondConverter = new LilypondConverter();

            Note = note;
            Alteration = alteration;
            MidiOctave = midiOctave;
        }

        public string ToLilyPondString() {
            string lilyPondString = string.Empty;

            lilyPondString += _lilypondConverter.NoteStepToLilyPondString(Note);
            lilyPondString += _lilypondConverter.AlterationToLilyPondString(Alteration);

            int sheetOctaveOffset = 5 - MidiOctave; // 5 is the base octave for LilyPond (C4 in MIDI is C5 in LilyPond)

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
            int midiNoteNumber = (MidiOctave * 12) + ((int)Note) + ((int)Alteration * 7); //TODO : tester
            return new WidgetPitch(midiNoteNumber);
        }

        internal PitchDto ToDto() {
            return new PitchDto {
                Note = Note,
                Alteration = Alteration,
                MidiOctave = MidiOctave
             };
        }
    }
}
