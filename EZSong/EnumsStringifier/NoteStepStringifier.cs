using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {
    public static class NoteStepStringifier {

        

        public static string ToHumanString(NoteStep note) {
            switch (note) {
                case NoteStep.C:
                    return "C";
                case NoteStep.D:
                    return "D";
                case NoteStep.E:
                    return "E";
                case NoteStep.F:
                    return "F";
                case NoteStep.G:
                    return "G";
                case NoteStep.A:
                    return "A";
                case NoteStep.B:
                    return "B";
            }
            return "?";
        }
    }
}
