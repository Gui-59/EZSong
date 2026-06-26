using EZSong.Model;
using EZSong.Settings;
using GLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.WidgetsData {
    public class WidgetMelodyChord {
        private List<WidgetPitch> _pitches = new();

        // List of pitches that form the chord at a single position.
        public List<WidgetPitch> Pitches {
            get {
                return _pitches;
            }
            set {
                _pitches = value;
            }
        }

        public List<int> MidiNotes {
            get {
                List<int> notes = new();
                foreach (WidgetPitch p in Pitches) {
                    notes.Add(p.MidiNoteNumber);
                }
                return notes;
            }
        }

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

        internal string ToLogString() {
            string logString = "";

            foreach (WidgetPitch p in Pitches) {
                logString += p.ToLogString() + ";";
            }

            return logString; 
        }

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
