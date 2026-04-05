
namespace EZSong.Model {

    public class BeatPattern {

        public List<RhythmElement> Elements { 
            get; 
            set; 
        }

        public RhythmRationalDuration ExpectedDuration {
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
            ExpectedDuration = new RhythmRationalDuration(0, 1, 0);
        }

        public BeatPattern(List<RhythmElement> elements, RhythmRationalDuration expectedDuration) { 
            Elements = elements;    
            ExpectedDuration = expectedDuration;    
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1, 0); //TODO ?

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
                elements,
                beat.ExpectedDuration
            );

        }

        internal static BeatPatternDto ToDto(BeatPattern beat) {
            List<RhythmElementDto> elements = new();
            foreach (RhythmElement element in beat.Elements) {
                elements.Add(RhythmElement.ToDto(element));
            }
            return new BeatPatternDto() {
                Elements = elements,
                ExpectedDuration=  beat.ExpectedDuration
            };
        }

    }

}