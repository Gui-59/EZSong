using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Exporting.Lilypond {
    internal class LilyPondVar {

        private string _name;
        public string Name {
            get {
                return _name;
            }
        }

        private string _value;
        public string Value {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }

        public LilyPondVar(string varNamePrefix, SongElementCoordinates songElementCoordinates) {
            //Le nom de variable Lilypond ne peut pas terminer par un chiffre ; on met la lettre équivalente (via la table ASCII)

            if (songElementCoordinates.StaffIndex == null) {
                _name = varNamePrefix + Convert.ToChar(65 + songElementCoordinates.SegmentIndex);
            } else {
                _name = varNamePrefix + Convert.ToChar(65 + songElementCoordinates.SegmentIndex) + "_" + Convert.ToChar(65 + songElementCoordinates.StaffIndex);
            }
            _value = string.Empty;
        }
    }
}
