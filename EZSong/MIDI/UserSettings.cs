using EZSong.MIDI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.MIDI {
    internal class UserSettings {

        GMVoice _midiInputEchoVoice = GMVoice.PIPE_PanFlute;
        public GMVoice MidiInputEchoVoice {
            get {
                return _midiInputEchoVoice;
            }
            set {
                _midiInputEchoVoice = value;
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

        int _midiInputEchoDurationMs = 500;
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
