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
        public List<MeasureData> Measures { 
            get; 
            set; 
        }

        //Constructeur vide (requis, entre autres, pour la (dé)sérialisation JSON)
        public Song() { 
            Title = string.Empty;
            Artist = string.Empty;
            Comment = string.Empty;
            Measures = new List<MeasureData>();
        }

        public Song(string title, string artist, string comment, List<MeasureData> measures) {
            Title = title;
            Artist = artist;    
            Comment = comment;  
            Measures = measures;    
        }

        public SongDto ToDto() {
            List<MeasureDataDto> measures = new();
            foreach (MeasureData m in Measures) {
                measures.Add(m.ToDto());
            }
            return new SongDto () {
                Title = Title,
                Artist = Artist,
                Comment = Comment,
                Measures = measures
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
