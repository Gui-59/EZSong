using EZSong.MIDI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Settings {

    /// <summary>
    /// Stores user-specific settings for musical input configuration.
    /// </summary>
    public class UserSettings {

        /// <summary>
        /// Musical font family used for rendering musical notation in the application.
        /// </summary>
        string _musicalFontFamily = "Bravura";
        public string MusicalFontFamily {
            get {
                return _musicalFontFamily;
            }
            set {
                _musicalFontFamily = value;
            }
        }

        /// <summary>
        /// MIDI voice used for echoing MIDI input, allowing users to hear the notes they play in real-time.
        /// </summary>
        GMVoice _midiInputEchoDefaultVoice = GMVoice.PIANO_BrightAcoustic;
        public GMVoice MidiInputDefaultVoice {
            get {
                return _midiInputEchoDefaultVoice;
            }
            set {
                _midiInputEchoDefaultVoice = value;
            }
        }

        /// <summary>
        /// MIDI velocity used for echoing MIDI input, determining the volume of the echoed notes.
        /// </summary>
        int _midiInputEchoVeloctiy = 100;
        public int MidiInputEchoVeloctiy {
            get {
                return _midiInputEchoVeloctiy;
            }
            set {
                _midiInputEchoVeloctiy = value;
            }
        }

        /// <summary>
        /// MIDI echo duration in milliseconds, specifying how long the echoed notes should be sustained after being played.
        /// </summary>
        int _midiInputEchoDurationMs = 400;
        public int MidiInputEchoDurationMs {
            get {
                return _midiInputEchoDurationMs;
            }
            set {
                _midiInputEchoDurationMs = value;
            }
        }
    }
}
