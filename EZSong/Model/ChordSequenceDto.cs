namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */
    public class ChordSequenceDto {
        public List<ChordDto> Chords { get; set; }

        //Constructeur vide (requis pour la sérialisation JSON)
        public ChordSequenceDto() {
            Chords = new List<ChordDto>();
        }
    }
}