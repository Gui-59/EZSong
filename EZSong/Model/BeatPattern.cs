namespace EZSong.Model {

    public class BeatPattern {

        public List<IRhythmElement> Elements { 
            get; 
            set; 
        }

        public int AttackCount {
            get {
                return Elements.Count(e => !e.IsRest());
            }
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public BeatPattern() {
            Elements = new List<IRhythmElement>();
        }

        public BeatPattern(List<IRhythmElement> elements) { 
            Elements = elements;      
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1, 0);

            foreach (IRhythmElement e in Elements) {
                total += e.GetEffectiveDuration();
            }

            return total;
        }

        internal static BeatPattern FromDto(BeatPatternDto beat) {
            List<IRhythmElement> elements = new();
            foreach (RhythmSimpleElementDto elementDto in beat.Elements) {
                elements.Add(RhythmSimpleElement.FromDto(elementDto));
            }

            return new BeatPattern (
                elements
            );

        }

        internal static BeatPatternDto ToDto(BeatPattern beat) {
            List<RhythmSimpleElementDto> elements = new();
            foreach (RhythmSimpleElement element in beat.Elements) {
                elements.Add(RhythmSimpleElement.ToDto(element));
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

        internal bool CanAdd(IRhythmElement element, RhythmRationalDuration beatDuration) {
            RhythmRationalDuration eltDuration = element.GetEffectiveDuration();
            if (eltDuration.Numerator <= 0) {
                return false;
            }
            RhythmRationalDuration remaining = beatDuration - GetTotalDuration();
            return (remaining - eltDuration).Numerator >= 0;
        }
    }

}