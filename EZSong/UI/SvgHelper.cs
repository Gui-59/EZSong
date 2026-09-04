using Gtk;
using Gdk;
using Svg.Skia;
using Svg.Model;
using System.Reflection;
using SkiaSharp;

namespace EZSong.UI {

    public static class SvgHelper {

        public static Image LoadSvgAsGtkImage(string resourceName, int size) {
            Assembly asm = Assembly.GetExecutingAssembly();
            using Stream? stream = asm.GetManifestResourceStream(resourceName)
                ?? throw new Exception($"Ressource SVG introuvable : {resourceName}");

            Svg.Skia.SKSvg svg = new();
            _ = svg.Load(stream);

            using SKBitmap bmp = new(size, size, false);
            using (SKCanvas canvas = new(bmp)) {
                canvas.Clear(SKColors.Transparent);
                SKPicture? pic = svg.Picture;
                if (pic != null) {
                    float scale = Math.Min(size / pic.CullRect.Width, size / pic.CullRect.Height);
                    float x = (size - pic.CullRect.Width * scale) / 2;
                    float y = (size - pic.CullRect.Height * scale) / 2;
                    canvas.Translate(x, y);
                    canvas.Scale(scale);
                    canvas.DrawPicture(pic);
                }
            }

            using SKData png = bmp.Encode(SKEncodedImageFormat.Png, 100);
            byte[] pngBytes = png.ToArray();

            using Gdk.PixbufLoader loader = new();
            _ = loader.Write(pngBytes);
            _ = loader.Close();
            Gdk.Pixbuf pixbuf = loader.Pixbuf;
            return new Gtk.Image(pixbuf);
        }

    }
}
