using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI {
    public class SelectableValues {

        public KeySignature DefaultKeySignature;
        public Dictionary<string, string> Tonalities;

        public int DefaultUpperTimeSig;
        public int[] UpperTimeSigs;

        public int DefaultLowerTimeSig;
        public int[] LowerTimeSigs;

        public SelectableValues() {

            DefaultKeySignature = new(NoteStep.C, Alteration.neutral, SongMode.major);

            Tonalities = new() {
                {
                    new KeySignature(NoteStep.C, Alteration.neutral, SongMode.major).ToDropDownId(),
                    new KeySignature(NoteStep.C, Alteration.neutral, SongMode.major).ToDropDownLabel()
                },
                {
                    new KeySignature(NoteStep.A, Alteration.neutral, SongMode.major).ToDropDownId(),
                    new KeySignature(NoteStep.A, Alteration.neutral, SongMode.major).ToDropDownLabel()
                }
            };


            DefaultUpperTimeSig = 4;
            UpperTimeSigs = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            DefaultLowerTimeSig = 4;
            LowerTimeSigs = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        }

   
    }
}
