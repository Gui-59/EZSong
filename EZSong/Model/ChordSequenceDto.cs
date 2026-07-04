namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */
    public class ChordSequenceDto {
        public List<ChordBeatDto> ChordBeats { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public ChordSequenceDto() {
            ChordBeats = new List<ChordBeatDto>();
        }
    }
}