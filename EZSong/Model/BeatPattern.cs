

namespace EZSong.Model {

    public class BeatPattern {

        public List<IRhythmElement> Elements { 
            get; 
            set; 
        }

        public int GetAttackCount(BeatPattern? previousBeat) {
                int count = 0;
                bool previousElementWasTieFrom = false;

                if (previousBeat != null && previousBeat.EndsWithTie()) {
                    previousElementWasTieFrom = true;
                }

                foreach (IRhythmElement e in Elements) {

                    if (e.GetType() == typeof(RhythmSimpleElement)) {

                        RhythmSimpleElement rhythmSimpleElement = (RhythmSimpleElement)e;
                        if (rhythmSimpleElement.IsRest()) {
                            //Un silence n'est pas une attaque
                            continue;
                        }
                        if (!previousElementWasTieFrom) {
                            count++;
                        }
                        previousElementWasTieFrom = false;
                    } else if (e.GetType() == typeof(RhythmTuplet)) {

                        RhythmTuplet tuplet = (RhythmTuplet)e;
                        
                        if (!previousElementWasTieFrom) {
                            count += tuplet.AttackCount();
                        } else {
                            bool tupletFirstSubdivisionIsRest = tuplet.IsFirstSubdivisionRest();
                            if (tupletFirstSubdivisionIsRest) {
                                //Si la première subdivision du tuplet est un silence,
                                //alors la liaison ne fait pas que le tuplet entier soit non attaqué, car la liaison ne lie que la première subdivision du tuplet
                                count += tuplet.AttackCount();
                            } else {
                                //Si la première subdivision du tuplet n'est pas un silence,
                                //alors la liaison lie la première subdivision du tuplet à la note précédente,
                                //et donc on ne compte pas cette première subdivision comme une attaque
                                count += tuplet.AttackCount() - 1;
                            }
                        }
                        previousElementWasTieFrom = false;
                    } else if (e.GetType() == typeof(RhythmTieFrom)) {
                        //Une liaison ne correspond pas à une attaque

                        previousElementWasTieFrom = true;
                    }

                }

     
                return count;
            
        }

        private bool EndsWithTie() {
            if (Elements.Count == 0) {
                return false;
            }
            if (Elements.Last().GetType() == typeof(RhythmTieFrom)) {
                return true;
            }
            return false;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public BeatPattern() {
            Elements = new List<IRhythmElement>();          
        }

        public BeatPattern(RhythmRationalDuration beatDuration) {
            Elements = new List<IRhythmElement>();
            Elements.Add(
                new RhythmSimpleElement(
                    beatDuration,
                    true
                )
            );
        }

        public BeatPattern(List<IRhythmElement> elements) { 
            Elements = elements;      
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1, 0);

            foreach (IRhythmElement e in Elements) {
                if (e.GetType() == typeof(RhythmTuplet)) {
                    RhythmTuplet rhythmTuplet = (RhythmTuplet)e;
                    total += rhythmTuplet.GetEffectiveDuration();
                } else if (e.GetType() == typeof(RhythmSimpleElement)) {
                    RhythmSimpleElement rhythmSimpleElement = (RhythmSimpleElement)e;
                    total += rhythmSimpleElement.GetEffectiveDuration();

                }
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
                } else if (element.GetType() == typeof(RhythmTieFromDto)) {
                    elements.Add(RhythmTieFrom.FromDto());
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
                } else if (element.GetType() == typeof(RhythmTieFrom)) {
                    elements.Add(RhythmTieFrom.ToDto()); 
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

            if (element.GetType() == typeof(RhythmTuplet)) {
                RhythmTuplet rhythmTuplet = (RhythmTuplet)element;
                RhythmRationalDuration eltDuration = rhythmTuplet.GetEffectiveDuration();
                if (eltDuration.Numerator <= 0) {
                    return false;
                }
                RhythmRationalDuration remaining = beatDuration - GetTotalDuration();
                return (remaining - eltDuration).Numerator >= 0;
            } else if (element.GetType() == typeof(RhythmSimpleElement)) {
                RhythmSimpleElement rhythmSimpleElement = (RhythmSimpleElement)element;
                RhythmRationalDuration eltDuration = rhythmSimpleElement.GetEffectiveDuration();
                if (eltDuration.Numerator <= 0) {
                    return false;
                }
                RhythmRationalDuration remaining = beatDuration - GetTotalDuration();
                return (remaining - eltDuration).Numerator >= 0;
            }

            return true;

        }

        internal bool CanAddTieFrom() {
            if (Elements.Count == 0) {
                return false;
            }


            IRhythmElement lastRhythmElementElement = Elements.Last();
            if (lastRhythmElementElement.GetType() == typeof(RhythmTuplet)) {
                RhythmTuplet rhythmTuplet = (RhythmTuplet)lastRhythmElementElement;
                RhythmSimpleElement lastTupletElement = rhythmTuplet.Subdivisions.Last();
                if (lastTupletElement.IsRest()) {
                    return false;
                }
            } else if (lastRhythmElementElement.GetType() == typeof(RhythmSimpleElement)) {
                RhythmSimpleElement rhythmSimpleElement = (RhythmSimpleElement)lastRhythmElementElement;
                if (rhythmSimpleElement.IsRest()) {
                    return false;
                }
            } else {
                //Si le dernier element n'est pas une note, on ne peut pas faire de liaison à partir de celui-ci
                return false;
            }

            return true;
        }
    }

}