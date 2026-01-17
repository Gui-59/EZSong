using EZSong.Serializable;
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
        internal string ToLogString() {
            string logString = "";

            foreach (WidgetPitch p in Pitches) {
                logString += p.ToLogString() + ";";
            }

            return logString; 
        }

        internal MelodyChord ToMelodyChord() {
            MelodyChord melodyChord = new();
            
            foreach (WidgetPitch widgetPitch in _pitches) {
                melodyChord.Pitches.Add(widgetPitch.ToPitch());
            }

            return melodyChord;
        }
    }
}
