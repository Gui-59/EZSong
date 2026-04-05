using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    public class ChordSequence {

        public List<Chord> Chords { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public ChordSequence() { 
            Chords = new List<Chord>();
        }

        public ChordSequence(List<Chord> chords) {
            Chords = chords;
        }

        public ChordSequence(string chordListGuiString) {
            Chords = new();
            foreach (string guiChord in chordListGuiString.Split(" ")) {
                Chords.Add(new Chord(guiChord));
            }
        }

        public string ToLilyPondString() {
            string lilyPondString = "";
            foreach (Chord chord in Chords) {
                lilyPondString += chord.ToLilyPondString() + " ";
            }
            return lilyPondString.Trim();
        }

        public string ToGuiString() {
            string guiString = "";
            foreach (Chord chord in Chords) {
                guiString += chord.ToGuiString() + " ";
            }
            return guiString.Trim();
        }

        public ChordSequenceDto ToDto() {
            List<ChordDto> chordDtos = new();
            foreach (Chord chord in Chords) {
                chordDtos.Add(chord.ToDto());
            }
            ChordSequenceDto dto = new() {
                Chords = chordDtos
            };
            return dto;
        }

        public static ChordSequence FromDto(ChordSequenceDto chords) {
            List<Chord> chordList = new();
            foreach (ChordDto chordDto in chords.Chords) {
                chordList.Add(Chord.FromDto(chordDto));
            }
            return new ChordSequence (chordList);

        }
    }
}
