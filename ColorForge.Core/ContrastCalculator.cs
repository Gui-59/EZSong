using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorForge.Core;

namespace ColorForge.Core {

    //TODO : A remettre en place plus tard ?
    /*

    /// <summary>
    /// ContrastCalculator
    /// </summary>
    public static class ContrastCalculator {

        /// <summary>
        /// BestForeground
        /// </summary>
        /// <param name="background"></param>
        /// <returns></returns>
        public static ThemeColor BestForeground(
            ThemeColor background) {
            double white =
        ContrastRatio(
            ThemeColor.White,
            background);

            double black =
                ContrastRatio(
                    ThemeColor.Black,
                    background);

            return white > black
                ? ThemeColor.White
                : ThemeColor.Black;
        }

        /// <summary>
        /// IsAccessible
        /// </summary>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static bool IsAccessible(
            ThemeColor foreground,
            ThemeColor background,
            AccessibilityLevel level) {
            throw new NotImplementedException(); //TODO
        }

        /// <summary>
        /// Cette formule est celle du standard sRGB.
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static double RelativeLuminance(
    LinearRgbColor rgb) {
            return
                0.2126 * rgb.R +
                0.7152 * rgb.G +
                0.0722 * rgb.B;
        }

        /// <summary>
        /// ContrastRatio
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double ContrastRatio(
    ThemeColor a,
    ThemeColor b) {
            double la =
                RelativeLuminance(
                    ColorConverter.ToLinearRgb(a));

            double lb =
                RelativeLuminance(
                    ColorConverter.ToLinearRgb(b));

            if (la < lb) {
                (la, lb) = (lb, la);
            }

            return (la + 0.05) /
                   (lb + 0.05);
        }
    }
    */
}
