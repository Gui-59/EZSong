using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.ThemeEngine {
    public abstract class ThemeProfile {
        protected ThemeProfile(string name) {
            Name = name;
        }

        /// <summary>
        /// Gets the human-readable profile name.
        /// </summary>
        public string Name {
            get;
        }

        internal abstract SurfaceProfile Surface {
            get;
        }

        internal abstract SemanticProfile Semantic {
            get;
        }

        internal abstract ControlProfile Control {
            get;
        }

        public override string ToString() {
            return Name;
        }
    }
}
