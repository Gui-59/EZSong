using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.Helpers {
    public class UICompositeGlyph {

        private List<UIGlyph> Glyphs {
            get;
        }
        public UICompositeGlyph() {
            Glyphs = new List<UIGlyph>();
        }

        internal void AddGlyph(UIGlyph uIGlyph) {
            Glyphs.Add(uIGlyph);
        }

        internal void AddCompositeGlyph(UICompositeGlyph compositeGlyph) {
            foreach (UIGlyph glyph in compositeGlyph.Glyphs) {
                Glyphs.Add(glyph);
            }
        }

        public override string ToString() {
            StringBuilder sb = new();
            foreach (UIGlyph glyph in Glyphs) {
                _ = sb.Append(glyph.ToString());
            }
            return sb.ToString();
        }
    }
}
