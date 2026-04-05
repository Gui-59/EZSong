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
        public List<MelodyChordDto> MelodyChords { 
            get; 
            set; 
        }
        public MeasureRhythmPatternDto RhythmPattern { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public MeasureMelodyDto() {
            MelodyChords = new List<MelodyChordDto>();
            RhythmPattern = new MeasureRhythmPatternDto();
        }
    }
}
