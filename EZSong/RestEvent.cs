using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong {
    public sealed class RestEvent : RhythmEvent {
        public RestEvent(RhythmDurationKind durationKind)
            : base(durationKind) {
        }
    }
}

