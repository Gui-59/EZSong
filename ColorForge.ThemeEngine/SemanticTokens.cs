namespace ColorForge.ThemeEngine;

/// <summary>
/// Semantic colors used throughout the UI.
/// </summary>
public sealed record SemanticTokens {
    public required SemanticColor Success {
        get; init;
    }

    public required SemanticColor Warning {
        get; init;
    }

    public required SemanticColor Error {
        get; init;
    }

    public required SemanticColor Information {
        get; init;
    }
}