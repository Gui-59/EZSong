using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.EnumsStringifier {
    public class ChordTypeStringifier {
        public static string ToHumanString(ChordType chordType) {
            switch(chordType) {
                case ChordType.NoneOrMajor:
                    return "";
                case ChordType.Minor:
                    return "m";
                case ChordType.Seventh:
                    return "7";
                case ChordType.MinorSeventh:
                    return "m7";
                case ChordType.MajorSeventh:
                    return "maj7";
                case ChordType.PowerChord:
                    return "5";
                case ChordType.Sixth:
                    return "6";
                case ChordType.MinorSixth:
                    return "m6";
                case ChordType.SuspendedSecond:
                    return "sus2";
                case ChordType.SuspendedFourth:
                    return "sus4";
                case ChordType.Diminished:
                    return "dim";
                case ChordType.Augmented:
                    return "aug";
                case ChordType.DiminishedSeventh:
                    return "dim7";
                case ChordType.AugmentedSeventh:
                    return "aug7";
                case ChordType.AddSecond:
                    return "add2";
                case ChordType.AddFourth:
                    return "add4";
                case ChordType.AddSixth:
                    return "add6";
                case ChordType.AddNinth:
                    return "add9";
                case ChordType.Ninth:
                    return "9";
                case ChordType.MinorNinth:
                    return "m9";
                case ChordType.MajorNinth:
                    return "maj9";
                case ChordType.Eleventh:
                    return "11";
                case ChordType.MinorEleventh:
                    return "m11";
                case ChordType.MajorEleventh:
                    return "maj11";
                case ChordType.Thirteenth:
                    return "13";
                case ChordType.MinorThirteenth:
                    return "m13";
                case ChordType.MajorThirteenth:
                    return "maj13";
                case ChordType.MinorMajorSeventh:
                    return "m(maj7)";
                case ChordType.SixthNinth:
                    return "6/9";
                case ChordType.SeventhMinusFive:
                    return "7-5";
                case ChordType.SeventhPlusFive:
                    return "7+5";
                case ChordType.MinorSeventhFlatFive:
                    return "m7b5";
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

    }
}
