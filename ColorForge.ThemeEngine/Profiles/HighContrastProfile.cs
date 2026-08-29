namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual profile designed for strongly differentiated interface elements.
/// </summary>
public sealed class HighContrastProfile : ThemeProfile {
    public HighContrastProfile()
        : base("High Contrast") {
    }

    internal override SurfaceProfile Surface {
        get;
    }
        = SurfaceProfile.Neutral;

    internal override SemanticProfile Semantic {
        get;
    }
        = SemanticProfile.Balanced;

    internal override ControlProfile Control {
        get;
    }
        = ControlProfile.Balanced;
}