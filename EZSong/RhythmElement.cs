namespace EZSong {

    [Serializable]
    public class RhythmElement {
        public RhythmRationalDuration Duration {
            get;
        }

        public bool TieToNext {
            get; set;
        }

        public bool IsRest {
            get; set;
        }

        public RhythmTuplet? Tuplet {
            get;
        }

        /// Constructeur par défaut pour la sérialisation
        public RhythmElement() {
        
        }

        public RhythmElement(
            RhythmRationalDuration duration,
            bool isRest = false,
            RhythmTuplet? tuplet = null) {
            Duration = duration;
            IsRest = isRest;
            Tuplet = tuplet;
        }

        public RhythmRationalDuration GetEffectiveDuration() {
            if (Tuplet == null) {
                return Duration;
            }

            RhythmRationalDuration baseDur = Duration.ApplyDots();

            int num = baseDur.Numerator * Tuplet.InTimeOf;
            int den = baseDur.Denominator * Tuplet.Count;

            return new RhythmRationalDuration(num, den).Normalize();
        }
    }

}