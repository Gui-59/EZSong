namespace ColorForge.ThemeEngine;

/// <summary>
/// Defines semantic colors.
/// </summary>
public sealed record SemanticRecipe {

    /// <summary>
    /// SuccessHue
    /// </summary>
    public required double SuccessHue {
        get; init;
    }

    /// <summary>
    /// WarningHue
    /// </summary>
    public required double WarningHue {
        get; init;
    }

    /// <summary>
    /// ErrorHue
    /// </summary>
    public required double ErrorHue {
        get; init;
    }

    /// <summary>
    /// InfoHue
    /// </summary>
    public required double InfoHue {
        get; init;
    }

    /// <summary>
    /// Chroma
    /// </summary>
    public required double Chroma {
        get; init;
    }
}