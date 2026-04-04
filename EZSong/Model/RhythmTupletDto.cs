

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */
    public class RhythmTupletDto {
        public int Count {
            get; set;
        }
        public int InTimeOf {
            get; set;
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public RhythmTupletDto() {
        }

        public static RhythmTuplet FromDto(RhythmTupletDto tuplet) {
            if (tuplet == null) {
                return new RhythmTuplet(1, 1);
            }
            return new RhythmTuplet(tuplet.Count, tuplet.InTimeOf);

        }

        internal static RhythmTupletDto ToDto(RhythmTuplet tuplet) {
            if (tuplet == null) {
                return new RhythmTupletDto() {
                        Count = 1,
                        InTimeOf = 1
                };
            }
            return new RhythmTupletDto() {
                Count = tuplet.Count,
                InTimeOf = tuplet.InTimeOf
            };
        }
    }
}