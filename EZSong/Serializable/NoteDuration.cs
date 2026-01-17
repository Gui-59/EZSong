using EZSong.Enums;
using EZSong.EnumsStringifier;
using EZSong.Exporting.Lilypond;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Serializable {




    [Serializable]
    public class NoteDuration {

        public WholeFraction Fraction;
        public bool Doted;
        public bool Doubledoted;

        private readonly ILilypondConverter _lilypondSerializer;

        public NoteDuration() {
            // Constructeur par défaut (requis pour la sérialisation)
            _lilypondSerializer = new LilypondConverter();
        }

        public NoteDuration(WholeFraction fraction, int dotsCount) {

            _lilypondSerializer = new LilypondConverter();

            Fraction = fraction;
            if (dotsCount == 1) {
                Doted = true;
                Doubledoted = false;
            } else if (dotsCount == 2) {
                Doted = true;
                Doubledoted = true;
            } else {
                Doted = false;
                Doubledoted = false;
            }
        }

        public string ToLilyPondString() {
            string lilyPondString = string.Empty;
            lilyPondString += _lilypondSerializer.WholeFractionToLilyPondString(Fraction);
            if (Doubledoted) {
                lilyPondString += "..";
            } else if (Doted) {
                lilyPondString += ".";
            }
            return lilyPondString;  
        }

    }
}
