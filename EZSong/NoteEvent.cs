using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong {
    public sealed class NoteEvent : RhythmEvent {
        public bool TiedFromPrevious {
            get; set;
        }
        public bool TiedToNext {
            get; set;
        }

        public List<Ornament> Ornaments { get; } = new();

        public NoteEvent(RhythmDurationKind durationKind)
            : base(durationKind) {
        }
    }
}

