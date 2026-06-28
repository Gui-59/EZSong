using EZSong.MIDI.Enums;
using EZSong.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class StaffSettings {

        public String Name {
            get;
            set;
        }

        public GMVoice Voice { 
            get; 
            set; 
        }
        public int BaseOctave {
            get;
            set;
        }

        //Constructeur vide (requis, entre autres, pour la (dé)sérialisation JSON)
        public StaffSettings() {
            Name = Settings.Constants.DefaultStaffName;
            UserSettings userSettings = new(); //TODO : centraliser
            Voice = userSettings.MidiInputDefaultVoice; //TODO : permettre le choix via l'IHM
            BaseOctave = Constants.MelodyBaseOctave; //TODO : permettre le choix via l'IHM
        }

        public StaffSettings(String name, GMVoice voice, int baseOctave) {
            Name = name;
            Voice = voice;
            BaseOctave = baseOctave;
        }

        public StaffSettingsDto ToDto() {
            return new StaffSettingsDto(Name, Voice, BaseOctave);
        }

        public static StaffSettings FromDto(StaffSettingsDto dto) {
            return new StaffSettings(dto.Name, dto.Voice, dto.BaseOctave);
        }
    }
}
