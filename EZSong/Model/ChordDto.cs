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
        public bool IsSilentChord { 
            get; 
            set; 
        }

        public RhythmRationalDuration Duration { 
            get; 
            set; 
        }

        public NoteStep RootNote { 
            get; 
            set; 
        }

        public Alteration RootNoteAlteration { 
            get; 
            set; 
        }

        public ChordType ChordType { 
            get; 
            set; 
        }
        
        //Constructeur vide (requis pour la sérialisation JSON)
        public ChordDto() {
        }

        public ChordDto(bool isSilentChord,
                RhythmRationalDuration duration,
                NoteStep rootNote,
                Alteration rootNoteAlteration,
                ChordType chordType
        ) {
            IsSilentChord = isSilentChord; 
            Duration = duration;
            RootNote = rootNote;
            RootNoteAlteration = rootNoteAlteration;
            ChordType = chordType;
        }
    }
}