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

    public class MeasureMelodyDto {

        public int StaffIndex {
            get;
            set;
        }

        public List<MelodyChordDto> MelodyChords { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public MeasureMelodyDto() {
            StaffIndex = 0; //Par défaut
            MelodyChords = new List<MelodyChordDto>();
        }

        public MeasureMelodyDto(int staffIndex, List<MelodyChordDto> melodyChords) {
            StaffIndex = staffIndex;
            MelodyChords = melodyChords;
        }
    }
}
