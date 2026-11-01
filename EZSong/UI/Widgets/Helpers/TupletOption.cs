using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.Helpers {

    /// <summary>
    /// A class that represents a tuplet option for UI rendering, 
    /// allowing users to select and create tuplets in the musical notation interface.
    /// </summary>
    public class TupletOption {


        public RhythmTuplet RhythmTuplet {
            get;
        }

        /// <summary>
        /// Creates a new instance of the TupletOption class with the specified RhythmTuplet,
        /// </summary>
        /// <param name="rhythmTuplet">The RhythmTuplet to associate with this option.</param>
        public TupletOption(RhythmTuplet rhythmTuplet) {
            RhythmTuplet = rhythmTuplet;
        }

        /// <summary>
        /// Creates a new instance of the associated RhythmTuplet.
        /// </summary>
        /// <returns>A new instance of the associated RhythmTuplet.</returns>
        public IRhythmElement Create() {
            return new RhythmTuplet();
        }

    }
}
