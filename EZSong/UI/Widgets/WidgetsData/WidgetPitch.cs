using EZSong.Enums;
using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.WidgetsData {

    /// <summary>
    /// Class representing a musical pitch in the context of a widget, using MIDI note numbers to define the pitch.
    /// </summary>
    public class WidgetPitch {

        //https://www.researchgate.net/figure/Each-MIDI-number-corresponds-to-an-octave-listed-in-the-left-column-and-a-note-listed-on_tbl1_305311624
        //Widget use midi notes (0-127) to represent pitch (piano roll)
        public int MidiNoteNumber {
            get; set;
        }

        /// <summary>
        /// Creates a new instance of the WidgetPitch class with default values.
        /// </summary>
        public WidgetPitch() {
        }

        /// <summary>
        /// Creates a new instance of the WidgetPitch class with the specified MIDI note number.
        /// </summary>
        /// <param name="midiNoteNumber">The MIDI note number representing the pitch.</param>
        public WidgetPitch(int midiNoteNumber) {
            MidiNoteNumber = midiNoteNumber;
        }

        /// <summary>
        /// Converts the WidgetPitch instance to a string representation suitable for logging, primarily displaying the MIDI note number.
        /// </summary>
        /// <returns>A string representation of the WidgetPitch instance.</returns>
        internal string ToLogString() {;
            return 
                MidiNoteNumber.ToString();
        }

        /// <summary>
        /// Converts the WidgetPitch instance to a Pitch object, which represents the musical pitch in terms of note step, alteration, and MIDI octave.
        /// </summary>
        /// <returns>A Pitch object representing the musical pitch.</returns>
        /// <exception cref="Exception"></exception>
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
