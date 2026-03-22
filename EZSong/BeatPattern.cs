namespace EZSong {

    public class BeatPattern {

        public List<RhythmElement> Elements { get; } = new();

        public RhythmRationalDuration ExpectedDuration {
            get; set;
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1);

            foreach (RhythmElement e in Elements) {
                total += e.GetEffectiveDuration();
            }

            return total;
        }

        public int AttackCount {
            get {
                return Elements.Count(e => !e.IsRest);
            }
        }

    }

}