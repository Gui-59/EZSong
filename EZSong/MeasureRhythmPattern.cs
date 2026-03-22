using EZSong.Serializable;

namespace EZSong {

    [Serializable]
    public class MeasureRhythmPattern {

        private readonly List<BeatPattern> _beats = new();

        public IReadOnlyList<BeatPattern> Beats {
            get {
                return _beats;
            }
        }

        public TimeSignature TimeSignature {
            get;
        }

        public MeasureRhythmPattern() {
            // Constructeur par défaut pour la sérialisation
            TimeSignature = default!;
        }

        public MeasureRhythmPattern(TimeSignature ts) {
            TimeSignature = ts;
            InitializeFromTimeSignature(ts);
        }

        public void SetBeat(int index, BeatPattern beat) {
            _beats[index] = beat;
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
            foreach (BeatPattern beat in Beats) {
                if (!beat.GetTotalDuration().Equals(beat.ExpectedDuration)) {
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

        public void InitializeFromTimeSignature(TimeSignature ts) {
            _beats.Clear();

            int beatCount = ts.GetBeatCount();
            RhythmRationalDuration beatDuration = ts.GetBeatDuration();

            for (int i = 0; i < beatCount; i++) {
                _beats.Add(new BeatPattern {
                    ExpectedDuration = beatDuration
                });
            }
        }

    }

}