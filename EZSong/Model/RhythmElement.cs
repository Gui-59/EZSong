namespace EZSong.Model {

    public class RhythmElement {
        public RhythmRationalDuration Duration {
            get; set;
        }

        public bool TieToNext {
            get; set;
        }

        public bool IsRest {
            get; set;
        }

        public RhythmTuplet Tuplet {
            get;
        }


        public RhythmElement(
            RhythmRationalDuration duration,
            bool isRest,
            RhythmTuplet tuplet) {
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

        public static RhythmElement FromDto(RhythmElementDto rhythmElementDto) {
            RhythmRationalDuration duration = new(
                rhythmElementDto.Numerator,
                rhythmElementDto.Denominator,
                rhythmElementDto.Dots);
            return new RhythmElement(
                duration,
                rhythmElementDto.IsRest,
                RhythmTupletDto.FromDto(rhythmElementDto.Tuplet)
            ) {
                TieToNext = rhythmElementDto.TieToNext
            };

        }

        internal static RhythmElementDto ToDto(RhythmElement rhythmElement) {
            return new RhythmElementDto(
                rhythmElement.Duration.Numerator,
                rhythmElement.Duration.Denominator,
                rhythmElement.Duration.Dots,
                rhythmElement.IsRest,
                rhythmElement.TieToNext,
                RhythmTupletDto.ToDto(rhythmElement.Tuplet)
            );
        }
    }

}