namespace EZSong.Model {

    public class BeatPattern {

        public List<IRhythmElement> Elements { 
            get; 
            set; 
        }

        public int AttackCount {
            get {
                int count = 0;
                foreach (IRhythmElement e in Elements) {
                    if (e.IsRest()) {
                        continue;
                    }
                    if (e.GetType() == typeof(RhythmTuplet)) {
                        RhythmTuplet tuplet = (RhythmTuplet)e;
                        count += tuplet.AttackCount();
                    } else {
                        count++;
                    }
                        
                }
                return count;
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
            foreach (IRhythmElementDto element in beat.Elements) {

                if (element.GetType() == typeof(RhythmSimpleElementDto)) {
                    RhythmSimpleElementDto simpleElementDto = (RhythmSimpleElementDto)element;
                    elements.Add(RhythmSimpleElement.FromDto(simpleElementDto));
                } else if (element.GetType() == typeof(RhythmTupletDto)) {
                    RhythmTupletDto tupletDto = (RhythmTupletDto)element;
                    elements.Add(RhythmTuplet.FromDto(tupletDto));
                }
            }

            return new BeatPattern (
                elements
            );

        }

        internal static BeatPatternDto ToDto(BeatPattern beat) {
            List<IRhythmElementDto> elements = new();
            foreach (IRhythmElement element in beat.Elements) {

                if (element.GetType() == typeof(RhythmSimpleElement)) {
                    RhythmSimpleElement simpleElement = (RhythmSimpleElement)element;
                    elements.Add(RhythmSimpleElement.ToDto(simpleElement));
                } else if (element.GetType() == typeof(RhythmTuplet)) {
                    RhythmTuplet tuplet = (RhythmTuplet)element;
                    elements.Add(RhythmTuplet.ToDto(tuplet));
                } 
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