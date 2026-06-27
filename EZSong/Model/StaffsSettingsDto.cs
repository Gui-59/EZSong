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
    public class StaffsSettingsDto {

        public List<StaffSettingsDto> Staffs {
            get;
            set;
        }

        public StaffsSettingsDto() {
            Staffs = new();
        }

        public StaffsSettingsDto(List<StaffSettingsDto> staffs) {
            Staffs = staffs;
        }


    }
}
