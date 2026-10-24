using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {

    /// <summary>
    /// Stringify "SongMode" enum
    /// </summary>
    public static class SongModeStringifier {

        /// <summary>
        /// Stringify "SongMode" enum
        /// </summary>
        /// <param name="mode">The song mode value to convert to a human-readable string.</param>
        /// <returns>A human-readable string representation of the song mode.</returns>
        public static string ToHumanString(SongMode mode) {
            switch (mode) {
                case SongMode.minor:
                    return "min.";
                case SongMode.major:
                    return "maj.";
            }
            return "?";
        }
    }
}
