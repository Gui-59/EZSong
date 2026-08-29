using ColorForge.Core;

namespace ColorForge.ThemeEngine;

/// <summary>
/// Accent colors generated from the application's identity color.
/// </summary>
public sealed record AccentTokens {
    public required ThemeColor Fill {
        get; init;
    }

    public required ThemeColor Hover {
        get; init;
    }

    public required ThemeColor Pressed {
        get; init;
    }

    public required ThemeColor Selected {
        get; init;
    }

    public required ThemeColor Subtle {
        get; init;
    }

    public required ThemeColor Text {
        get; init;
    }
}