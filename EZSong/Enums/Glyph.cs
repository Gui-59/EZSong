using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Enums {
    public enum Glyph {
        Unknown,
        Dot,
        TieFrom,
        TieTo,
        tupletStart,
        tupletEnd,
        WholeNote, //Ronde
        HalfNote, //Blanche
        QuarterNote, //Noire
        EighthNote, //Croche
        SixteenthNote, //Double croche
        WholeRest, //Ronde  
        HalfRest, //Blanche
        QuarterRest, //Noire
        EighthRest, //Croche
        SixteenthRest, //Double croche
        UndefindedDurationNote, //Note sans durée définie
    }
}
