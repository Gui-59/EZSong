using ColorForge.Core;

namespace ColorForge.ThemeEngine;

/// <summary>
/// Colors used to render textual and graphical content.
/// </summary>
public sealed record ContentTokens {
    /// <summary>Main application text.</summary>
    public required ThemeColor TextPrimary {
        get; init;
    }

    /// <summary>Secondary text.</summary>
    public required ThemeColor TextSecondary {
        get; init;
    }

    /// <summary>Disabled text.</summary>
    public required ThemeColor TextDisabled {
        get; init;
    }

    /// <summary>Text displayed on dark accents.</summary>
    public required ThemeColor TextInverse {
        get; init;
    }

    /// <summary>Primary icon color.</summary>
    public required ThemeColor IconPrimary {
        get; init;
    }

    /// <summary>Secondary icon color.</summary>
    public required ThemeColor IconSecondary {
        get; init;
    }

    /// <summary>Border color.</summary>
    public required ThemeColor Border {
        get; init;
    }

    /// <summary>Divider color.</summary>
    public required ThemeColor Divider {
        get; init;
    }
}