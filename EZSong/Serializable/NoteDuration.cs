using Gtk;
using EZSong.Enums;
using EZSong.EnumsStringifier;
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

        public NoteDuration() {
            // Constructeur par défaut (requis pour la sérialisation)
        }

        public NoteDuration(WholeFraction fraction, int dotsCount) {
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
            lilyPondString += WholeFractionStringifier.ToLilyPondString(Fraction);
            if (Doubledoted) {
                lilyPondString += "..";
            } else if (Doted) {
                lilyPondString += ".";
            }
            return lilyPondString;  
        }

    }
}
