using EZSong.Model;
using EZSong.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.Helpers {
    public class UIGlyph {

        

        public Enums.Glyph Glyph {
            get;
        }

        public UIGlyph(Enums.Glyph glyph) {
            Glyph = glyph;
        }

        public static UIGlyph DotGlyph() {
            return new UIGlyph(Enums.Glyph.Dot);
        }

        public static UIGlyph TiefromGlyph() {
            return new UIGlyph(Enums.Glyph.TieFrom);
        }

        public static UIGlyph FromDescriptor(RhythmRationalDuration duration, bool isRest) {

            UIGlyph? glyph;
            if (isRest) {

                switch (duration.Denominator) {
                    case 1:
                        glyph = new UIGlyph(Enums.Glyph.WholeRest); // ronde 
                        break;
                    case 2:
                        glyph = new UIGlyph(Enums.Glyph.HalfRest); // blanche
                        break;
                    case 4:
                        glyph = new UIGlyph(Enums.Glyph.QuarterRest); // noire
                        break;
                    case 8:
                        glyph = new UIGlyph(Enums.Glyph.EighthRest); // croche
                        break;
                    case 16:
                        glyph = new UIGlyph(Enums.Glyph.SixteenthRest); // double croche
                        break;
                    default:
                        glyph = new UIGlyph(Enums.Glyph.Unknown);
                        break;
                }

 
            } else {

                switch (duration.Denominator) {
                    case 1:
                        glyph = new UIGlyph(Enums.Glyph.WholeNote); // ronde
                        break;
                    case 2:
                        glyph = new UIGlyph(Enums.Glyph.HalfNote); // blanche
                        break;
                    case 4:
                        glyph = new UIGlyph(Enums.Glyph.QuarterNote); // noire
                        break;
                    case 8:
                        glyph = new UIGlyph(Enums.Glyph.EighthNote); // croche
                        break;
                    case 16:
                        glyph = new UIGlyph(Enums.Glyph.SixteenthNote); // double croche
                        break;
                    default:
                        glyph = new UIGlyph(Enums.Glyph.Unknown);
                        break;
                }
            }

            return glyph;
        }

        public override string ToString() {

            //https://w3c-cg.github.io/smufl/latest/tables/function-theory-symbols.html

            //https://w3c-cg.github.io/smufl/latest/tables/metronome-marks.html

            //https://w3c-cg.github.io/smufl/latest/tables/tuplets.html

            string _musicalFontFamily = "Bravura"; //TODO: make this dynamic based on settings
            if (_musicalFontFamily == "") {
                return "Glyph not loaded";
            }
            return Glyph switch {
                Enums.Glyph.Dot => "\uECB7", //OK
                Enums.Glyph.TieFrom => "\uE551",
                Enums.Glyph.TieTo => "\uE551",
                Enums.Glyph.WholeNote => "\uECA2", //OK
                Enums.Glyph.HalfNote => "\uECA3", //OK
                Enums.Glyph.QuarterNote => "\uECA5", //OK
                Enums.Glyph.EighthNote => "\uECA7", //OK
                Enums.Glyph.SixteenthNote => "\uECA9", //OK

                Enums.Glyph.WholeRest => "\uE4E3", //OK
                Enums.Glyph.HalfRest => "\uE4E4", //OK
                Enums.Glyph.QuarterRest => "\uE4E5", //OK
                Enums.Glyph.EighthRest => "\uE4E6", //OK
                Enums.Glyph.SixteenthRest => "\uE4E7", //OK

                Enums.Glyph.tupletStart => "\uEA7A",
                Enums.Glyph.tupletEnd => "\uEA7C", 

                _ => "\uE120" //OK
            };
        }


    }
}
