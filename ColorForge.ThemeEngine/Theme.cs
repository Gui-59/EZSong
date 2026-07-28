using ColorForge.ThemeEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {

    /// <summary>
    /// Theme
    /// </summary>
    public sealed record Theme {

        /// <summary>
        /// Mode
        /// </summary>
        public required ThemeMode Mode {
            get; init;
        }

        /// <summary>
        /// Surface
        /// </summary>
        public required SurfaceColors Surface {
            get; init;
        }

        /// <summary>
        /// Text
        /// </summary>
        public required TextColors Text {
            get; init;
        }

        /// <summary>
        /// Accent
        /// </summary>
        public required AccentColors Accent {
            get; init;
        }

        /// <summary>
        /// Button
        /// </summary>
        public required ButtonColors Button {
            get; init;
        }

        /// <summary>
        /// Semantic
        /// </summary>
        public required SemanticColors Semantic {
            get; init;
        }
    }
}
