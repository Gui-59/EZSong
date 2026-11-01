using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */

    /// <summary>
    /// A Data Transfer Object (DTO) class that represents a collection of staff settings
    /// </summary>
    public class StaffsSettingsDto {

        /// <summary>
        /// List of staff settings, where each staff is represented by a StaffSettingsDto object.
        /// </summary>
        public List<StaffSettingsDto> Staffs {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of the StaffsSettingsDto class, initializing the Staffs list to an empty list.
        /// </summary>
        public StaffsSettingsDto() {
            Staffs = new();
        }


        /// <summary>
        /// Creates a new instance of the StaffsSettingsDto class with the provided list of staff settings.
        /// </summary>
        /// <param name="staffs"></param>
        public StaffsSettingsDto(List<StaffSettingsDto> staffs) {
            Staffs = staffs;
        }
    }
}
