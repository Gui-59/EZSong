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
    public class RhythmSimpleElementDto:IRhythmElementDto {
        public int Numerator {
            get; 
            set;
        }
        public int Denominator {
            get; 
            set;
        }
        public int Dots {
            get; 
            set;
        }

        public bool IsRest {
            get; 
            set;
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public RhythmSimpleElementDto() {
            Numerator = 1;
            Denominator = 4;
            Dots = 0;
            IsRest = true;
        }

    }
}
