namespace ColorForge.ThemeEngine;

/// <summary>
/// Defines how neutral surfaces are generated.
/// </summary>
public sealed record SurfaceRecipe {
    /// <summary>
    /// Base surface lightness.
    /// </summary>
    public required double BaseLightness {
        get; init;
    }

    /// <summary>
    /// Chroma used for neutral surfaces.
    /// </summary>
    public required double Chroma {
        get; init;
    }

    /// <summary>
    /// Delta between successive surfaces.
    /// </summary>
    public required double Step {
        get; init;
    }
}