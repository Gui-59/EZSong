using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class MeasureDataDto {

        public int Index {
            get; set;
        }

        public TimeSignatureDto TimeSignature {
            get; set;
        }

        public KeySignatureDto KeySignature {
            get; set;
        }

        public ChordSequenceDto ChordSequence {
            get; set;
        }

        public MeasureMelodyDto Melody {
            get; set;
        }

        public string Lyrics {
            get; set;
        }

        public MeasureRhythmPatternDto Rhythm {
            get; set;
        }

        public MeasureDataDto(TimeSignatureDto timeSignature, KeySignatureDto keySignature, ChordSequenceDto chordSequence, MeasureMelodyDto melody, string lyrics, MeasureRhythmPatternDto rhythm) {
            TimeSignature = timeSignature;
            KeySignature = keySignature;
            ChordSequence = chordSequence;
            Melody = melody;
            Lyrics = lyrics;
            Rhythm = rhythm;
        }
    }
}
