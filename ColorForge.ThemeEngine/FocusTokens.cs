using ColorForge.Core;

namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual definition of keyboard focus.
/// </summary>
public sealed record FocusTokens {
    public required ThemeColor Color {
        get; init;
    }

    public required double Thickness {
        get; init;
    }

    public required double Offset {
        get; init;
    }
}