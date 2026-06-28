using Melanchall.DryWetMidi.Interaction;
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

        public List<MeasureGlobalMelodyDto> Staffs { //Groupe de portées
            get; 
            set;
        }

        public string Lyrics {
            get; 
            set;
        }

        

        //Constructeur vide (requis pour la sérialisation JSON)
        public MeasureDataDto() {
            Index = 0;
            TimeSignature = new TimeSignatureDto();
            KeySignature = new KeySignatureDto();
            ChordSequence = new ChordSequenceDto();
            Staffs = new List<MeasureGlobalMelodyDto>();
            Lyrics = string.Empty;
        }

        public MeasureDataDto(int index,
                    TimeSignatureDto timeSignature,
                    KeySignatureDto keySignature,
                    ChordSequenceDto chordSequence,
                    List<MeasureGlobalMelodyDto>  staffs,
                    String lyrics) {

            Index = index;
            TimeSignature = timeSignature;
            KeySignature = keySignature;
            ChordSequence = chordSequence;
            Staffs = staffs;
            Lyrics = lyrics;
        }
    }
}
