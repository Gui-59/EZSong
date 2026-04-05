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
    public class RhythmElementDto {
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
        public bool TieToNext {
            get; 
            set;
        }

        public RhythmTupletDto Tuplet {
            get; 
            set;
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public RhythmElementDto() {
                Numerator = 1;
                Denominator = 4;
                Dots = 0;
                IsRest = true;
                TieToNext = false;
                Tuplet = new RhythmTupletDto() {
                    Count = 1,
                    InTimeOf = 1
                };
        }

    }
}
