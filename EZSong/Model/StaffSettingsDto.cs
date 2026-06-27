using EZSong.MIDI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */
    public class StaffSettingsDto {

        public GMVoice Voice {
            get; set;
        }

        public StaffSettingsDto() {
        }

        public StaffSettingsDto(GMVoice voice) {
            Voice = voice;
        }
    }
}
