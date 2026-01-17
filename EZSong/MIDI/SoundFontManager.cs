using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.MIDI {
    public class SoundFontManager {

        private string _currentSoundFont;

        public SoundFontManager() {
            _currentSoundFont = "C:\\SoundFonts\\Arachno SoundFont - Version 1.0.sf2";
        }

        public string GetCurrentSoundFontPath() {
            return _currentSoundFont;
        }
    }
}
