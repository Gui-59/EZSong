using ColorForge.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {

    /// <summary>
    /// TextColors
    /// </summary>
    public sealed record TextColors {

        /// <summary>
        /// Primary
        /// </summary>
        public required ThemeColor Primary {
            get; init;
        }

        /// <summary>
        /// Secondary
        /// </summary>
        public required ThemeColor Secondary {
            get; init;
        }

        /// <summary>
        /// Disabled
        /// </summary>
        public required ThemeColor Disabled {
            get; init;
        }

        /// <summary>
        /// Inverse
        /// </summary>
        public required ThemeColor Inverse {
            get; init;
        }

        /// <summary>
        /// Link
        /// </summary>
        public required ThemeColor Link {
            get; init;
        }
    }
}
