namespace ColorForge.ThemeEngine;

/// <summary>
/// Describes the visual philosophy used to generate a theme.
/// </summary>
public sealed record ThemeRecipe {

    /// <summary>
    /// Surface
    /// </summary>
    public required SurfaceRecipe Surface {
        get; init;
    }

    /// <summary>
    /// Accent
    /// </summary>
    public required AccentRecipe Accent {
        get; init;
    }

    /// <summary>
    /// Semantic
    /// </summary>
    public required SemanticRecipe Semantic {
        get; init;
    }

    /// <summary>
    /// Controls
    /// </summary>
    public required ControlRecipe Controls {
        get; init;
    }

    /// <summary>
    /// Office365
    /// </summary>
    public static ThemeRecipe Office365 {
        get;
    } =
        new() {
            Surface = new SurfaceRecipe {
                BaseLightness = 0.96,
                Chroma = 0.010,
                Step = 0.035
            },

            Accent = new AccentRecipe {
                MaxChroma = 0.18,
                HoverDelta = 0.04,
                PressedDelta = 0.08,
                DisabledSaturation = 0.20
            },

            Semantic = new SemanticRecipe {
                SuccessHue = 145,
                WarningHue = 85,
                ErrorHue = 28,
                InfoHue = 250,
                Chroma = 0.16
            },

            Controls = new ControlRecipe {
                BorderContrast = 0.12,
                FocusBoost = 0.15,
                DisabledOpacity = 0.45
            }
        };
}