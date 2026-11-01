using EZSong.Enums;
using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Exporting.Lilypond {

    /// <summary>
    /// A class that represents a variable in LilyPond notation, 
    /// associated with a specific song layer (chords, melody, or lyrics) and coordinates within the song structure.
    /// </summary>
    internal class LilyPondVar {

        /// <summary>
        /// Layer of the song that this variable is associated with (e.g., chords, melody, lyrics).
        /// </summary>
        private SongLayer _layer;
        public SongLayer Layer {
            get {
                return _layer;
            }
        }

        /// <summary>
        /// Coordinates of the song element that this variable represents, including segment and staff indices.
        /// </summary>
        private SongElementCoordinates _coord;
        public SongElementCoordinates Coord {
            get {
                return _coord;
            }
        }

        /// <summary>
        /// Name of the variable in LilyPond notation, generated based on the layer and coordinates.
        /// </summary>
        private string _name;
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Value of the variable in LilyPond notation, which can be set and retrieved as needed.
        /// </summary>
        private string _value;
        public string Value {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }

        /// <summary>
        /// Constructs a new instance of the LilyPondVar class, 
        /// initializing it with the specified song layer and coordinates, 
        /// and generating a unique variable name based on these parameters.
        /// </summary>
        /// <param name="layer">The layer of the song that this variable is associated with (e.g., chords, melody, lyrics).</param>
        /// <param name="songElementCoordinates">The coordinates of the song element that this variable represents, including segment and staff indices.</param>
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
