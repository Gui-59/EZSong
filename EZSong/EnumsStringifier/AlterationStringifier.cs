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
                    return "b"; //TODO : mettre un vrai flat unicode
                case Alteration.neutral:
                    return "";
                case Alteration.sharp:
                    return "#"; //TODO : mettre un vrai sharp unicode
            }
            return "?";
        }
    }
}
