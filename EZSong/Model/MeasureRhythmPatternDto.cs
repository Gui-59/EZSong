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
    public class MeasureRhythmPatternDto {
        public List<BeatPatternDto> Beats { 
            get; 
            set; 
        }
        public TimeSignature TimeSignature { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public MeasureRhythmPatternDto() {
            Beats = new List<BeatPatternDto>();
            TimeSignature = new TimeSignature(4, 4);
        }
    }
}
