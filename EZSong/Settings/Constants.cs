using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Settings {

    /// <summary>
    /// Defines a static class containing constant values used throughout the EZSong application,
    /// such as base octaves for melody and bass, and default names for segments and staves.
    /// </summary>
    public static class Constants {

        /// <summary>
        /// Octave number used as the base for melody notes, typically representing the 5th octave in standard musical notation.
        /// </summary>
        public const int MelodyBaseOctave = 5;

        /// <summary>
        /// Octave number used as the base for bass notes, typically representing the 3rd octave in standard musical notation.
        /// </summary>
        public const int BassBaseOctave = 3;

        /// <summary>
        /// Default name assigned to segments that do not have a specified name, used for identification and display purposes.
        /// </summary>
        public const string DefaultSegmentName = "Unnamed Segment";

        /// <summary>
        /// Default name assigned to staves that do not have a specified name, used for identification and display purposes.
        /// </summary>
        public const string DefaultStaffName = "Unnamed Staff";
    }
}
