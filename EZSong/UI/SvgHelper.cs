using Gtk;
using Gdk;
using SkiaSharp;
using Svg.Skia;
using Svg.Model;
using System.Reflection;

namespace EZSong.UI {

    public static class SvgHelper {

        public static Image LoadSvgAsGtkImage(string resourceName, int size) {
            Assembly asm = Assembly.GetExecutingAssembly();

            using Stream? stream = asm.GetManifestResourceStream(resourceName);
            if (stream == null) {
                throw new Exception($"Ressource SVG introuvable : {resourceName}");
            }

            // Charger le SVG dans un modèle
            SKSvg svg = new();
            _ = svg.Load(stream);

            // Rendu final en SKBitmap
            SKBitmap bmp = new(size, size, true);
            using (SKCanvas canvas = new(bmp)) {
                canvas.Clear(SKColors.Transparent);
                SKPicture? pic = svg.Picture;

                if (pic != null) {
                    float scaleX = size / pic.CullRect.Width;
                    float scaleY = size / pic.CullRect.Height;
                    float scale = Math.Min(scaleX, scaleY);

                    canvas.Scale(scale);
                    canvas.DrawPicture(pic);
                }
            }

            // Conversion en Pixbuf pour Gtk
            byte[] data = bmp.Bytes;
            Pixbuf pixbuf = new(data, true, 8,
                                    bmp.Width, bmp.Height, bmp.RowBytes);

            return new Image(pixbuf);
        }

    }
}
