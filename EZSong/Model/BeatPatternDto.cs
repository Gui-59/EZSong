using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class BeatPatternDto {
        public List<RhythmElementDto> Elements { get; set; }

        public RhythmRationalDuration ExpectedDuration {get; set;}

        public BeatPatternDto(List<RhythmElementDto> elements, RhythmRationalDuration expectedDuration) { 
            Elements = elements;    
            ExpectedDuration = expectedDuration;
        }
    }
}
