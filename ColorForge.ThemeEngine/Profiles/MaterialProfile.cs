namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual profile inspired by Material-style design systems.
/// </summary>
public sealed class MaterialProfile : ThemeProfile {
    public MaterialProfile()
        : base("Material") {
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