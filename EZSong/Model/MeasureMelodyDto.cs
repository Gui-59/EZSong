using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class MeasureMelodyDto {
        public List<MelodyChordDto> MelodyChords { get; set; }
        public MeasureRhythmPattern RhythmPattern { get; set; }

        public MeasureMelodyDto(List<MelodyChordDto> melodyChords, MeasureRhythmPattern rhythmPattern) {
            MelodyChords = melodyChords;
            RhythmPattern = rhythmPattern;
        }
    }
}
