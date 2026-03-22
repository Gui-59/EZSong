using EZSong.Serializable;

namespace EZSong {

    public class MeasureRhythmPattern {

        public List<BeatPattern> Beats { get; } = new();

        public TimeSignature TimeSignature {
            get;
        }

        public MeasureRhythmPattern(TimeSignature ts) {
            TimeSignature = ts;
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1);

            foreach (BeatPattern beat in Beats) {
                total += beat.GetTotalDuration();
            }

            return total;
        }

        public int AttackCount {

            get {

                int count = 0;

                foreach (BeatPattern beat in Beats) {
                    count += beat.AttackCount;
                }

                return count;

            }
        }

        public bool IsDurationValid() {

            return GetTotalDuration().Equals(TimeSignature.TotalDuration);

        }

        public bool AreBeatsValid() {

            RhythmRationalDuration beatDuration =
                new(1, TimeSignature.BeatUnit);

            foreach (BeatPattern beat in Beats) {

                if (!beat.GetTotalDuration().Equals(beatDuration)) {
                    return false;
                }
            }

            return true;
        }

        public bool IsCompatibleWithNoteCount(int noteCount, int graceNoteCount) {

            int totalNotes = noteCount + graceNoteCount;

            return totalNotes == AttackCount;

        }

        private string DurationToSymbol(RhythmRationalDuration d) {
            string symbol = d.Denominator switch {
                1 => "w",
                2 => "h",
                4 => "q",
                8 => "e",
                16 => "s",
                32 => "t",
                _ => $"1/{d.Denominator}"
            };

            if (d.Dots > 0) {
                symbol += new string('.', d.Dots);
            }

            return symbol;
        }

        private string ElementToString(RhythmElement e) {
            string s = DurationToSymbol(e.Duration);

            if (e.IsRest) {
                s = "r" + s;
            }

            if (e.TieToNext) {
                s += "~";
            }

            return s;
        }

        public override string ToString() {
            List<string> beatStrings = new();

            foreach (BeatPattern beat in Beats) {
                List<string> elements = new();

                foreach (RhythmElement e in beat.Elements) {
                    elements.Add(ElementToString(e));
                }

                beatStrings.Add(string.Join(" ", elements));
            }

            return string.Join(" : ", beatStrings) ;
        }

    }

}