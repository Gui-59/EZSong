using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.Helpers {
    public class TupletOption {

        public RhythmTuplet RhythmTuplet {
            get;
        }

        public TupletOption(RhythmTuplet rhythmTuplet) {
            RhythmTuplet = rhythmTuplet;
        }

        public IRhythmElement Create() {

            return new RhythmTuplet();

        }

    }
}
