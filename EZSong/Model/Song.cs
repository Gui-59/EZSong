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

        /// <summary>
        /// Scinde un segment juste avant la mesure indiquée : les mesures à
        /// partir de cet index forment un nouveau segment inséré juste après
        /// le segment source. Les index des segments sont ensuite renumérotés
        /// pour rester cohérents avec leur position dans la liste.
        /// </summary>
        /// <param name="segmentIndex">Index du segment à scinder.</param>
        /// <param name="measureIndex">Index de la première mesure du nouveau segment.</param>
        /// <returns>L'index du nouveau segment, ou -1 si la scission est impossible.</returns>
        internal int SplitSegmentBefore(int segmentIndex, int measureIndex) {
            if (segmentIndex < 0 || segmentIndex >= Segments.Count) {
                return -1;
            }

            SegmentData segment = Segments[segmentIndex];

            // La scission avant la première mesure (ou hors du segment)
            // produirait un segment vide : on la refuse.
            if (measureIndex <= 0 || measureIndex >= segment.Measures.Count) {
                return -1;
            }

            List<MeasureData> movedMeasures =
                segment.Measures.GetRange(measureIndex, segment.Measures.Count - measureIndex);
            segment.Measures.RemoveRange(measureIndex, movedMeasures.Count);

            SegmentData newSegment =
                new(segmentIndex + 1, SongSettings, movedMeasures);
            Segments.Insert(segmentIndex + 1, newSegment);

            ReindexSegments();
            segment.ReindexMeasures();
            newSegment.ReindexMeasures();

            return segmentIndex + 1;
        }

        /// <summary>
        /// Renumérote les segments pour que leur index corresponde à leur
        /// position dans la liste.
        /// </summary>
        private void ReindexSegments() {
            for (int i = 0; i < Segments.Count; i++) {
                Segments[i].Index = i;
            }
        }
    }

}
