using EZSong.Model;
using EZSong.Settings;
using GLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.WidgetsData {

    /// <summary>
    /// A class that represents a chord in a melody, 
    /// consisting of multiple pitches played simultaneously.
    /// </summary>
    public class WidgetMelodyChord {
        private List<WidgetPitch> _pitches = new();

        /// <summary>
        /// List of pitches that make up the melody chord, 
        /// where each pitch is represented by a WidgetPitch object.
        /// </summary>
        public List<WidgetPitch> Pitches {
            get {
                return _pitches;
            }
            set {
                _pitches = value;
            }
        }

        /// <summary>
        /// List of MIDI note numbers corresponding to the pitches in the melody chord,
        /// </summary>
        public List<int> MidiNotes {
            get {
                List<int> notes = new();
                foreach (WidgetPitch p in Pitches) {
                    notes.Add(p.MidiNoteNumber);
                }
                return notes;
            }
        }

        /// <summary>
        /// Enumerable of velocities for each pitch in the melody chord,
        /// </summary>
        public IEnumerable<int> Velocities {
            get {
                UserSettings userSettings = new(); //TODO : Centraliser
                List<int> velocities = new();
                for (int i = 0; i < _pitches.Count; i++) {
                    velocities.Add(userSettings.MidiInputEchoVeloctiy);
                }
                return velocities;
            }
        }

        /// <summary>
        /// Converts the WidgetMelodyChord to a string representation suitable for logging purposes.
        /// </summary>
        /// <returns>A string representation of the WidgetMelodyChord instance.</returns>
        internal string ToLogString() {
            string logString = "";

            foreach (WidgetPitch p in Pitches) {
                logString += p.ToLogString() + ";";
            }

            return logString; 
        }


        /// <summary>
        /// Converts the WidgetMelodyChord to a MelodyChord object, which is a model representation of the chord.
        /// </summary>
        /// <returns>A MelodyChord object representing the chord.</returns>
        public MelodyChord ToMelodyChord() {
            
            List<Pitch> pitches = new();
            foreach (WidgetPitch widgetPitch in _pitches) {
                pitches.Add(widgetPitch.ToPitch());
            }

            MelodyChord melodyChord = new(pitches);
            return melodyChord;
        }
    }
}
