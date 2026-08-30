using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Gtk;

namespace EZSong.Model {

    public class Song {

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

        public SongSettings SongSettings;
        public List<SegmentData> Segments;

        //Constructeur vide (requis, entre autres, pour la (dé)sérialisation JSON)
        public Song() { 
            Title = string.Empty;
            Artist = string.Empty;
            Comment = string.Empty;
            SongSettings = new(new StaffsSettings());
            Segments = new List<SegmentData>();
            Segments.Add(new SegmentData(0, SongSettings)); // Toujours au moins un segment
        }

        public Song(string title, string artist, string comment, List<SegmentData> segments, SongSettings songSettings) {
            Title = title;
            Artist = artist;    
            Comment = comment;
            SongSettings = songSettings;
            Segments = segments;
        }

        public SongDto ToDto() {

            List<SegmentDataDto> segments = new();
            foreach (SegmentData s in Segments) {
                segments.Add(s.ToDto());
            }
            return 
                new SongDto (
                    Title, 
                    Artist, 
                    Comment, 
                    segments, 
                    SongSettings.ToDto()
                );
        }

        public static Song FromDto(SongDto dto) {

            SongSettings songSettings = SongSettings.FromDto(dto.SongSettings);

            List<SegmentData> segments = new();
            foreach (SegmentDataDto s in dto.Segments) {
                segments.Add(SegmentData.FromDto(s, songSettings));
            }

            Song song = new(dto.Title, dto.Artist, dto.Comment, segments, songSettings);

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

            foreach (SegmentData segment in Segments) {
                segment.AddOrRemoveMesuresStaffs(excpectedStaffCount);
            }            
        }

        internal void AddSegment() {
            SegmentData newSegement = new(Segments.Count, SongSettings);
            Segments.Add(newSegement);
        }
    }

}
