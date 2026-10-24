using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {

    /// <summary>
    /// Stringify "Alteration" enum
    /// </summary>
    internal class AlterationStringifier {

        /// <summary>
        /// Stringify "Alteration" enum
        /// </summary>
        /// <param name="alteration">The alteration value to convert to a human-readable string.</param>
        /// <returns>A human-readable string representation of the alteration.</returns>
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
