using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Serializable {

    [Serializable]
    public class TimeSignature {

        public int Upper;
        public int Lower;

        public TimeSignature() {
            // Constructeur par défaut (requis pour la sérialisation)
        }

        public TimeSignature(int upper, int lower) {
            Upper = upper;
            Lower = lower;
        }

        public string ToLilyPondString() {
            return Upper + "/" + Lower;
        }
    }
}
