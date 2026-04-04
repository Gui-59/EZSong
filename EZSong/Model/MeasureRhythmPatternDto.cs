using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class MeasureRhythmPatternDto {
        public List<BeatPatternDto> Beats { get; set; }
        public TimeSignature TimeSignature { get; set; }

        public MeasureRhythmPatternDto(List<BeatPatternDto> beats, TimeSignature timeSignature) {
            Beats = beats;
            TimeSignature = timeSignature;
        }
    }
}
