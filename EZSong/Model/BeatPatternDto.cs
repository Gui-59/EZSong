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

    public class BeatPatternDto {
        public List<RhythmElementDto> Elements { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public BeatPatternDto() { 
            Elements = new List<RhythmElementDto>();
        }
    }
}
