using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {
    public static class SongModeStringifier {

        public static string ToLilyPondString(SongMode mode) {
            switch (mode) {
                case SongMode.minor:
                    return "\\minor";
                case SongMode.major:
                    return "\\major";
            }
            return "?";
        }

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
