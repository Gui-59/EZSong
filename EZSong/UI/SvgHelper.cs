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

            SKSvg svg = new();
            _ = svg.Load(stream);

            SKBitmap bmp = new(size, size, true);

            using (SKCanvas canvas = new(bmp)) {
                canvas.Clear(SKColors.Transparent);

                SKPicture? pic = svg.Picture;
                if (pic != null) {
                    float scale = Math.Min(
                        size / pic.CullRect.Width,
                        size / pic.CullRect.Height);

                    float x = (size - pic.CullRect.Width * scale) / 2;
                    float y = (size - pic.CullRect.Height * scale) / 2;

                    canvas.Translate(x, y);
                    canvas.Scale(scale);
                    canvas.DrawPicture(pic);
                }
            }

            byte[] data = bmp.Bytes;

            for (int i = 0; i < data.Length; i += 4) {
                byte b = data[i];
                byte g = data[i + 1];
                byte r = data[i + 2];
                byte a = data[i + 3];

                data[i] = r;
                data[i + 1] = g;
                data[i + 2] = b;
                data[i + 3] = a;
            }

            Pixbuf pixbuf = new(data, true, 8,
                bmp.Width, bmp.Height, bmp.RowBytes);

            return new Image(pixbuf);
        }

    }
}
