using ColorForge.Core;

namespace ColorForge.ThemeEngine;

/// <summary>
/// Complete semantic color definition.
/// </summary>
public sealed record SemanticColor {
    public required ThemeColor Background {
        get; init;
    }

    public required ThemeColor Surface {
        get; init;
    }

    public required ThemeColor Border {
        get; init;
    }

    public required ThemeColor Text {
        get; init;
    }

    public required ThemeColor Icon {
        get; init;
    }
}