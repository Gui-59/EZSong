using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.Helpers {

    /// <summary>
    /// A class that represents a composite musical glyph for UI rendering,
    /// </summary>
    public class UICompositeGlyph {

        /// <summary>
        /// List of UIGlyphs that make up the composite glyph, 
        /// allowing for the representation of complex musical symbols composed of multiple simpler glyphs.
        /// </summary>
        private List<UIGlyph> Glyphs {
            get;
        }

        /// <summary>
        /// Creates a new instance of the UICompositeGlyph class, 
        /// initializing an empty list of UIGlyphs.
        /// </summary>
        public UICompositeGlyph() {
            Glyphs = new List<UIGlyph>();
        }
        
        /// <summary>
        /// Adds a UIGlyph to the composite glyph.
        /// </summary>
        /// <param name="uIGlyph">The UIGlyph to add.</param>
        internal void AddGlyph(UIGlyph uIGlyph) {
            Glyphs.Add(uIGlyph);
        }

        /// <summary>
        /// Adds all the UIGlyphs from another UICompositeGlyph to this composite glyph.
        /// </summary>
        /// <param name="compositeGlyph">The UICompositeGlyph whose glyphs are to be added.</param>
        internal void AddCompositeGlyph(UICompositeGlyph compositeGlyph) {
            foreach (UIGlyph glyph in compositeGlyph.Glyphs) {
                Glyphs.Add(glyph);
            }
        }

        /// <summary>
        /// Converts the composite glyph to a string representation by concatenating the string representations of its constituent UIGlyphs.
        /// </summary>
        /// <returns>A string representation of the composite glyph.</returns>
        public override string ToString() {
            StringBuilder sb = new();
            foreach (UIGlyph glyph in Glyphs) {
                _ = sb.Append(glyph.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates a UICompositeGlyph from a RhythmTuplet descriptor, 
        /// adding the appropriate start and end tuplet glyphs along with the glyphs for each subdivision.
        /// </summary>
        /// <param name="te">The RhythmTuplet descriptor to create a UICompositeGlyph for.</param>
        /// <returns>A UICompositeGlyph representing the tuplet.</returns>
        internal static UICompositeGlyph FromTupletDescriptor(RhythmTuplet te) {
            UICompositeGlyph uICompositeGlyph = new();
            uICompositeGlyph.AddGlyph(new UIGlyph(Enums.Glyph.tupletStart));
            foreach (RhythmSimpleElement subdivision in te.Subdivisions) {
                uICompositeGlyph.AddGlyph(UIGlyph.FromDescriptor(subdivision));
            }
            uICompositeGlyph.AddGlyph(new UIGlyph(Enums.Glyph.tupletEnd));
            return uICompositeGlyph;
        }
    }
}
