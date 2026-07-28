using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.Core {

    /// <summary>
    /// SemanticColors
    /// </summary>
    public sealed record SemanticColors {

        /// <summary>
        /// Success
        /// </summary>
        public required SemanticEntry Success {
            get; init;
        }

        /// <summary>
        /// Warning
        /// </summary>
        public required SemanticEntry Warning {
            get; init;
        }

        /// <summary>
        /// Error
        /// </summary>
        public required SemanticEntry Error {
            get; init;
        }

        /// <summary>
        /// Information
        /// </summary>
        public required SemanticEntry Information {
            get; init;
        }
    }
}
