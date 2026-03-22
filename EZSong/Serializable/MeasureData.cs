using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gtk;
using EZSong.Enums;

namespace EZSong.Serializable
{
    [Serializable]
    public class MeasureData
    {
        public int Index { get; set; }

        public TimeSignature TimeSignature { get; set; } = new(4,4);
        public KeySignature KeySignature { get; set; } = new(NoteStep.C, Alteration.neutral, SongMode.major);
        public ChordSequence ChordSequence { get; set; } = new();       
        public MeasureMelody Melody { get; set; } = new();
        public string Lyrics { get; set; } = "";
        public MeasureRhythmPattern? RhythmPattern {
            get; set;
        }

        public MeasureData() {
            //Contructeur par défaut (requis pour la sérialisation)
        }
    }
}
