using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class KeySignatureDto {
        public NoteStep Note;
        public Alteration Alteration;
        public SongMode Mode;

        public KeySignatureDto(NoteStep note, Alteration alteration, SongMode mode) {
            Note = note;
            Alteration = alteration;    
            Mode = mode;    
        }
    }
}
