using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    public class TimeSignature {

        public int Beats;
        public int BeatUnit;

        public TimeSignature(int beat, int beatUnit) {
            Beats = beat;
            BeatUnit = beatUnit;
        }

        /// Durée totale de la mesure, en unités rationnelles
        public RhythmRationalDuration TotalDuration {
            get {
                return new RhythmRationalDuration(Beats, BeatUnit);
            }
        }

        public int GetBeatCount() {
            // Cas simple
            if (Beats % 3 == 0 && BeatUnit == 8 && Beats > 3) {
                // signature composée (6/8, 9/8, 12/8)
                return Beats / 3;
            }

            return Beats;
        }

        public RhythmRationalDuration GetBeatDuration() {
            // signature composée
            if (Beats % 3 == 0 && BeatUnit == 8 && Beats > 3) {
                // noire pointée = 3 croches
                return new RhythmRationalDuration(3, 8);
            }

            // cas simple
            return new RhythmRationalDuration(1, BeatUnit);
        }

        public string ToLilyPondString() {
            return Beats + "/" + BeatUnit;
        }
    }
}
