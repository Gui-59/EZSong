using EZSong.Enums;

namespace EZSong.Model {
    public class ChordDto {
        public bool IsSilentChord;

        public RhythmRationalDuration Duration;

        public NoteStep RootNote;
        public Alteration RootNoteAlteration;
        public ChordMode ThirdNoteMode;
        public ChordMode FithNoteMode;
        public ChordMode SeventhNoteMode;
        public ChordMode NinthNoteMode;
    }
}