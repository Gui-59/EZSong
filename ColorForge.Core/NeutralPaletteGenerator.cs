using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorForge.Core {
    internal static class NeutralPaletteGenerator {

        private static readonly double[] _lightnessCurve =
        {
            0.985,
            0.965,
            0.935,
            0.890,
            0.820,
            0.720,
            0.610,
            0.500,
            0.390,
            0.270,
            0.180,
            0.120,
            0.070,
            0.030
        };

        public static TonalPalette Generate(
            ThemeContext context) {

            double hue = context.Accent.H;

            double chroma =
                Math.Min(
                    context.Accent.C * 0.08,
                    context.Recipe.Surface.Chroma);

            double lightness =
                context.Mode == ThemeMode.Light
                    ? _lightnessCurve[i]
                    : 1.0 - _lightnessCurve[i];

            OklchColor color =
                new(
                    lightness,
                    chroma,
                    hue);

            palette.Add(
                ColorConverter.FromOklch(color));
        }
    }
}
