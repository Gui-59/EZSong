using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    public class ChordSequence {

        public List<ChordBeat> ChordBeats { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public ChordSequence() { 
            ChordBeats = new List<ChordBeat>();
            InitializeFromTimeSignature(new TimeSignature());
        }

        public ChordSequence(List<ChordBeat> chordBeats) {
            ChordBeats = new();
            foreach (ChordBeat chordBeat in chordBeats) {
                ChordBeats.Add(chordBeat);
            }
        }

        public void InitializeFromTimeSignature(TimeSignature ts) {
            ChordBeats.Clear();

            int beatCount = ts.GetBeatCount();
            RhythmRationalDuration beatDuration = ts.GetBeatDuration();

            for (int i = 0; i < beatCount; i++) {
                ChordBeats.Add(new ChordBeat(beatDuration));
            }
        }

        public string ToLilyPondString() {
            string lilyPondString = "";
            foreach (ChordBeat chordBeat in ChordBeats) {
                lilyPondString += chordBeat.ToLilyPondString() + " & "; //TODO : trouver un meilleur séparateur que " & " pour séparer les accords dans la séquence
            }
            return lilyPondString.Trim(); //TODO : enlever le dernier " & " de la chaîne
        }

        public ChordSequenceDto ToDto() {
            List<ChordBeatDto> chordBeats = new();
            foreach (ChordBeat chordBeat in ChordBeats) {
                chordBeats.Add(chordBeat.ToDto());
            }
            ChordSequenceDto dto = new() {
                ChordBeats = chordBeats
            };
            return dto;
        }

        public static ChordSequence FromDto(ChordSequenceDto dto) {
            List<ChordBeat> chordBeats = new();
            foreach (ChordBeatDto chordBeatDto in dto.ChordBeats) {
                chordBeats.Add(ChordBeat.FromDto(chordBeatDto));
            }
            return new ChordSequence (chordBeats);

        }

        internal int GetBeatCount() {
            return ChordBeats.Count;    
        }
    }
}
