using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.Core {

    /// <summary>
    /// StateColors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed record StateColors<T> {

        /// <summary>
        /// Normal
        /// </summary>
        public required T Normal {
            get; init;
        }

        /// <summary>
        /// Hover
        /// </summary>
        public required T Hover {
            get; init;
        }

        /// <summary>
        /// Pressed
        /// </summary>
        public required T Pressed {
            get; init;
        }

        /// <summary>
        /// Disabled
        /// </summary>
        public required T Disabled {
            get; init;
        }

        /// <summary>
        /// Focused
        /// </summary>
        public required T Focused {
            get; init;
        }
    }
}
