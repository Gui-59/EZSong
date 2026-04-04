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
    public class SongDto {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Comment { get; set; }
        public List<MeasureDataDto> Measures { get; set; }

        //Constructeur vide (requis pour la sérialisation JSON)
        public SongDto() {
            Title = string.Empty;
            Artist = string.Empty;  
            Comment = string.Empty; 
            Measures = new List<MeasureDataDto>();
        }
    }
}
