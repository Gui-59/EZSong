using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {
    public static class ThemeProfiles {
        public static ThemeProfile Fluent {
            get;
        }
            = new FluentProfile();

        public static ThemeProfile Studio {
            get;
        }
            = new StudioProfile();

        public static ThemeProfile Material {
            get;
        }
            = new MaterialProfile();

        public static ThemeProfile Classic {
            get;
        }
            = new ClassicProfile();

        public static ThemeProfile HighContrast {
            get;
        }
            = new HighContrastProfile();
    }
}
