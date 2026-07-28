using ColorForge.Core;

namespace ColorForge.ThemeEngine;

/// <summary>
/// Builds complete application themes.
/// </summary>
public sealed class ThemeBuilder {

    /// <summary>
    /// Build
    /// </summary>
    /// <param name="accent"></param>
    /// <param name="recipe"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public Theme Build(
        ThemeColor accent,
        ThemeRecipe recipe,
        ThemeMode mode) {
        ArgumentNullException.ThrowIfNull(recipe);

        // 1 - Base palettes

        var accentPalette =
            AccentPaletteGenerator.Generate(
                accent,
                recipe,
                mode);

        TonalPalette neutralPalette =
            NeutralPaletteGenerator.Generate(
                accent,
                recipe,
                mode);

        // 2 - Semantic colors

        var semantic =
            SemanticColorGenerator.Generate(
                accentPalette,
                recipe,
                mode);

        // 3 - Surfaces

        var surfaces =
            SurfaceGenerator.Generate(
                neutralPalette,
                recipe,
                mode);

        // 4 - Controls

        var controls =
            ControlGenerator.Generate(
                accentPalette,
                surfaces,
                recipe,
                mode);

        // 5 - Text

        var text =
            TextGenerator.Generate(
                surfaces,
                accentPalette,
                recipe,
                mode);

        return new Theme(
            surfaces,
            controls,
            semantic,
            text);
    }
}