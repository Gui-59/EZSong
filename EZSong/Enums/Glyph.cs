using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Enums {

    /// <summary>
    /// Specifies musical glyphs used in notation, including notes, rests, ties, tuplets, and other symbols.
    /// </summary>
    /// <remarks>Each member represents a distinct musical symbol commonly found in sheet music.</remarks>
    public enum Glyph {
        Unknown,
        Dot,
        TieFrom,
        TieTo,
        tupletStart,
        tupletEnd,
        WholeNote,
        HalfNote,
        QuarterNote,
        EighthNote,
        SixteenthNote,
        WholeRest,
        HalfRest,
        QuarterRest,
        EighthRest,
        SixteenthRest,
        UndefindedDurationNote,
    }
}
