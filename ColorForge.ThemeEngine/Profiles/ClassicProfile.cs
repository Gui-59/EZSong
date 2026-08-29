namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual profile designed for a restrained and traditional desktop appearance.
/// </summary>
public sealed class ClassicProfile : ThemeProfile {
    public ClassicProfile()
        : base("Classic") {
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