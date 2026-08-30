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

        public string Title { 
            get; 
            set; 
        }

        public string Artist { 
            get; 
            set; 
        }

        public string Comment { 
            get; 
            set; 
        }

        public SongSettingsDto SongSettings {
            get;
            set;
        }

        public List<SegmentDataDto> Segments { 
            get;
            set; 
        }       

        //Constructeur vide (requis pour la sérialisation JSON)
        public SongDto() {
            Title = string.Empty;
            Artist = string.Empty;  
            Comment = string.Empty;
            SongSettings = new();
            Segments = new List<SegmentDataDto>();
        }

        public SongDto(String title, String artist, String comment, List<SegmentDataDto> segments, SongSettingsDto songSettings) {
            Title = title;
            Artist = artist;
            Comment = comment;
            SongSettings = songSettings;
            Segments = segments;
        }
    }
}
