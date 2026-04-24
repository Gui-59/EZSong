using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.Helpers {
    public class TupletOption {
        public string Label;
        public int InTimeOf;
        public int Count;

        public TupletOption(string label, int count, int intTimeOf) { //TODO : gérer la notion de durée globale du tuplet (ex: 3 croches dans le temps de 2)
            Label = label;

            Count = count;
            InTimeOf = intTimeOf;
        }

        public RhythmElement Create() {

            return new RhythmElement(new RhythmRationalDuration(1, Count, 0), false, new RhythmTuplet());

        }
    }
}
