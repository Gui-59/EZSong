using Cairo;

namespace EZSong.Model {

    public class MeasureRhythmPattern {

        public int StaffIndex {
            get;
            set;
        }

        private readonly List<BeatPattern> _beats = new();

        public IReadOnlyList<BeatPattern> Beats {
            get {
                return _beats;
            }
        }

        private TimeSignature _timeSignature;
        public TimeSignature TimeSignature {
            get {
                return _timeSignature;
            }
            set {
                _timeSignature = value;
                //On doit s'assurer de garder le bon nombre de beats
                if (_beats.Count > TimeSignature.GetBeatCount()) {
                    // Supprimer les beats excédentaires
                    _beats.RemoveRange(TimeSignature.GetBeatCount(), _beats.Count - TimeSignature.GetBeatCount());
                } else if (_beats.Count < TimeSignature.GetBeatCount()) {
                    // Ajouter des beats manquants
                    for (int i = _beats.Count; i < TimeSignature.GetBeatCount(); i++) {
                        _beats.Add(new BeatPattern(TimeSignature.GetBeatDuration()));
                    }
                }
            }
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public MeasureRhythmPattern() {
            StaffIndex = 0; //Par défaut
            _timeSignature = new TimeSignature();
            InitializeFromTimeSignature(TimeSignature);
        }

        public MeasureRhythmPattern(int staffIndex) {
            StaffIndex = staffIndex;
            _timeSignature = new TimeSignature();
            InitializeFromTimeSignature(TimeSignature);
        }

        public MeasureRhythmPattern(int staffIndex, TimeSignature ts) {
            StaffIndex = staffIndex;
            _timeSignature = ts;
            InitializeFromTimeSignature(ts);
        }

        public void SetBeat(int index, BeatPattern beat) { //TODO : utiliser le setter de Beats pour gérer le nombre de beats automatiquement
            _beats[index] = beat;
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1, 0);

            foreach (BeatPattern beat in Beats) {
                total += beat.GetTotalDuration();
            }

            return total;
        }

        // Il faut transmettre l'eventuelle mesure précédente pour gérer les liaisons correctement
        public int GetAttackCount(MeasureData? previousMeasure) {

            int count = 0;

            BeatPattern? previousBeat = null;
            if (previousMeasure != null) {
                previousBeat = previousMeasure.Staffs[StaffIndex].Pattern.Beats.Last();
            }

            foreach (BeatPattern beat in Beats) {
                count += beat.GetAttackCount(previousBeat); 
                previousBeat = beat;
            }

            return count;
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

        // Il faut transmettre l'eventuelle mesure précédente pour gérer les liaisons correctement
        public bool IsCompatibleWithNoteCount(MeasureMelody measureMelody,  MeasureData? precedingMeasure) {
            
            int noteCount = measureMelody.MelodyChords.Count();
            int graceNoteCount = 0; //TODO : cas des appogiatures  

            int totalNotes = noteCount + graceNoteCount;

            return totalNotes == GetAttackCount(precedingMeasure);
        }

        // Il faut transmettre l'eventuelle mesure précédente pour gérer les liaisons correctement
        public bool HasValidCadency(MeasureMelody measureMelody, MeasureData? precedingMeasure) {

            if (!IsDurationValid()) {
                return false;
            }

            if (GetAttackCount(precedingMeasure) != measureMelody.MelodyChords.Count) {
                return false;
            }
            
            return true;
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

        private string ElementToString(IRhythmElement rhythmElement) {

            string s = string.Empty;

            if (rhythmElement.GetType() == typeof(RhythmTuplet)) {

                RhythmTuplet tuplet = (RhythmTuplet)rhythmElement;

                s += "<";
                foreach (RhythmSimpleElement subdivision in tuplet.Subdivisions) {
                    if (subdivision.IsRest()) {
                        s += "r";
                    }
                    s += DurationToSymbol(subdivision.GetEffectiveDuration()) + " ";
                }
                s += ">";

            } else if (rhythmElement.GetType() == typeof(RhythmSimpleElement)) {

                RhythmSimpleElement rhythmSimpleElement = (RhythmSimpleElement)rhythmElement;
                if (rhythmSimpleElement.IsRest()) {
                    s += "r" + s;
                }
                s += DurationToSymbol(rhythmSimpleElement.GetEffectiveDuration());

            } else if (rhythmElement.GetType() == typeof(RhythmTieFrom)) {
                s += "~";
            }

            return s;
        }

        public override string ToString() {
            List<string> beatStrings = new();

            foreach (BeatPattern beat in Beats) {
                List<string> elements = new();

                foreach (IRhythmElement e in beat.Elements) {
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
                List<IRhythmElement> elements = new() {
                    new RhythmSimpleElement(
                        beatDuration,
                        true
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

            MeasureRhythmPatternDto measureRhythmPatternDto = new(StaffIndex, beatDtos, TimeSignature);

            return measureRhythmPatternDto;
        }
       
        public static MeasureRhythmPattern FromDto(MeasureRhythmPatternDto dto) {

            MeasureRhythmPattern pattern = new(dto.StaffIndex, dto.TimeSignature) {};

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

        internal bool EndsWithTie() {
            
            if (Beats.Count == 0) {
                return false;
            }
            BeatPattern lastBeat = Beats.Last();
            if (lastBeat.Elements.Count == 0) {
                return false;
            }
            IRhythmElement lastElement = lastBeat.Elements.Last();
            return lastElement is RhythmTieFrom;
        }
    }
}