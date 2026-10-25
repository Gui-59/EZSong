using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Helpers {
    public static class MathHelper {

        /// <summary>
        /// Calculates the index within a circular range after applying an offset.
        /// </summary>
        /// <param name="index">The initial index to adjust.</param>
        /// <param name="count">The total number of elements in the circular range.</param>
        /// <param name="offset">The offset to apply to the index.</param>
        /// <returns>The adjusted index within the range [0, count).</returns>
        /// <exception cref="ArgumentException">Thrown when count is less than or equal to zero.</exception>
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
