namespace ColorForge.ThemeEngine;

/// <summary>
/// Describes the visual philosophy used to generate a theme.
/// </summary>
internal sealed record ThemeRecipe {
    internal required ThemeStyle Style {
        get; init;
    }
}