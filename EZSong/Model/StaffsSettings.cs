using EZSong.MIDI.Enums;
using EZSong.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class StaffsSettings {

        public List<StaffSettings> Staffs {
            get;
            set;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public StaffsSettings() {
            Staffs = new();
            //Toujours au moins 1 portée
            UserSettings userSettings = new();
            //TODO : Mélodie ou basse ?
            Staffs.Add(new StaffSettings(Constants.DefaultStaffName, userSettings.MidiInputDefaultVoice, Constants.MelodyBaseOctave));
        }

        public StaffsSettings(List<StaffSettings> staffs) {
            Staffs = staffs;
        }

        public StaffsSettingsDto ToDto() {
            List<StaffSettingsDto> staffs = new();
            foreach (StaffSettings staffSettings in Staffs) {
                staffs.Add(staffSettings.ToDto());
            }
            return new StaffsSettingsDto(staffs);
        }

        public static StaffsSettings FromDto(StaffsSettingsDto dto) {
            List<StaffSettings> staffs = new();
            foreach (StaffSettingsDto staffSettings in dto.Staffs ) {
                staffs.Add(StaffSettings.FromDto(staffSettings));
            }
            return new StaffsSettings(staffs);
        }

        public GMVoice GetStaffVoice(int staffIndex) {
            return Staffs[staffIndex].Voice;
        }

        internal int GetStaffBaseOctave(int staffIndex) {
            return Staffs[staffIndex].BaseOctave;
        }

        internal void AddStaff(string staffName, GMVoice voice, int baseOctave) {
            
            Staffs.Add(new StaffSettings(staffName, voice, baseOctave));
        }
    }
}
