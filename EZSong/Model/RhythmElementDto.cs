using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class RhythmElementDto {
        public int Numerator {
            get; set;
        }
        public int Denominator {
            get; set;
        }
        public int Dots {
            get; set;
        }

        public bool IsRest {
            get; set;
        }
        public bool TieToNext {
            get; set;
        }

        public RhythmTupletDto Tuplet {
            get; set;
        }

        public RhythmElementDto(int numerator, int denominator, int dots, bool isRest, bool tieToNext, RhythmTupletDto tuplet) {
            Numerator = numerator;
            Denominator = denominator;  
            Dots = dots;    
            IsRest = isRest;    
            TieToNext = tieToNext;  
            Tuplet = tuplet;

        }

    }
}
