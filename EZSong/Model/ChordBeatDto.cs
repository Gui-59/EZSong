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

    public class ChordBeatDto {

        public List<ChordDto> Chords { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public ChordBeatDto() { 
            Chords = new List<ChordDto>();
        }
    }
}
