using ColorForge.Core;

namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual colors used to render an interactive control.
/// </summary>
public sealed record ControlState {
    /// <summary>
    /// Background fill.
    /// </summary>
    public required ThemeColor Fill {
        get; init;
    }

    /// <summary>
    /// Border color.
    /// </summary>
    public required ThemeColor Border {
        get; init;
    }

    /// <summary>
    /// Foreground text.
    /// </summary>
    public required ThemeColor Text {
        get; init;
    }

    /// <summary>
    /// Icon color.
    /// </summary>
    public required ThemeColor Icon {
        get; init;
    }
}