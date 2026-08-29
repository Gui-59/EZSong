namespace ColorForge.ThemeEngine;

/// <summary>
/// Tokens used by common controls.
/// </summary>
public sealed record ControlTokens {
    public required InteractiveTokens Primary {
        get; init;
    }

    public required InteractiveTokens Secondary {
        get; init;
    }

    public required InteractiveTokens Subtle {
        get; init;
    }

    public required InteractiveTokens Destructive {
        get; init;
    }
}