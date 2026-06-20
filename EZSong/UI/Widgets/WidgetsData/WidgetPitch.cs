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
            NoteStep noteStep = (NoteStep)(noteIndex % 7);
            Alteration alteration = (Alteration)(noteIndex / 7); //TODO : A tester
            return new Pitch(noteStep, alteration, midiOctave);
        }
    }
}
