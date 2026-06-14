using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gtk;
using EZSong.Enums;

namespace EZSong.Model
{
    public class MeasureData
    {
        public int Index { 
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
        public MeasureGlobalMelody GlobalMelody { //TODO : Gérer des groupes de mélodies
            get; 
            set; 
        }
        public string Lyrics { 
            get; 
            set; 
        }


        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public MeasureData() { 
            TimeSignature = new TimeSignature();
            KeySignature = new KeySignature();
            ChordSequence = new ChordSequence();
            GlobalMelody = new MeasureGlobalMelody();
            Lyrics = string.Empty;
        }

        public MeasureData(int index, TimeSignature timeSignature, KeySignature keySignature, ChordSequence chordSequence, MeasureGlobalMelody globalMelody, string lyrics) {
            Index = index;
            TimeSignature = timeSignature;
            KeySignature = keySignature;
            ChordSequence = chordSequence;
            GlobalMelody = globalMelody;
            Lyrics = lyrics;
        }

        public MeasureDataDto ToDto() {

            TimeSignatureDto timeSignature = TimeSignature.ToDTo();

            KeySignatureDto keySignature = KeySignature.ToDto();

            ChordSequenceDto chordSequence = ChordSequence.ToDto();

            MeasureGlobalMelodyDto globalMelody = GlobalMelody.ToDto();

            String lyrics = Lyrics ?? string.Empty;

            return new MeasureDataDto() {
                Index = Index,
                TimeSignature = timeSignature,
                KeySignature = keySignature,
                ChordSequence = chordSequence,
                GlobalMelody = globalMelody,
                Lyrics = lyrics,          
            };
        }

        public static MeasureData FromDto(MeasureDataDto dto) {
            MeasureData measure = 
                new(
                    dto.Index, 
                    TimeSignature.FromDto(dto.TimeSignature), 
                    KeySignature.FromDto(dto.KeySignature),
                    ChordSequence.FromDto(dto.ChordSequence),
                    MeasureGlobalMelody.FromDto(dto.GlobalMelody), 
                    dto.Lyrics
                );               

            return measure;
        }
    }
}
