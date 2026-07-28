using ColorForge.Core;
using ColorForge.ThemeEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {
    internal sealed class ThemeContext {
        public required ThemeMode Mode {
            get; init;
        }

        public required ThemeRecipe Recipe {
            get; init;
        }

        public required OklchColor Accent {
            get; init;
        }

        public required TonalPalette AccentPalette {
            get; init;
        }

        public required TonalPalette NeutralPalette {
            get; init;
        }

        public required TonalPalette NeutralVariantPalette {
            get; init;
        }
    }
}
