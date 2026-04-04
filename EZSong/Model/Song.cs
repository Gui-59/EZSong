using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gtk;

namespace EZSong.Model
{

    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; } 
        public string Comment { get; set; }
        public List<MeasureData> Measures { get; set; }

        public Song(string title, string artist, string comment, List<MeasureData> measures) {
            Title = title;
            Artist = artist;    
            Comment = comment;  
            Measures = measures;    
        }

        public SongDto ToDto() {
            return new SongDto () {
                Title = Title,
                Artist = Artist,
                Comment = Comment,
                Measures = Measures.Select(m => m.ToDto()).ToList()
            };
        }

        public static Song FromDto(SongDto dto) {

            List<MeasureData> measures = new();
            foreach (MeasureDataDto m in dto.Measures) {
                measures.Add(MeasureData.FromDto(m));
            }

            Song song = new(dto.Title, dto.Artist, dto.Comment, measures);

            return song;
        }
    }

}
