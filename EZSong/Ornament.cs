using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong {
    public sealed class Ornament {
        public OrnamentType Type {
            get;
        }
        public RhythmRationalDuration? SuggestedDuration {
            get;
        }

        public Ornament(OrnamentType type, RhythmRationalDuration? suggestedDuration = null) {
            Type = type;
            SuggestedDuration = suggestedDuration;
        }
    }
}
