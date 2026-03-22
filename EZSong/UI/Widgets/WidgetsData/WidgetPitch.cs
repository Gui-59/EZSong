using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.WidgetsData {

    public class WidgetPitch {

        // 0..6 for C D E F G A B (7 positions per octave)
        public int NoteIndex {
            get; set;
        }

        // octave offset relative to baseOctave (set on widget)
        public int OctaveOffset { get; set; } = 0;
        
        public Alteration Alteration { get; set; } = Alteration.neutral;
        
        public WidgetPitch() {
        }

        public WidgetPitch(int noteIndex, int octaveOffset = 0, Alteration alteration = Alteration.neutral) {
            NoteIndex = noteIndex;
            OctaveOffset = octaveOffset;
            Alteration = alteration;
        }

        internal string ToLogString() {;
            return 
                NoteIndex.ToString() 
                + "-" 
                + OctaveOffset.ToString() 
                + "-" 
                + Alteration.ToString();
        }

        internal Pitch ToPitch() {
            Pitch pitch = new();
           
            switch (NoteIndex) {
                case 0:
                    pitch.Note = NoteStep.C;
                    break;
                case 1:
                    pitch.Note = NoteStep.D;
                    break;
                case 2:
                    pitch.Note = NoteStep.E;
                    break;
                case 3:
                    pitch.Note = NoteStep.F;
                    break;
                case 4:
                    pitch.Note = NoteStep.G;
                    break;
                case 5:
                    pitch.Note = NoteStep.A;
                    break;
                case 6:
                    pitch.Note = NoteStep.B;
                    break;
            }

            pitch.Alteration = Alteration;
            pitch.OctaveOffset = OctaveOffset;

            return pitch;
        }
    }
}
