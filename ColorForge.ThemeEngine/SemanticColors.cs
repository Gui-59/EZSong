using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {

    /// <summary>
    /// SemanticColors
    /// </summary>
    public sealed record SemanticColors {

        /// <summary>
        /// Success
        /// </summary>
        public required SemanticState Success {
            get; init;
        }

        /// <summary>
        /// Warning
        /// </summary>
        public required SemanticState Warning {
            get; init;
        }

        /// <summary>
        /// Error
        /// </summary>
        public required SemanticState Error {
            get; init;
        }

        /// <summary>
        /// Information
        /// </summary>
        public required SemanticState Information {
            get; init;
        }
    }
}
