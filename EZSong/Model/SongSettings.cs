using EZSong.MIDI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class SongSettings {

        public StaffsSettings StaffsSettings {
            get;
            set;
        }

        //Constructeur vide (requis, entre autres, pour la (dé)sérialisation JSON)
        public SongSettings() {
            StaffsSettings = new StaffsSettings();
        }

        public SongSettings(StaffsSettings staffsSettings) {
            StaffsSettings = staffsSettings;
        }

        public SongSettingsDto ToDto() {
            return new SongSettingsDto(StaffsSettings.ToDto());
        }
        public static SongSettings FromDto(SongSettingsDto dto) {
            return new SongSettings(StaffsSettings.FromDto(dto.StaffsSettings));
        }

        public GMVoice GetStaffVoice(int staffIndex) {
            return StaffsSettings.GetStaffVoice(staffIndex);
        }
    }
}
