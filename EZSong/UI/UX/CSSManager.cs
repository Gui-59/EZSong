using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.UX {
    internal class CSSManager {
        
        private ColorPaletteManager _colorPaletteManager;

        private String _css = string.Empty;
        public String Css {
            get { 
                return _css; 
            }
        }

        private Dictionary<String, Dictionary<String, String>> _cssClasses = new();


        public CSSManager() {
            _colorPaletteManager = new ColorPaletteManager();
            _css =  MakeCss();
        }

        private String ColorToCssRgb(System.Drawing.Color color) {
            return "rgb(" + color.R.ToString() + ", " + color.G.ToString() + ", " + color.B.ToString() + ")";
        }

        private String BackgroundColorAttributeToCssRgb(System.Drawing.Color color) {
            return "background-color: " + ColorToCssRgb(color);
        }

        private void ComputeBaseCss() {

            Dictionary<string, string> cssProperties = new();
            cssProperties.Add("padding", "0px");
            cssProperties.Add("margin", "0px");
            _cssClasses.Add("*", cssProperties);

            cssProperties = new();
            cssProperties.Add("background-color", ColorToCssRgb(_colorPaletteManager.WindowBg));
            cssProperties.Add("color", ColorToCssRgb(_colorPaletteManager.WindowFore));
            _cssClasses.Add(".gtk-window", cssProperties);


            cssProperties = new();
            cssProperties.Add("background-color", "white"); //TODO
            cssProperties.Add("margin-top", "6px");
            cssProperties.Add("margin-left", "7px");
            cssProperties.Add("margin-right", "7px");
            cssProperties.Add("margin-bottom", "10px");
            cssProperties.Add("padding", "2px");
            cssProperties.Add("border-radius", "5px");
            cssProperties.Add("box-shadow", "rgba(0, 0, 0, 0.16) 0px 1px 4px"); //TODO
            _cssClasses.Add("flowbox", cssProperties);
        }

        private void ComputeStandardFormsCss() {

            Dictionary<string, string> cssProperties = new();
            cssProperties.Add("padding", "4px");
            cssProperties.Add("border-radius", "4px");
            cssProperties.Add("margin", "1px");
            _cssClasses.Add("button, entry", cssProperties);

            cssProperties = new();
            cssProperties.Add("font-family", "Bravura"); //TODO : rendre la police de glyph dynamique en fonction des paramètres utilisateur
            cssProperties.Add("font-size", "20px");
            _cssClasses.Add("button.glyph", cssProperties);

            cssProperties = new();
            cssProperties.Add("font-weight", "bold");
            cssProperties.Add("padding", "4px");
            _cssClasses.Add(".titleLabel", cssProperties);

            cssProperties = new();
            cssProperties.Add("padding", "4px");
            _cssClasses.Add(".infoLabel", cssProperties);
        }

        private void ComputeStackSwitcherCss() {
            Dictionary<string, string> cssProperties = new();
            cssProperties.Add("padding", "0px");
            cssProperties.Add("margin", "0px");
            _cssClasses.Add(".stack-switcher", cssProperties);

            cssProperties = new();
            cssProperties.Add("font-weight", "bold");
            cssProperties.Add("background", "transparent");
            cssProperties.Add("color", "rgba(0, 0, 0, 1)"); //TODO
            cssProperties.Add("border", "0px");
            cssProperties.Add("border-bottom", "2px solid");
            cssProperties.Add("border-radius", "0px");
            cssProperties.Add("border-color", "transparent");
            cssProperties.Add("box-shadow", "none");
            cssProperties.Add("background-image", "none");
            cssProperties.Add("padding", "0px");
            cssProperties.Add("margin", "0px");
            _cssClasses.Add(".stack-switcher button", cssProperties);

            cssProperties = new();
            cssProperties.Add("border-color", "lightblue"); //TODO
            _cssClasses.Add(".stack-switcher button:hover", cssProperties);

            cssProperties = new();
            cssProperties.Add("border-color", "blue"); //TODO
            _cssClasses.Add(".stack-switcher button:checked", cssProperties);

            cssProperties = new();
            cssProperties.Add("font-size", "9pt");
            cssProperties.Add("font-weight", "normal");
            _cssClasses.Add(".stack-switcher button label", cssProperties);

            cssProperties = new();
            cssProperties.Add("font-weight", "bold");
            _cssClasses.Add(".stack-switcher button:checked label", cssProperties);
        }

        private String MakeCss() {
            ComputeBaseCss();
            ComputeStandardFormsCss();
            ComputeStackSwitcherCss();

            return MakeCssString();
        }

        private String MakeCssString() {

            StringBuilder stringBuilder = new();

            foreach ( (string cssClassName, Dictionary<string, string> properties) in _cssClasses) {
                _ = stringBuilder.AppendLine(cssClassName);
                _ = stringBuilder.AppendLine(@"{");
                foreach ((string propertyName, string propertyValue) in properties) {
                    _ = stringBuilder.AppendLine(propertyName);
                    _ = stringBuilder.AppendLine(":");
                    _ = stringBuilder.AppendLine(propertyValue);
                    _ = stringBuilder.AppendLine(";");
                }
                _ = stringBuilder.AppendLine(@"}");


            }
            return stringBuilder.ToString();
        }
    }
}
