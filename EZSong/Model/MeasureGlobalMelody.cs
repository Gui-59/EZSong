using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class MeasureGlobalMelody {

        public int StaffIndex { 
            get; 
            set; 
        }

        public MeasureMelody Melody { 
            get; 
            set;
        }
        public MeasureRhythmPattern Pattern { 
            get; 
            set;
        }

        public MeasureGlobalMelody() {
            StaffIndex = 0;
            Melody = new MeasureMelody();
            Pattern = new MeasureRhythmPattern();
        }

        public MeasureGlobalMelody(int staffIndex) {
            StaffIndex = staffIndex;
            Melody = new MeasureMelody(staffIndex);
            Pattern = new MeasureRhythmPattern(staffIndex);
        }

        public MeasureGlobalMelody(int staffIndex, MeasureMelody melody, MeasureRhythmPattern pattern) {
            StaffIndex = staffIndex;
            Melody = melody;
            Pattern = pattern;
        }

        internal MeasureGlobalMelodyDto ToDto() {
            return new MeasureGlobalMelodyDto(StaffIndex, Melody.ToDto(), Pattern.ToDto());
        }

        public static MeasureGlobalMelody FromDto(MeasureGlobalMelodyDto melody) {
            return new MeasureGlobalMelody(melody.StaffIndex, MeasureMelody.FromDto(melody.Melody), MeasureRhythmPattern.FromDto(melody.Pattern));
        }
    }
}
