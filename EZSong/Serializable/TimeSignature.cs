using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Serializable {

    [Serializable]
    public class TimeSignature {

        public int Beats;
        public int BeatUnit;

        public TimeSignature() {
            // Constructeur par défaut (requis pour la sérialisation)
        }

        public TimeSignature(int beat, int beatUnit) {
            Beats = beat;
            BeatUnit = beatUnit;
        }

        /// Durée totale de la mesure, en unités rationnelles
        public RationalDuration TotalDuration {
            get {
                return new RationalDuration(Beats, BeatUnit);
            }
        }

        public string ToLilyPondString() {
            return Beats + "/" + BeatUnit;
        }
    }
}
