namespace ColorForge.ThemeEngine;

/// <summary>
/// Defines the visual characteristics used to generate semantic colors.
/// </summary>
public sealed record SemanticProfile {
    /// <summary>
    /// Saturation multiplier applied to semantic colors.
    /// </summary>
    public required double Saturation {
        get; init;
    }

    /// <summary>
    /// Strength of semantic backgrounds.
    /// </summary>
    public required double BackgroundStrength {
        get; init;
    }

    /// <summary>
    /// Strength of semantic borders.
    /// </summary>
    public required double BorderStrength {
        get; init;
    }

    /// <summary>
    /// Relative prominence of semantic colors.
    /// </summary>
    public required double Emphasis {
        get; init;
    }

    /// <summary>
    /// Creates a balanced semantic profile.
    /// </summary>
    public static SemanticProfile Balanced {
        get {
            return new() {
                Saturation = 1.0,
                BackgroundStrength = 1.0,
                BorderStrength = 1.0,
                Emphasis = 1.0
            };
        }
    }
}