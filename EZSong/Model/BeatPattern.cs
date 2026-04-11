namespace EZSong.Model {

    public class BeatPattern {

        public List<RhythmElement> Elements { 
            get; 
            set; 
        }

        public int AttackCount {
            get {
                return Elements.Count(e => !e.IsRest);
            }
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public BeatPattern() {
            Elements = new List<RhythmElement>();
        }

        public BeatPattern(List<RhythmElement> elements) { 
            Elements = elements;      
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1, 0);

            foreach (RhythmElement e in Elements) {
                total += e.GetEffectiveDuration();
            }

            return total;
        }

        internal static BeatPattern FromDto(BeatPatternDto beat) {
            List<RhythmElement> elements = new();
            foreach (RhythmElementDto elementDto in beat.Elements) {
                elements.Add(RhythmElement.FromDto(elementDto));
            }

            return new BeatPattern (
                elements
            );

        }

        internal static BeatPatternDto ToDto(BeatPattern beat) {
            List<RhythmElementDto> elements = new();
            foreach (RhythmElement element in beat.Elements) {
                elements.Add(RhythmElement.ToDto(element));
            }
            return new BeatPatternDto() {
                Elements = elements
            };
        }

        internal void Clear() {
            Elements.Clear();

        }

        internal RhythmRationalDuration GetRemainingDuration(RhythmRationalDuration beatDuration) {
            return beatDuration - GetTotalDuration();
        }

        internal bool CanAdd(RhythmElement element, RhythmRationalDuration beatDuration) {
            RhythmRationalDuration eltDuration = element.GetEffectiveDuration();
            if (eltDuration.Numerator <= 0) {
                return false;
            }
            RhythmRationalDuration remaining = beatDuration - GetTotalDuration();
            return (remaining - eltDuration).Numerator >= 0;
        }
    }

}