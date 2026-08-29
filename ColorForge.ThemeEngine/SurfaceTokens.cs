using ColorForge.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {
    public sealed record SurfaceTokens {
        public required ThemeColor Application {
            get; init;
        }

        public required ThemeColor Workspace {
            get; init;
        }

        public required ThemeColor Panel {
            get; init;
        }

        public required ThemeColor Card {
            get; init;
        }

        public required ThemeColor Toolbar {
            get; init;
        }

        public required ThemeColor Sidebar {
            get; init;
        }

        public required ThemeColor Overlay {
            get; init;
        }

        public required ThemeColor Tooltip {
            get; init;
        }
    }
}
