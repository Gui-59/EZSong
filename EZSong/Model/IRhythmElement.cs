using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EZSong.Model {

    //Un élement de rythme peut être
    // - une note simple (noire, croche, etc.) éventuellement pointée
    // - une silence éventuellement pointée
    // - un tuplet
    // - une liaison,
    // - un symbole de début/fin de phrase,
    // ...
    public interface IRhythmElement {
    }
}
