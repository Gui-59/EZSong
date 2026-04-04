using EZSong.Enums;

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */
    public class ChordDto {
        public bool IsSilentChord;

        public RhythmRationalDuration Duration;

        public NoteStep RootNote;
        public Alteration RootNoteAlteration;
        public ChordMode ThirdNoteMode;
        public ChordMode FithNoteMode;
        public ChordMode SeventhNoteMode;
        public ChordMode NinthNoteMode;

        //Constructeur vide (requis pour la sérialisation JSON)
        public ChordDto() {
        }
    }
}