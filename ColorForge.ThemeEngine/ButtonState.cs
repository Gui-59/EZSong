using ColorForge.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {

    /// <summary>
    /// ButtonState
    /// </summary>
    public sealed record ButtonState {

        /// <summary>
        /// Background
        /// </summary>
        public required ThemeColor Background {
            get; init;
        }

        /// <summary>
        /// Foreground
        /// </summary>
        public required ThemeColor Foreground {
            get; init;
        }

        /// <summary>
        /// Border
        /// </summary>
        public required ThemeColor Border {
            get; init;
        }
    }
}
