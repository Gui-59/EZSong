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

    public class SegmentDataDto {

        public int Index {
            get;
            set;
        }

        public List<MeasureDataDto> MeasureData { //Mesures
            get;
            set;
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public SegmentDataDto() {
            Index = 0;
            MeasureData = new List<MeasureDataDto>();
        }

        public SegmentDataDto(
            int index,
            List<MeasureDataDto> measureData
        ) {
            Index = index;
            MeasureData = measureData;
        }
    }
}
