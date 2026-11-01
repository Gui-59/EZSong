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

    /// <summary>
    /// A data transfer object (DTO) that encapsulates the settings for a musical staff, including its name, voice, and base octave.
    /// </summary>
    public class StaffSettingsDto {

        /// <summary>
        /// Name of the staff, used for identification and display purposes.
        /// </summary>
        public String Name { 
            get; 
            set; 
        }

        /// <summary>
        /// General MIDI voice assigned to the staff, determining the instrument sound used for playback.
        /// </summary>
        public GMVoice Voice {
            get; set;
        }

        /// <summary>
        /// Base octave for the staff, indicating the starting octave for pitch calculations and note representation.
        /// </summary>
        public int BaseOctave {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of the StaffSettingsDto class with default values for name, voice, and base octave.
        /// </summary>
        public StaffSettingsDto() {
            Name = Settings.Constants.DefaultStaffName;
            Voice = GMVoice.PIANO_AcousticGrand;
            BaseOctave = -1;
        }

        /// <summary>
        /// Creates a new instance of the StaffSettingsDto class with specified values for name, voice, and base octave.
        /// </summary>
        /// <param name="name">The name of the staff.</param>
        /// <param name="voice">The GMVoice to associate with the staff.</param>
        /// <param name="baseOctave">The base octave for the staff.</param>
        public StaffSettingsDto(String name, GMVoice voice, int baseOctave) {
            Name = name;
            Voice = voice;
            BaseOctave = baseOctave;
        }
    }
}
