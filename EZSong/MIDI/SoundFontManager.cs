using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.MIDI {
    public class SoundFontManager {

        private string _currentSoundFont;

        public SoundFontManager() {
            _currentSoundFont = "C:\\SoundFonts\\Arachno SoundFont - Version 1.0.sf2"; //TODO: rendre ça configurable
        }

        /// <summary>
        /// Retrieves the file path of the currently selected sound font.   
        /// </summary>
        /// <returns>The file path of the current sound font.</returns>
        public string GetCurrentSoundFontPath() {
            return _currentSoundFont;
        }
    }
}
