using EZSong.MIDI.Enums;
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
            Staffs.Add(new StaffSettings());
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
    }
}
