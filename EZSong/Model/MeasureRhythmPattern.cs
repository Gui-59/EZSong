using Cairo;

namespace EZSong.Model {

    public class MeasureRhythmPattern {

        private readonly List<BeatPattern> _beats = new();

        public IReadOnlyList<BeatPattern> Beats {
            get {
                return _beats;
            }
        }

        public TimeSignature TimeSignature {
            get; 
            set;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public MeasureRhythmPattern() {
            TimeSignature = new TimeSignature();
            InitializeFromTimeSignature(TimeSignature);
        }

        public MeasureRhythmPattern(TimeSignature ts) {
            TimeSignature = ts;
            InitializeFromTimeSignature(ts);
        }

        public void SetBeat(int index, BeatPattern beat) {
            _beats[index] = beat;
        }

        public RhythmRationalDuration GetTotalDuration() { //TODO : ne calcul pas correctement !!!

            RhythmRationalDuration total = new(0, 1, 0);

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
            RhythmRationalDuration totalDuration = GetTotalDuration();
            bool isValid = totalDuration.Equals(TimeSignature.TotalDuration);
            return isValid;
        }

        public bool AreBeatsValid() {
            foreach (BeatPattern beat in Beats) {
                if (!beat.GetTotalDuration().Equals(TimeSignature.ExpectedBeatDuration)) {
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
                List<RhythmElement> elements = new() {
                    new RhythmElement(
                        beatDuration,
                        true,
                        new RhythmTuplet(1, 1)
                    )
                };
                _beats.Add(new BeatPattern (
                    elements
                ));
            }
        }

        public MeasureRhythmPatternDto ToDto() {

            List<BeatPatternDto> beatDtos = new();
            foreach (BeatPattern beat in Beats) {
                beatDtos.Add(BeatPattern.ToDto(beat));
            }

            MeasureRhythmPatternDto measureRhythmPatternDto = new() {
                Beats = beatDtos,    
                TimeSignature = TimeSignature
            };

            return measureRhythmPatternDto;
        }
       
        public static MeasureRhythmPattern FromDto(MeasureRhythmPatternDto dto) {

            MeasureRhythmPattern pattern = new(dto.TimeSignature) {};

            int i = 0;
            foreach (BeatPatternDto beat in dto.Beats) {
                pattern.SetBeat(
                    i,
                    BeatPattern.FromDto(beat)
                );

                i++;
            }

            return pattern;

        }
    }
}