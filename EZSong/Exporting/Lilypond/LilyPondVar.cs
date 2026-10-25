using EZSong.Enums;
using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Exporting.Lilypond {
    internal class LilyPondVar {

        private SongLayer _layer;
        public SongLayer Layer {
            get {
                return _layer;
            }
        }

        private SongElementCoordinates _coord;
        public SongElementCoordinates Coord {
            get {
                return _coord;
            }
        }

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

        public LilyPondVar(SongLayer layer, SongElementCoordinates songElementCoordinates) {
            
            _layer = layer;   
            _coord = songElementCoordinates;

            string varNamePrefix = string.Empty;
            switch (layer) {
                case SongLayer.Chords:
                    varNamePrefix = "chord_";
                    break;
                case SongLayer.Melody:
                    varNamePrefix = "melody_";
                    break;
                case SongLayer.Lyrics:
                    varNamePrefix = "lyrics_";
                    break;
            }

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
