using EZSong.MIDI.Enums;
using NFluidsynth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */

    public class StaffSettingsDto {

        public String Name { 
            get; 
            set; 
        }

        public GMVoice Voice {
            get; set;
        }

        public int BaseOctave {
            get;
            set;
        }

        public StaffSettingsDto() {
            Name = Settings.Constants.DefaultStaffName;
            Voice = GMVoice.PIANO_AcousticGrand;
            BaseOctave = -1;
        }

        public StaffSettingsDto(String name, GMVoice voice, int baseOctave) {
            Name = name;
            Voice = voice;
            BaseOctave = baseOctave;
        }
    }
}
