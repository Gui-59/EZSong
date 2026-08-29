namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual profile designed for dense professional and creative applications.
/// </summary>
public sealed class StudioProfile : ThemeProfile {
    public StudioProfile()
        : base("Studio") {
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