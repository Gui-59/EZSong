using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {
    public static class NoteStepStringifier {

        public static string ToLilyPondString(NoteStep note) {
            switch (note) {
                case NoteStep.C:
                    return "c";
                case NoteStep.D:
                    return "d";
                case NoteStep.E:
                    return "e";
                case NoteStep.F:
                    return "f";
                case NoteStep.G:
                    return "g";
                case NoteStep.A:
                    return "a";
                case NoteStep.B:
                    return "b";
            }
            return "?";
        }

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
