using EZSong.MIDI.Enums;
using EZSong.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class StaffSettings {

        public GMVoice Voice { 
            get; 
            set; 
        }

        //Constructeur vide (requis, entre autres, pour la (dé)sérialisation JSON)
        public StaffSettings() {
            UserSettings userSettings = new(); //TODO : centraliser
            Voice = userSettings.MidiInputDefaultVoice; //TODO : permettre le choix via l'IHM
        }

        public StaffSettings(GMVoice voice) {
            Voice = voice;
        }

        public StaffSettingsDto ToDto() {
            return new StaffSettingsDto(Voice);
        }

        public static StaffSettings FromDto(StaffSettingsDto dto) {
            return new StaffSettings(dto.Voice);
        }
    }
}
