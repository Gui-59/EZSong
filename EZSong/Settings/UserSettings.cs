using EZSong.MIDI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Settings {
    public class UserSettings {

        string _musicalFontFamily = "Bravura";
        public string MusicalFontFamily {
            get {
                return _musicalFontFamily;
            }
            set {
                _musicalFontFamily = value;
            }
        }

        GMVoice _midiInputEchoDefaultVoice = GMVoice.PIANO_BrightAcoustic;
        public GMVoice MidiInputDefaultVoice {
            get {
                return _midiInputEchoDefaultVoice;
            }
            set {
                _midiInputEchoDefaultVoice = value;
            }
        }

        int _midiInputEchoVeloctiy = 100;
        public int MidiInputEchoVeloctiy {
            get {
                return _midiInputEchoVeloctiy;
            }
            set {
                _midiInputEchoVeloctiy = value;
            }
        }

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
