

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */

    public class RhythmTupletDto:RhythmSimpleElementDto,IRhythmElementDto {

        public List<RhythmSimpleElement> Subdivisions {
            get; set; //Set nécessaire pour la sérialisation JSON
        }

        public RhythmRationalDuration GlobalDuration {
            get; set; //Set nécessaire pour la sérialisation JSON
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public RhythmTupletDto() {
            Subdivisions = new List<RhythmSimpleElement>();
            GlobalDuration = new RhythmRationalDuration(1, 1, 0);
        }
        
        public RhythmTupletDto(List<RhythmSimpleElement> subdivisions, RhythmRationalDuration globalDuration) {
                Subdivisions = subdivisions;
                GlobalDuration = globalDuration;
        }

        public static RhythmTuplet FromDto(RhythmTupletDto tuplet) {
            if (tuplet == null) {
                return new RhythmTuplet();
            }
            return new RhythmTuplet(tuplet.Subdivisions, tuplet.GlobalDuration);
        }

        internal static RhythmTupletDto ToDto(RhythmTuplet tuplet) {
            if (tuplet == null) {
                return new RhythmTupletDto(new List<RhythmSimpleElement>(), new RhythmRationalDuration(1, 1, 0));
            }
            return new RhythmTupletDto(tuplet.Subdivisions, tuplet.GetEffectiveDuration());
        }
    }
}