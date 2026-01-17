using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {
    internal class AlterationStringifier {

        public static string ToLilyPondString(Alteration alteration) {
            switch (alteration) {
                case Alteration.flat:
                    return "es";
                case Alteration.flatflat:
                    return "esee";
                case Alteration.neutral:
                    return "";
                case Alteration.sharp:
                    return "is";
                case Alteration.sharpsharp:
                    return "isis";
            }
            return "?";
        }

        public static string ToHumanString(Alteration alteration) {
            switch (alteration) {
                case Alteration.flat:
                    return "b";
                case Alteration.flatflat:
                    return "bb";
                case Alteration.neutral:
                    return "";
                case Alteration.sharp:
                    return "#";
                case Alteration.sharpsharp:
                    return "##";
            }
            return "?";
        }
    }
}
