using EZSong.Enums;
using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.WidgetsData {

    public class WidgetPitch {

        //https://www.researchgate.net/figure/Each-MIDI-number-corresponds-to-an-octave-listed-in-the-left-column-and-a-note-listed-on_tbl1_305311624
        //Widget use midi notes (0-127) to represent pitch (piano roll)
        public int MidiNoteNumber {
            get; set;
        }
       
        public WidgetPitch() {
        }

        public WidgetPitch(int midiNoteNumber) {
            MidiNoteNumber = midiNoteNumber;
        }

        internal string ToLogString() {;
            return 
                MidiNoteNumber.ToString();
        }

        internal Pitch ToPitch() {
            int midiOctave = MidiNoteNumber / 12;
            int noteIndex = MidiNoteNumber % 12;

            switch (noteIndex) {
                case 0:
                    return new Pitch(NoteStep.C, Alteration.neutral, midiOctave);
                case 1:
                    return new Pitch(NoteStep.C, Alteration.sharp, midiOctave);
                case 2:
                    return new Pitch(NoteStep.D, Alteration.neutral, midiOctave);
                case 3:
                    return new Pitch(NoteStep.D, Alteration.sharp, midiOctave);
                case 4:
                    return new Pitch(NoteStep.E, Alteration.neutral, midiOctave);
                case 5:
                    return new Pitch(NoteStep.F, Alteration.neutral, midiOctave);
                case 6:
                    return new Pitch(NoteStep.F, Alteration.sharp, midiOctave);
                case 7:
                    return new Pitch(NoteStep.G, Alteration.neutral, midiOctave);
                case 8:
                    return new Pitch(NoteStep.G, Alteration.sharp, midiOctave);
                case 9:
                    return new Pitch(NoteStep.A, Alteration.neutral, midiOctave);
                case 10:
                    return new Pitch(NoteStep.A, Alteration.sharp, midiOctave);
                case 11:
                    return new Pitch(NoteStep.B, Alteration.flat, midiOctave);
                default:
                    throw new Exception();
            }

            
            
        }
    }
}
