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
        public required ThemeIdentity Identity {
            get; init;
        }

        public required SurfaceTokens Surfaces {
            get; init;
        }

        public required ContentTokens Content {
            get; init;
        }

        public required AccentTokens Accent {
            get; init;
        }

        public required SemanticTokens Semantic {
            get; init;
        }

        public required ControlTokens Controls {
            get; init;
        }

        public required FocusTokens Focus {
            get; init;
        }

        public required ThemeMetrics Metrics {
            get; init;
        }
    }
}
