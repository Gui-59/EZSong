namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */
    public class MelodyChordDto {
        public List<PitchDto> Pitches { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public MelodyChordDto() {
            Pitches = new List<PitchDto>();
        }
    }
}