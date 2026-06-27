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

        public int StaffIndex {
            get;
            set;
        }

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
            StaffIndex = 0; //Par défaut
            Beats = new List<BeatPatternDto>();
            TimeSignature = new TimeSignature();
        }

        public MeasureRhythmPatternDto(int staffIndex, List<BeatPatternDto> beats, TimeSignature ts) {
            StaffIndex = staffIndex; 
            Beats = beats;
            TimeSignature = ts;
        }
    }
}
