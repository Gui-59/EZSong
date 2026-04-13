using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class MeasureGlobalMelodyDto {
        public MeasureMelodyDto Melody {
            get;
            set;
        }
        public MeasureRhythmPatternDto Pattern {
            get;
            set;
        }
        //Constructeur vide (requis pour la sérialisation JSON)
        public MeasureGlobalMelodyDto() {
            Melody = new MeasureMelodyDto();
            Pattern = new MeasureRhythmPatternDto();
        }
    }
}
