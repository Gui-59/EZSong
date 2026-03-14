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

        public bool IsRest {
            get; protected set;
        }

        public int Dots {
            get; protected set;
        }

        protected RhythmEvent(RhythmDurationKind durationKind) {
            DurationKind = durationKind;
        }

        public RhythmRationalDuration Duration {
            get {
                //TODO: indiquer les vraies durées (en fonction de la signature rythmique)
                return DurationKind switch {
                    RhythmDurationKind.Whole => new RhythmRationalDuration(1, 1, Dots),
                    RhythmDurationKind.Half => new RhythmRationalDuration(1, 2, Dots),
                    RhythmDurationKind.Quarter => new RhythmRationalDuration(1, 4, Dots),
                    RhythmDurationKind.Eighth => new RhythmRationalDuration(1, 8, Dots),
                    RhythmDurationKind.Sixteenth => new RhythmRationalDuration(1, 16, Dots),
                    _ => throw new NotSupportedException()
                };
            }
        }


    }
}

