using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    /*
     * Un DTO compatible JSON doit être :
     * - public class
     * - constructeur vide
     * - propriétés publiques get/set
     * - aucune logique
     */


    /// <summary>
    /// A data transfer object (DTO) that represents a musical time signature,
    /// </summary>
    public class TimeSignatureDto {

        /// <summary>
        /// Beats per measure in the time signature, indicating how many beats are in each measure.
        /// </summary>
        public int Beats {
            get; 
            set;
        }

        /// <summary>
        /// Beat unit in the time signature, indicating the note value that represents one beat (e.g., 4 for quarter notes, 8 for eighth notes).
        /// </summary>
        public int BeatUnit {
            get; 
            set;
        }

        /// <summary>
        /// Creates a new instance of the TimeSignatureDto class with default values.
        /// Note : This constructor is required for JSON serialization/deserialization.
        /// </summary>
        public TimeSignatureDto() {
        }

        /// <summary>
        /// Creates a new instance of the TimeSignatureDto class with the specified beats and beat unit.
        /// </summary>
        /// <param name="beats">Beats per measure in the time signature.</param>
        /// <param name="beatUnit">Beat unit in the time signature.</param>
        public TimeSignatureDto(int beats, int beatUnit) {
            Beats = beats;
            BeatUnit = beatUnit;
        }
    }
}
