namespace ColorForge.ThemeEngine;

/// <summary>
/// Defines generic control behavior.
/// </summary>
public sealed record ControlRecipe {

    /// <summary>
    /// BorderContrast
    /// </summary>
    public required double BorderContrast {
        get; init;
    }

    /// <summary>
    /// FocusBoost
    /// </summary>
    public required double FocusBoost {
        get; init;
    }

    /// <summary>
    /// DisabledOpacity
    /// </summary>
    public required double DisabledOpacity {
        get; init;
    }
}