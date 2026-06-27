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
    public class SongSettingsDto {

        public StaffsSettingsDto StaffsSettings {
            get;
            set;
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public SongSettingsDto() {
            StaffsSettings = new();
        }

        public SongSettingsDto(StaffsSettingsDto staffsSettingsDto) {
            StaffsSettings = staffsSettingsDto;
        }
    }
}
