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
        public int BaseOctave {
            get;
            set;
        }

        //Constructeur vide (requis, entre autres, pour la (dé)sérialisation JSON)
        public StaffSettings() {
            UserSettings userSettings = new(); //TODO : centraliser
            Voice = userSettings.MidiInputDefaultVoice; //TODO : permettre le choix via l'IHM
            BaseOctave = Constants.MelodyBaseOctave; //TODO : permettre le choix via l'IHM
        }

        public StaffSettings(GMVoice voice, int baseOctave) {
            Voice = voice;
            BaseOctave = baseOctave;
        }

        public StaffSettingsDto ToDto() {
            return new StaffSettingsDto(Voice, BaseOctave);
        }

        public static StaffSettings FromDto(StaffSettingsDto dto) {
            return new StaffSettings(dto.Voice, dto.BaseOctave);
        }
    }
}
