using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public struct RhythmElementNeighborhood {

        //Structure qui représente le voisinage d'un élément rythmique (élément précédent et élément suivant)
        //Cette structure pourra être utilisée pour déterminer si un élément rythmique est lié à l'élément précédent ou à l'élément suivant, ou s'il est isolé
        //Cela permettra de gérer les liaisons entre les éléments rythmiques (par exemple, une liaison entre deux croches, ou une liaison entre une croche et une noire)
        //Cela permettra également de gérer les liaisons entre les éléments rythmiques et les silences (par exemple, une liaison entre une croche et un silence de croche)

        public List<BeatPattern> PrecedingBeats = new();
        public List<BeatPattern> FollowingBeats = new();

        public RhythmElementNeighborhood() {

        }
    }
}
