using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {
    public static class NoteStepStringifier {
        public static string ToHumanString(NoteStep note, bool internationalNotation) {
            switch (note) {
                case NoteStep.C:
                    return internationalNotation ? "C" : "Do";
                case NoteStep.D:
                    return internationalNotation ? "D" : "Ré";
                case NoteStep.E:
                    return internationalNotation ? "E" : "Mi";
                case NoteStep.F:
                    return internationalNotation ? "F" : "Fa";
                case NoteStep.G:
                    return internationalNotation ? "G" : "Sol";
                case NoteStep.A:
                    return internationalNotation ? "A" : "La";
                case NoteStep.B:
                    return internationalNotation ? "B" : "Si";
            }
            return "?";
        }
    }
}
