
namespace EZSong.Model {

    public class BeatPattern {

        public List<RhythmElement> Elements { get; set; }

        public RhythmRationalDuration ExpectedDuration {get; set;}

        public BeatPattern(List<RhythmElement> elements, RhythmRationalDuration expectedDuration) { 
            Elements = elements;    
            ExpectedDuration = expectedDuration;    
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1);

            foreach (RhythmElement e in Elements) {
                total += e.GetEffectiveDuration();
            }

            return total;
        }

        internal static BeatPattern FromDto(BeatPatternDto beat) {
            return new BeatPattern (
                beat.Elements.Select(e => RhythmElement.FromDto(e)).ToList(),
                beat.ExpectedDuration
            );

        }

        internal static BeatPatternDto ToDto(BeatPattern beat) {
            return new BeatPatternDto (
                beat.Elements.Select(e => RhythmElement.ToDto(e)).ToList(),
                beat.ExpectedDuration
            );
        }

        public int AttackCount {
            get {
                return Elements.Count(e => !e.IsRest);
            }
        }

    }

}