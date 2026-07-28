using ColorForge.Core;

namespace ColorForge.ThemeEngine {

    /*
     * Note : Un accent n'est jamais "disabled". C'est le contrôle qui l'est.
     */

    /// <summary>
    /// AccentColors
    /// </summary>
    public sealed record AccentColors {

        /// <summary>
        /// Primary
        /// </summary>
        public required ThemeColor Primary {
            get; init;
        }

        /// <summary>
        /// Hover
        /// </summary>
        public required ThemeColor Hover {
            get; init;
        }

        /// <summary>
        /// Pressed
        /// </summary>
        public required ThemeColor Pressed {
            get; init;
        }

        /// <summary>
        /// Focus
        /// </summary>
        public required ThemeColor Focus {
            get; init;
        }
    }
}
