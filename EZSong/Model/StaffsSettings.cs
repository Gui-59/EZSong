using EZSong.MIDI.Enums;
using EZSong.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    /// <summary>
    /// A class that represents the settings for multiple musical staffs,
    /// </summary>
    public class StaffsSettings {

        /// <summary>
        /// List of StaffSettings objects, each representing the settings for an individual musical staff,
        /// </summary>
        public List<StaffSettings> Staffs {
            get;
            set;
        }

        /// <summary>
        /// Constructs a new instance of the StaffsSettings class, initializing it with a default staff.
        /// Note : Needed for serialization and deserialization.
        /// </summary>
        public StaffsSettings() {
            Staffs = new();
            //Toujours au moins 1 portée
            UserSettings userSettings = new();
            //TODO : Mélodie ou basse ?
            Staffs.Add(new StaffSettings(Constants.DefaultStaffName, userSettings.MidiInputDefaultVoice, Constants.MelodyBaseOctave));
        }

        /// <summary>
        /// Constructs a new instance of the StaffsSettings class with the specified list of staff settings.
        /// </summary>
        /// <param name="staffs">The list of StaffSettings objects to initialize the StaffsSettings with.</param>
        public StaffsSettings(List<StaffSettings> staffs) {
            Staffs = staffs;
        }

        /// <summary>
        /// Converts the current StaffsSettings instance to a StaffsSettingsDto object for data transfer or serialization purposes.
        /// </summary>
        /// <returns>A StaffsSettingsDto object representing the current StaffsSettings instance.   </returns>
        public StaffsSettingsDto ToDto() {
            List<StaffSettingsDto> staffs = new();
            foreach (StaffSettings staffSettings in Staffs) {
                staffs.Add(staffSettings.ToDto());
            }
            return new StaffsSettingsDto(staffs);
        }

        /// <summary>
        /// Creates a new instance of the StaffsSettings class from a given StaffsSettingsDto object.
        /// </summary>
        /// <param name="dto">The StaffsSettingsDto object to convert.</param>
        /// <returns>A StaffsSettings object representing the given StaffsSettingsDto</returns>
        public static StaffsSettings FromDto(StaffsSettingsDto dto) {
            List<StaffSettings> staffs = new();
            foreach (StaffSettingsDto staffSettings in dto.Staffs ) {
                staffs.Add(StaffSettings.FromDto(staffSettings));
            }
            return new StaffsSettings(staffs);
        }

        /// <summary>
        /// Gets the GMVoice associated with the staff at the specified index.
        /// </summary>
        /// <param name="staffIndex"></param>
        /// <returns></returns>
        public GMVoice GetStaffVoice(int staffIndex) {
            return Staffs[staffIndex].Voice;
        }

        /// <summary>
        /// Gets the base octave associated with the staff at the specified index.
        /// </summary>
        /// <param name="staffIndex"></param>
        /// <returns></returns>
        internal int GetStaffBaseOctave(int staffIndex) {
            return Staffs[staffIndex].BaseOctave;
        }

        /// <summary>
        /// Adds a new staff with the specified name, voice, and base octave to the Staffs list.
        /// </summary>
        /// <param name="staffName">The name of the staff to add.</param>
        /// <param name="voice">The GMVoice to associate with the new staff.</param>
        /// <param name="baseOctave">The base octave for the new staff.</param>
        internal void AddStaff(string staffName, GMVoice voice, int baseOctave) {
            Staffs.Add(new StaffSettings(staffName, voice, baseOctave));
        }
    }
}
