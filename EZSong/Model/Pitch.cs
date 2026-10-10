using EZSong.Enums;
using EZSong.EnumsStringifier;
using EZSong.Exporting.Lilypond;
using EZSong.Settings;
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
            MidiOctave = Constants.MelodyBaseOctave; //Par défaut
            _lilypondConverter = new LilypondConverter();
        }

        public Pitch(NoteStep note, Alteration alteration, int midiOctave) {

            if (midiOctave < 0 || midiOctave > 10) {
                throw new ArgumentOutOfRangeException("MidiOctave must be between 0 and 10.");
            }

            _lilypondConverter = new LilypondConverter();

            Note = note;
            Alteration = alteration;
            MidiOctave = midiOctave;
        }

        public string ToLilyPondString(int baseOctave) {

            string lilyPondString = string.Empty;

            lilyPondString += _lilypondConverter.NoteStepToLilyPondString(Note);
            lilyPondString += _lilypondConverter.AlterationToLilyPondString(Alteration);

            int sheetOctaveOffset = MidiOctave - baseOctave;

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
            return new WidgetPitch(ToMidiNoteNumber());
        }

        public int ToMidiNoteNumber() {
            return (MidiOctave * 12) + ((int)Note) + ((int)Alteration);
        }

        internal PitchDto ToDto() {
            return new PitchDto(Note, Alteration, MidiOctave);
        }

        public static Pitch FromMidi(int midi) {
            int pc = midi % 12;
            NoteStep note;
            Alteration alteration;
            (note, alteration) = pc switch {
                0 => (NoteStep.C, Alteration.neutral),
                1 => (NoteStep.C, Alteration.sharp),
                2 => (NoteStep.D, Alteration.neutral),
                3 => (NoteStep.D, Alteration.sharp),
                4 => (NoteStep.E, Alteration.neutral),
                5 => (NoteStep.F, Alteration.neutral),
                6 => (NoteStep.F, Alteration.sharp),
                7 => (NoteStep.G, Alteration.neutral),
                8 => (NoteStep.G, Alteration.sharp),
                9 => (NoteStep.A, Alteration.neutral),
                10 => (NoteStep.A, Alteration.sharp),
                11 => (NoteStep.B, Alteration.neutral),
                _ => throw new System.Diagnostics.UnreachableException()
            };
            return new Pitch(note, alteration, midi / 12);
        }
    }
}
