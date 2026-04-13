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
            get;
            set;
        }

        public TimeSignatureDto TimeSignature {
            get; 
            set;
        }

        public KeySignatureDto KeySignature {
            get; 
            set;
        }

        public ChordSequenceDto ChordSequence {
            get; 
            set;
        }

        public MeasureGlobalMelodyDto GlobalMelody { //TODO : Gérer des groupes de mélodies
            get; 
            set;
        }

        public string Lyrics {
            get; 
            set;
        }

        

        //Constructeur vide (requis pour la sérialisation JSON)
        public MeasureDataDto() {
            TimeSignature = new TimeSignatureDto();
            KeySignature = new KeySignatureDto();
            ChordSequence = new ChordSequenceDto();
            GlobalMelody = new MeasureGlobalMelodyDto();
            Lyrics = string.Empty;
        }
    }
}
