using EZSong.Enums;
using EZSong.Model;
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
            
            NoteStep note = NoteStep.C; // default value, will be overwritten by switch
            switch (NoteIndex) {
                case 0:
                    note = NoteStep.C;
                    break;
                case 1:
                    note = NoteStep.D;
                    break;
                case 2:
                    note = NoteStep.E;
                    break;
                case 3:
                    note = NoteStep.F;
                    break;
                case 4:
                    note = NoteStep.G;
                    break;
                case 5:
                    note = NoteStep.A;
                    break;
                case 6:
                    note = NoteStep.B;
                    break;
            }

            Pitch pitch = new(note, Alteration, OctaveOffset);
            return pitch;
        }
    }
}
