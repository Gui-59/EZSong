namespace ColorForge.ThemeEngine;

/// <summary>
/// Defines accent color generation.
/// </summary>
public sealed record AccentRecipe {
    /// <summary>
    /// Maximum chroma allowed.
    /// </summary>
    public required double MaxChroma {
        get; init;
    }

    /// <summary>
    /// Hover lightness offset.
    /// </summary>
    public required double HoverDelta {
        get; init;
    }

    /// <summary>
    /// Pressed lightness offset.
    /// </summary>
    public required double PressedDelta {
        get; init;
    }

    /// <summary>
    /// Disabled saturation factor.
    /// </summary>
    public required double DisabledSaturation {
        get; init;
    }
}