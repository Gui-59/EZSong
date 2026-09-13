using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Helpers {
    public static class MathHelper {

        public static int LoopIndex(int index, int count, int offset) {
            if (count <= 0) {
                throw new ArgumentException("Count must be greater than zero.", nameof(count));
            }
            int result = (index + offset) % count;
            if (result < 0) {
                result += count;
            }
            return result;
        }
    }
}
