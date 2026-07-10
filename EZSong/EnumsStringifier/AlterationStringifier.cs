using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {
    internal class AlterationStringifier {
        public static string ToHumanString(Alteration alteration) {
            switch (alteration) {
                case Alteration.flat:
                    return "b";
                case Alteration.neutral:
                    return "";
                case Alteration.sharp:
                    return "#";
            }
            return "?";
        }
    }
}
