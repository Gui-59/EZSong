namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual profile inspired by modern Fluent-style interfaces.
/// </summary>
public sealed class FluentProfile : ThemeProfile {
    public FluentProfile()
        : base("Fluent") {
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