using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {

    /// <summary>
    /// Stringify "NoteStep" enum
    /// </summary>
    public static class NoteStepStringifier {

        /// <summary>
        /// Stringify "NoteStep" enum
        /// </summary>
        /// <param name="note">The note step value to convert to a human-readable string.</param>
        /// <param name="internationalNotation">Whether to use international notation (C, D, E, etc.) or French notation (Do, Ré, Mi, etc.).</param>
        /// <returns>A human-readable string representation of the note step.</returns>
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
