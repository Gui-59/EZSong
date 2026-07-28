using ColorForge.Core;

namespace ColorForge.ThemeEngine;

/// <summary>
/// Defines application surface colors.
/// </summary>
public sealed record SurfaceColors {

    /// <summary>
    /// Background
    /// </summary>
    public required ThemeColor Background {
        get; init;
    }

    /// <summary>
    /// Layer1
    /// </summary>
    public required ThemeColor Layer1 {
        get; init;
    }

    /// <summary>
    /// Layer2
    /// </summary>
    public required ThemeColor Layer2 {
        get; init;
    }

    /// <summary>
    /// Layer3
    /// </summary>
    public required ThemeColor Layer3 {
        get; init;
    }

    /// <summary>
    /// Border
    /// </summary>
    public required ThemeColor Border {
        get; init;
    }

    /// <summary>
    /// Separator
    /// </summary>
    public required ThemeColor Separator {
        get; init;
    }
}