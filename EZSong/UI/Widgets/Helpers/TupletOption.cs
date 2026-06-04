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

        public TupletOption(RhythmTuplet rhythmTuplet) { //TODO : gérer la notion de durée globale du tuplet (ex: 3 croches dans le temps de 2)
            RhythmTuplet = rhythmTuplet;
        }

        public IRhythmElement Create() {

            return new RhythmTuplet();

        }

    }
}
