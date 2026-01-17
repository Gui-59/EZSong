using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Enums {

    /*
     * Durée            Code LilyPond   Nom français
     * ronde            1	            1
     * blanche          2	            1/2
     * noire            4	            1/4
     * croche           8	            1/8
     * double croche    16	            1/16
     * triple croche    32	            1/32
     */

    public enum WholeFraction {
        WHOLE = 1,
        HALF = 2,
        QUARTER = 4,
        EIGHTH = 8,
        SIXTEENTH = 16,
        THIRTYSECOND =  32
    }
}
