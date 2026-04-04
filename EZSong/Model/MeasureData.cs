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
        public int Index { get; set; }

        public TimeSignature TimeSignature { get; set; }
        public KeySignature KeySignature { get; set; }
        public ChordSequence ChordSequence { get; set; }      
        public MeasureMelody Melody { get; set; }
        public string Lyrics { get; set; }
        public MeasureRhythmPattern RhythmPattern {get; set;}

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

            TimeSignatureDto timeSignature = new() {
                Beats = TimeSignature.Beats,
                BeatUnit = TimeSignature.BeatUnit
            };

            KeySignatureDto keySignature = new(KeySignature.Note, KeySignature.Alteration, KeySignature.Mode);


            ChordSequenceDto chordSequence = ChordSequence.ToDto();


            MeasureMelodyDto melody = Melody.ToDto();

            MeasureRhythmPatternDto rhythm = RhythmPattern.ToDto();

            String lyrics = Lyrics ?? string.Empty;

            return new MeasureDataDto(timeSignature, keySignature, chordSequence, melody, lyrics, rhythm);
        }

        public static MeasureData FromDto(MeasureDataDto dto) {
            TimeSignature ts = new(dto.TimeSignature.Beats, dto.TimeSignature.BeatUnit);

            MeasureData measure = 
                new(
                    dto.Index, 
                    ts, 
                    KeySignature.FromDto(dto.KeySignature),  
                    ChordSequence.FromDto(dto.ChordSequence), 
                    MeasureMelody.FromDto(dto.Melody), 
                    dto.Lyrics, 
                    MeasureRhythmPattern.FromDto(dto.Rhythm)
                );               
            

            measure.RhythmPattern = MeasureRhythmPattern.FromDto(dto.Rhythm);

            // KeySignature
            if (dto.KeySignature != null) {
                measure.KeySignature = new KeySignature(dto.KeySignature.Note, dto.KeySignature.Alteration, dto.KeySignature.Mode);
            }

            // Chords
            measure.ChordSequence = ChordSequence.FromDto(dto.ChordSequence);

            // Melody
            if (dto.Melody != null) {
                measure.Melody = MeasureMelody.FromDto(dto.Melody);
            }

            return measure;
        }
    }
}
