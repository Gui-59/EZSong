using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.Core {
    internal sealed class ColorConversionCache {
        private readonly Dictionary<ThemeColor, OklchColor> _cache = new();
    }
}
