using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {
    /// <summary>
    /// ButtonColors
    /// </summary>
    public sealed record ButtonColors {

        /// <summary>
        /// Normal
        /// </summary>
        public required ButtonState Normal {
            get; init;
        }

        /// <summary>
        /// Hover
        /// </summary>
        public required ButtonState Hover {
            get; init;
        }

        /// <summary>
        /// Pressed
        /// </summary>
        public required ButtonState Pressed {
            get; init;
        }

        /// <summary>
        /// Disabled
        /// </summary>
        public required ButtonState Disabled {
            get; init;
        }

        /// <summary>
        /// Focused
        /// </summary>
        public required ButtonState Focused {
            get; init;
        }
    }
}
