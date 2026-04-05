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
        public MeasureMelody Melody { 
            get; 
            set; 
        }
        public string Lyrics { 
            get; 
            set; 
        }
        public MeasureRhythmPattern RhythmPattern {
            get; 
            set;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public MeasureData() { 
            TimeSignature = new TimeSignature();
            KeySignature = new KeySignature();
            ChordSequence = new ChordSequence();
            Melody = new MeasureMelody();
            Lyrics = string.Empty;
            RhythmPattern = new MeasureRhythmPattern();
        }

        public MeasureData(int index, TimeSignature timeSignature, KeySignature keySignature, ChordSequence chordSequence, MeasureMelody melody, string lyrics, MeasureRhythmPattern rhythmPattern) {
            Index = index;
            TimeSignature = timeSignature;
            KeySignature = keySignature;
            ChordSequence = chordSequence;
            Melody = melody;
            Lyrics = lyrics;
            RhythmPattern = rhythmPattern;
        }

        public MeasureDataDto ToDto() {

            TimeSignatureDto timeSignature = TimeSignature.ToDTo();

            KeySignatureDto keySignature = KeySignature.ToDto();

            ChordSequenceDto chordSequence = ChordSequence.ToDto();

            MeasureMelodyDto melody = Melody.ToDto();

            MeasureRhythmPatternDto rhythm = RhythmPattern.ToDto();

            String lyrics = Lyrics ?? string.Empty;

            return new MeasureDataDto() {
                Index = Index,
                TimeSignature = timeSignature,
                KeySignature = keySignature,
                ChordSequence = chordSequence,
                Melody = melody,
                Lyrics = lyrics,
                Rhythm = rhythm              
            };
        }

        public static MeasureData FromDto(MeasureDataDto dto) {
            TimeSignature ts = new(dto.TimeSignature.Beats, dto.TimeSignature.BeatUnit);

            MeasureData measure = 
                new(
                    dto.Index, 
                    TimeSignature.FromDto(dto.TimeSignature), 
                    KeySignature.FromDto(dto.KeySignature),
                    ChordSequence.FromDto(dto.ChordSequence),
                    MeasureMelody.FromDto(dto.Melody), 
                    dto.Lyrics,
                    MeasureRhythmPattern.FromDto(dto.Rhythm, ts)
                );               

            return measure;
        }
    }
}
