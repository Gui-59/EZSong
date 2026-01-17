using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Serializable {

    [Serializable]
    public class CadencyElement {
        public NoteDuration Duration;
        public bool IsRest;

        public CadencyElement() {
            //Contructeur par défaut (requis pour la sérialisation)
            Duration = new();
        }

        public CadencyElement(NoteDuration duration, bool isRest) {
            Duration = duration;
            IsRest = isRest;
        }
    }
}
