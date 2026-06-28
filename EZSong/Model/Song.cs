using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
        public List<MeasureData> Measures;
        public SongSettings SongSettings;

        //Constructeur vide (requis, entre autres, pour la (dé)sérialisation JSON)
        public Song() { 
            Title = string.Empty;
            Artist = string.Empty;
            Comment = string.Empty;
            SongSettings = new(new StaffsSettings());
            Measures = new List<MeasureData>();
            
        }

        public Song(string title, string artist, string comment, List<MeasureData> measures, SongSettings songSettings) {
            Title = title;
            Artist = artist;    
            Comment = comment;
            SongSettings = songSettings;
            Measures = measures;
            
        }

        public SongDto ToDto() {
            List<MeasureDataDto> measures = new();
            foreach (MeasureData m in Measures) {
                measures.Add(m.ToDto());
            }
            return 
                new SongDto (
                    Title, 
                    Artist, 
                    Comment, 
                    measures, 
                    SongSettings.ToDto()
                );
        }

        public static Song FromDto(SongDto dto) {

            SongSettings songSettings = SongSettings.FromDto(dto.SongSettings);

            List<MeasureData> measures = new();
            foreach (MeasureDataDto m in dto.Measures) {
                measures.Add(MeasureData.FromDto(m, songSettings));
            }

            Song song = new(dto.Title, dto.Artist, dto.Comment, measures, songSettings);

            return song;
        }

        internal void AddStaff(string staffName, bool isBass) {

            int baseOctave;
            if (!isBass) {
                baseOctave = Settings.Constants.MelodyBaseOctave;
            } else {
                baseOctave = Settings.Constants.BassBaseOctave;
            }

            int newStaffCount = SongSettings.AddStaff(staffName, MIDI.Enums.GMVoice.PIANO_AcousticGrand, baseOctave);
            AddOrRemoveMesuresStaffs(newStaffCount);
        }

        private void AddOrRemoveMesuresStaffs(int excpectedStaffCount) {
            foreach (MeasureData measure in Measures) {
                measure.AddOrRemoveStaffs(excpectedStaffCount);
            }
        }
    }

}
