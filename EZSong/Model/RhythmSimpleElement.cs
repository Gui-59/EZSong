namespace EZSong.Model {

    public class RhythmSimpleElement:IRhythmElement {
        private RhythmRationalDuration _duration;

        private bool _isTiedToNext;

        private bool _isRest;

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public RhythmSimpleElement() {
            _duration = new RhythmRationalDuration(1, 4, 0);
            _isRest = false;
        }

        public RhythmSimpleElement(
            RhythmRationalDuration duration,
            bool isRest) {
            _duration = duration;
            _isRest = isRest;
        }

        public static RhythmSimpleElement FromDto(RhythmSimpleElementDto rhythmElementDto) {
            RhythmRationalDuration duration = new(rhythmElementDto.Numerator, rhythmElementDto.Denominator, rhythmElementDto.Dots);
            return new RhythmSimpleElement(
                duration,
                rhythmElementDto.IsRest
            ) {
                _isTiedToNext = rhythmElementDto.TieToNext
            };

        }

        internal static RhythmSimpleElementDto ToDto(RhythmSimpleElement rhythmElement) {
            return new RhythmSimpleElementDto() {
                Numerator = rhythmElement._duration.Numerator,
                Denominator = rhythmElement._duration.Denominator,
                Dots = rhythmElement._duration.Dots,
                IsRest = rhythmElement._isRest,
                TieToNext = rhythmElement._isTiedToNext
            };
        }

        public RhythmRationalDuration GetEffectiveDuration() {
            return _duration;
        }

        public int DotCount() {
            return _duration.Dots;
        }

        public bool IsTiedToNext() {
            return _isTiedToNext;
        }

        public bool IsRest() {
            return _isRest;
        }
    }

}