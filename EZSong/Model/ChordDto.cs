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
        public ChordMode ThirdNoteMode { 
            get; 
            set; 
        }
        public ChordMode FithNoteMode { 
            get; 
            set; 
        }
        public ChordMode SeventhNoteMode { 
            get; 
            set; 
        }
        public ChordMode NinthNoteMode { 
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
                ChordMode thirdNoteMode,
                ChordMode fithNoteMode,
                ChordMode seventhNoteMode,
                ChordMode ninthNoteMode) {
            IsSilentChord = isSilentChord; 
            Duration = duration;
            RootNote = rootNote;
            RootNoteAlteration = rootNoteAlteration;
            ThirdNoteMode = thirdNoteMode;
            FithNoteMode = fithNoteMode;
            SeventhNoteMode = seventhNoteMode;  
            NinthNoteMode = ninthNoteMode;
        }
    }
}