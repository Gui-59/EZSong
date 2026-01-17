using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {

    /*
     * Durée            Code LilyPond   Nom français
     * ronde            1	            1
     * blanche          2	            1/2
     * noire            4	            1/4
     * croche           8	            1/8
     * double croche    16	            1/16
     * triple croche    32	            1/32
     */

    internal class WholeFractionStringifier {
        public static string ToLilyPondString(WholeFraction fraction) {
            switch ((int)fraction) {
                case 1:
                    return "1";
                case 2:
                    return "2";
                case 4:
                    return "4";
                case 8:
                    return "8";
                case 16:
                    return "16";
                case 32:
                    return "32";
                default:
                    return "?";
            }

        }
    }
}
