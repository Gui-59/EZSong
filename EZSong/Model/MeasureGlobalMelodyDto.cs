using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class MeasureGlobalMelodyDto {

        public int StaffIndex {
            get;
            set;
        }

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
            StaffIndex = 0;
            Melody = new MeasureMelodyDto();
            Pattern = new MeasureRhythmPatternDto();
        }

        public MeasureGlobalMelodyDto(int staffIndex, MeasureMelodyDto melody, MeasureRhythmPatternDto pattern) {
            StaffIndex = staffIndex;
            Melody = melody;
            Pattern = pattern;
        }
    }
}
