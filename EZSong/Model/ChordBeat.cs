using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EZSong.Model {
    public class ChordBeat {

        private RhythmRationalDuration _beatDuration;

        public List<Chord> Chords {
            get;
            set;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public ChordBeat() {
            Chords = new List<Chord>();
        }

        public ChordBeat(RhythmRationalDuration beatDuration) {
            _beatDuration = beatDuration;
            Chords = new List<Chord>();
        }

        public ChordBeat(List<Chord> chords) {
            Chords = chords;
        }

        public RhythmRationalDuration GetTotalDuration() {

            RhythmRationalDuration total = new(0, 1, 0);

            foreach (Chord c in Chords) {
                total += c.Duration;
            }

            return total;
        }

        internal static ChordBeat FromDto(ChordBeatDto chordBeat) {

            List<Chord> chords = new();
            foreach (ChordDto chord in chordBeat.Chords) {
                if (chord is not null) {
                    chords.Add(Chord.FromDto(chord));
                }
            }

            return new ChordBeat(
                chords
            );

        }

        internal ChordBeatDto ToDto() {
            List<ChordDto> chords = new();
            foreach (Chord chord in Chords) {
                chords.Add(chord.ToDto());
                
            }
            return new ChordBeatDto() {
                Chords = chords
            };
        }

        internal void Clear() {
            Chords.Clear();
        }

        internal RhythmRationalDuration GetRemainingDuration() {
            return _beatDuration - GetTotalDuration();
        }

        internal bool CanAdd(Chord chord) {
            if (chord.Duration.Numerator <= 0) {
                return false;
            }
            RhythmRationalDuration remaining = _beatDuration - GetTotalDuration();
            return (remaining - chord.Duration).Numerator >= 0;
        }

        internal string ToLilyPondString() {
            String lilyPondString = "";
            foreach (Chord chord in Chords) {
                if (!chord.IsSilentChord) {
                    lilyPondString += chord.ToLilyPondString()+" ";
                }
            }
            return lilyPondString;
        }

        internal string ToHumanString() {
            string humanString = "";
            foreach (Chord chord in Chords) {
                humanString += chord.ToHumanString() + " & "; //TODO : trouver mieux comme séparateur
            }
            return humanString.Trim(); //TODO: remove the last " & " at the end of the string
        }

        internal void AddChord(Chord chord) {
            if (CanAdd(chord)) {
                Chords.Add(chord);
            }
        }
    }
}
