using EZSong.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong {
    public sealed class Cadence {
        public TimeSignature TimeSignature {
            get;
        }
        public QuantizationMode Quantization {
            get;
        }

        private readonly List<RhythmEvent> _events = new();
        public IReadOnlyList<RhythmEvent> Events {
            get {
                return _events;
            }
        }

        public Cadence(TimeSignature timeSignature, QuantizationMode quantization) {
            TimeSignature = timeSignature;
            Quantization = quantization;
        }

        public void AddEvent(RhythmEvent ev) {
            _events.Add(ev);
        }

        public RationalDuration GetTotalDuration() {
            return _events
                .Select(e => e.Duration)
                .Aggregate(new RationalDuration(0, 1), (a, b) => a + b)
                .Normalize();
        }

        public bool IsComplete {
            get {
                return GetTotalDuration().Equals(TimeSignature.TotalDuration);
            }
        }
    }
}
