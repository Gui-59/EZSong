using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Serializable {

    [Serializable]
    public class ChordSequence {

        public List<Chord> Chords; 

        public ChordSequence() {
            // Constructeur par défaut (requis pour la sérialisation)
            Chords = new();
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
    }
}
