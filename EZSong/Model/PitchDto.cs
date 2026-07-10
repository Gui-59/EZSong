using EZSong.Enums;

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */

    public class PitchDto {

        public NoteStep Note {
            get; 
            set;
        }

        public Alteration Alteration {
            get; 
            set;
        }

        public int MidiOctave {
            get; 
            set;
        }

        //Constructeur vide (requis pour la sérialisation JSON)
        public PitchDto() {
        }

        public PitchDto(NoteStep note, Alteration alteration, int midiOctave) {
            Note = note;
            Alteration = alteration;
            MidiOctave = midiOctave;
        }
    }
}