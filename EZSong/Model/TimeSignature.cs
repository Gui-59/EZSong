using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    /// <summary>
    /// A class that represents a musical time signature, 
    /// which defines the number of beats in a measure 
    /// and the note value that represents one beat.
    /// </summary>
    public class TimeSignature {

        /// <summary>
        /// Beats per measure, indicating how many beats are contained in each measure of music.
        /// </summary>
        public int Beats {
            get; 
            set;
        }

        /// <summary>
        /// Beat unit, indicating the note value that represents one beat (e.g., 4 for quarter notes, 8 for eighth notes).
        /// </summary>
        public int BeatUnit {
            get; 
            set;
        }

        /// <summary>
        /// Computes the expected duration of a single beat based on the time signature,
        /// </summary>
        public RhythmRationalDuration ExpectedBeatDuration {

            get {
                // signature composée
                if (Beats % 3 == 0 && BeatUnit == 8 && Beats > 3) {
                    // noire pointée = 3 croches
                    return new RhythmRationalDuration(3, 8, 0);
                }
                // cas simple
                return new RhythmRationalDuration(1, BeatUnit, 0);
            }
        }

        /// <summary>
        /// Constructs a new instance of the TimeSignature class with default values (4/4 time signature).
        /// Note : This constructor is required for JSON serialization/deserialization.
        /// </summary>
        public TimeSignature() {
            Beats = 4;
            BeatUnit = 4;
        }

        /// <summary>
        /// Constructs a new instance of the TimeSignature class with the specified number of beats and beat unit.
        /// </summary>
        /// <param name="beat">Beats per measure in the time signature. </param>
        /// <param name="beatUnit">Beat unit in the time signature.</param>
        public TimeSignature(int beat, int beatUnit) {
            Beats = beat;
            BeatUnit = beatUnit;
        }

        /// <summary>
        /// Computes the total duration of a measure based on the time signature,
        /// </summary>
        public RhythmRationalDuration TotalDuration {
            get {
                return new RhythmRationalDuration(Beats, BeatUnit, 0);
            }
        }

        /// <summary>
        /// Gets the number of beats in a measure, taking into account whether the time signature is simple or compound.
        /// </summary>
        /// <returns>The number of beats in a measure.</returns>
        public int GetBeatCount() {
            // Cas simple
            if (Beats % 3 == 0 && BeatUnit == 8 && Beats > 3) {
                // signature composée (6/8, 9/8, 12/8)
                return Beats / 3;
            }

            return Beats;
        }


        /// <summary>
        /// Gets the duration of a single beat in the time signature, taking into account whether the time signature is simple or compound.
        /// </summary>
        /// <returns>The duration of a single beat.</returns>
        public RhythmRationalDuration GetBeatDuration() {

            // signature composée
            if (Beats % 3 == 0 && BeatUnit == 8 && Beats > 3) {
                // noire pointée = 3 croches
                return new RhythmRationalDuration(3, 8, 0);
            }

            // cas simple
            return new RhythmRationalDuration(1, BeatUnit, 0);
        }

        /// <summary>
        /// Converts the time signature to a string representation in LilyPond format (e.g., "4/4" for a 4/4 time signature).
        /// </summary>
        /// <returns>A string representation of the time signature in LilyPond format.</returns>
        public string ToLilyPondString() {
            return Beats + "/" + BeatUnit;
        }

        /// <summary>
        /// Converts a TimeSignatureDto object to a TimeSignature object.
        /// </summary>
        /// <param name="timeSignature">The TimeSignatureDto object to convert.</param>
        /// <returns>A TimeSignature object.</returns>
        internal static TimeSignature FromDto(TimeSignatureDto timeSignature) {
            return new TimeSignature(timeSignature.Beats, timeSignature.BeatUnit);
        }


        /// <summary>
        /// Converts the current TimeSignature object to a TimeSignatureDto object.
        /// </summary>
        /// <returns>A TimeSignatureDto object.</returns>
        internal TimeSignatureDto ToDTo() {
            return new TimeSignatureDto(Beats, BeatUnit);
        }
    }
}
