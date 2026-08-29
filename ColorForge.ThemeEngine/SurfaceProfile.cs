namespace ColorForge.ThemeEngine;

/// <summary>
/// Defines the visual characteristics used to generate application surfaces.
/// </summary>
public sealed record SurfaceProfile {
    /// <summary>
    /// Relative separation between the main application surface levels.
    /// </summary>
    public required double Contrast {
        get; init;
    }

    /// <summary>
    /// Amount of accent tint applied to neutral surfaces.
    /// A value of zero produces neutral grays.
    /// </summary>
    public required double TintStrength {
        get; init;
    }

    /// <summary>
    /// Visual prominence of elevated surfaces.
    /// </summary>
    public required double Elevation {
        get; init;
    }

    /// <summary>
    /// Creates a neutral surface profile.
    /// </summary>
    public static SurfaceProfile Neutral {
        get {
            return new() {
                Contrast = 1.0,
                TintStrength = 0.0,
                Elevation = 1.0
            };
        }
    }
}