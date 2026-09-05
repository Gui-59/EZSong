using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gtk;
using EZSong.Enums;
using EZSong.MIDI.Enums;

namespace EZSong.Model {
    public class MeasureData {

        public int Index { 
            get; 
            set; 
        }

        public SongSettings SongSettings {
            get;
            set;
        }

        public MeasureData? PrecedingMeasure {
            get; 
            set;
        }

        public MeasureData? FollowingMeasure {
            get; 
            set;
        }

        public TimeSignature TimeSignature { 
            get; 
            set; 
        }

        public KeySignature KeySignature { 
            get; 
            set; 
        }

        public ChordSequence ChordSequence { 
            get; 
            set; 
        }   
        
        public List<MeasureGlobalMelody> Staffs { //Groupes de portées
            get; 
            set; 
        }

        public string Lyrics { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public MeasureData() {
            SongSettings = new SongSettings();
            TimeSignature = new TimeSignature();
            KeySignature = new KeySignature();
            ChordSequence = new ChordSequence();
            Staffs = new List<MeasureGlobalMelody>();
            Staffs.Add(new MeasureGlobalMelody(0)); // Toujours au moins une portée
            Lyrics = string.Empty;
        }

        public MeasureData(int index, SongSettings songSettings, TimeSignature timeSignature, KeySignature keySignature, ChordSequence chordSequence, List<MeasureGlobalMelody> staffs, string lyrics) {
            Index = index;
            SongSettings = songSettings;
            TimeSignature = timeSignature;
            KeySignature = keySignature;
            ChordSequence = chordSequence;
            Staffs = staffs;
            Lyrics = lyrics;
        }

        public MeasureDataDto ToDto() {

            TimeSignatureDto timeSignature = TimeSignature.ToDTo();

            KeySignatureDto keySignature = KeySignature.ToDto();

            ChordSequenceDto chordSequence = ChordSequence.ToDto();

            List<MeasureGlobalMelodyDto> staffs = new();
            foreach (MeasureGlobalMelody staff in Staffs) {
                staffs.Add(staff.ToDto());            
            }

            String lyrics = Lyrics ?? string.Empty;

            return 
                new MeasureDataDto(
                    Index, 
                    timeSignature, 
                    keySignature, 
                    chordSequence, 
                    staffs, 
                    lyrics
                );
        }

        public static MeasureData FromDto(MeasureDataDto dto, SongSettings songSettings) {

            List<MeasureGlobalMelody> staffs = new();
            foreach (MeasureGlobalMelodyDto staff in dto.Staffs) {
                staffs.Add(MeasureGlobalMelody.FromDto(staff));
            }

            MeasureData measure = 
                new(
                    dto.Index,
                    songSettings,
                    TimeSignature.FromDto(dto.TimeSignature), 
                    KeySignature.FromDto(dto.KeySignature),
                    ChordSequence.FromDto(dto.ChordSequence),
                    staffs, 
                    dto.Lyrics
                );               

            return measure;
        }

        internal void AddOrRemoveStaffs(int excpectedStaffCount) {
            if (Staffs.Count() == excpectedStaffCount) {
                return;
            }

            while (Staffs.Count() > excpectedStaffCount) {
                Staffs.RemoveAt(Staffs.Count() - 1);
            }

            for (int staffIndex = Staffs.Count(); staffIndex < excpectedStaffCount; staffIndex++) {        
                Staffs.Add(new MeasureGlobalMelody(staffIndex));
            }
        }

        internal GMVoice GetGMVoiceForStaff(int staffIndex) {
            return SongSettings.GetGMVoiceForStaff(staffIndex);
        }
    }
}
