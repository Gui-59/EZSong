using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */
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

        //Constructeur vide (requis pour la sérialisation JSON)
        public MeasureDataDto() {
            TimeSignature = new TimeSignatureDto();
            KeySignature = new KeySignatureDto();
            ChordSequence = new ChordSequenceDto();
            Melody = new MeasureMelodyDto();
            Lyrics = string.Empty;
            Rhythm = new MeasureRhythmPatternDto();
        }
    }
}
