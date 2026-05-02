using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public interface IRhythmElement {
        RhythmRationalDuration GetEffectiveDuration();

        int DotCount();

        bool IsTiedToNext();

        bool IsRest();
    }
}
