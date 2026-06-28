using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    public class TimeSignature {

        public int Beats {
            get; 
            set;
        }
        public int BeatUnit {
            get; 
            set;
        }
        
        public RhythmRationalDuration ExpectedBeatDuration {
            get {

                // signature composée
                if (Beats % 3 == 0 && BeatUnit == 8 && Beats > 3) {
                    // noire pointée = 3 croches
                    return new RhythmRationalDuration(3, 8, 0);
                }
                // cas simple
                return new RhythmRationalDuration(1, BeatUnit, 0);
            }
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public TimeSignature() {
            Beats = 4;
            BeatUnit = 4;
        }

        public TimeSignature(int beat, int beatUnit) {
            Beats = beat;
            BeatUnit = beatUnit;
        }

        /// Durée totale de la mesure, en unités rationnelles
        public RhythmRationalDuration TotalDuration {
            get {
                return new RhythmRationalDuration(Beats, BeatUnit, 0);
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
                return new RhythmRationalDuration(3, 8, 0);
            }

            // cas simple
            return new RhythmRationalDuration(1, BeatUnit, 0);
        }

        public string ToLilyPondString() {
            return Beats + "/" + BeatUnit;
        }


        internal static TimeSignature FromDto(TimeSignatureDto timeSignature) {
            return new TimeSignature(timeSignature.Beats, timeSignature.BeatUnit);
        }

        internal TimeSignatureDto ToDTo() {
            return new TimeSignatureDto(Beats, BeatUnit);
        }
    }
}
