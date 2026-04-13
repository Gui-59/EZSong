using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class MeasureGlobalMelody {
        public MeasureMelody Melody { 
            get; 
            set;
        }
        public MeasureRhythmPattern Pattern { 
            get; 
            set;
        }

        public MeasureGlobalMelody() {
            Melody = new MeasureMelody();
            Pattern = new MeasureRhythmPattern();
        }

        internal MeasureGlobalMelodyDto ToDto() {
            return new MeasureGlobalMelodyDto() {
                Melody = Melody.ToDto(),
                Pattern = Pattern.ToDto()
            };
        }

        public static MeasureGlobalMelody FromDto(MeasureGlobalMelodyDto melody) {
            return new MeasureGlobalMelody() {
                Melody = MeasureMelody.FromDto(melody.Melody),
                Pattern = MeasureRhythmPattern.FromDto(melody.Pattern)
            };
        }
    }
}
