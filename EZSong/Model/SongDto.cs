using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class SongDto {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Comment { get; set; }
        public List<MeasureDataDto> Measures { get; set; }

        public SongDto(string title, string artist, string comment, List<MeasureDataDto> measures) {
            Title = title;
            Artist = artist;    
            Comment = comment;  
            Measures = measures;    
        }
    }
}
