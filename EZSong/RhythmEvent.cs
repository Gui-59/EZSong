using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong {
    public abstract class RhythmEvent {
        public RhythmDurationKind DurationKind {
            get; protected set;
        }

        protected RhythmEvent(RhythmDurationKind durationKind) {
            DurationKind = durationKind;
        }

        public RationalDuration Duration {
            get {
                return DurationKind switch {
                    RhythmDurationKind.Whole => new RationalDuration(1, 1),
                    RhythmDurationKind.Half => new RationalDuration(1, 2),
                    RhythmDurationKind.Quarter => new RationalDuration(1, 4),
                    RhythmDurationKind.Eighth => new RationalDuration(1, 8),
                    RhythmDurationKind.Sixteenth => new RationalDuration(1, 16),
                    _ => throw new NotSupportedException()
                };
            }
        }
    }
}

