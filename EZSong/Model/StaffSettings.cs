using EZSong.MIDI.Enums;
using EZSong.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    /// <summary>
    /// A class that represents the settings for a musical staff, including its name, voice, and base octave.
    /// </summary>
    public class StaffSettings {

        /// <summary>
        /// Name of the staff, used for identification and display purposes.
        /// </summary>
        public String Name {
            get;
            set;
        }

        /// <summary>
        /// GMVoice representing the voice assigned to the staff, which determines the instrument sound for MIDI playback.
        /// </summary>
        public GMVoice Voice { 
            get; 
            set; 
        }

        /// <summary>
        /// Base octave for the staff, which affects the pitch range of the notes played on this staff.
        /// </summary>
        public int BaseOctave {
            get;
            set;
        }

        /// <summary>
        /// Constructs a new instance of the StaffSettings class with default values for name, voice, and base octave.
        /// Note : needed for JSON serialization/deserialization.
        /// </summary>
        public StaffSettings() {
            Name = Settings.Constants.DefaultStaffName;
            UserSettings userSettings = new(); //TODO : centraliser
            Voice = userSettings.MidiInputDefaultVoice; //TODO : permettre le choix via l'IHM
            BaseOctave = Constants.MelodyBaseOctave; //TODO : permettre le choix via l'IHM
        }

        /// <summary>
        /// Constructs a new instance of the StaffSettings class with the specified name, voice, and base octave.
        /// </summary>
        /// <param name="name">The name of the staff.</param>
        /// <param name="voice">The GMVoice to associate with the staff.</param>
        /// <param name="baseOctave">The base octave for the staff.</param>
        public StaffSettings(String name, GMVoice voice, int baseOctave) {
            Name = name;
            Voice = voice;
            BaseOctave = baseOctave;
        }

        /// <summary>
        /// Converts the current StaffSettings instance to a StaffSettingsDto for data transfer purposes.
        /// </summary>
        /// <returns>A StaffSettingsDto representing the current StaffSettings instance.</returns>
        public StaffSettingsDto ToDto() {
            return new StaffSettingsDto(Name, Voice, BaseOctave);
        }
        
        /// <summary>
        /// Creates a StaffSettings instance from a given StaffSettingsDto.
        /// </summary>
        /// <param name="dto">The StaffSettingsDto to convert.</param>
        /// <returns>A StaffSettings instance representing the data in the DTO.</returns>
        public static StaffSettings FromDto(StaffSettingsDto dto) {
            return new StaffSettings(dto.Name, dto.Voice, dto.BaseOctave);
        }
    }
}
