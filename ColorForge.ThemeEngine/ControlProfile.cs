namespace ColorForge.ThemeEngine;

/// <summary>
/// Defines the visual characteristics used to generate control states.
/// </summary>
public sealed record ControlProfile {
    /// <summary>
    /// Overall visual prominence of interactive controls.
    /// </summary>
    public required double Emphasis {
        get; init;
    }

    /// <summary>
    /// Intensity of hover states.
    /// </summary>
    public required double HoverStrength {
        get; init;
    }

    /// <summary>
    /// Intensity of pressed states.
    /// </summary>
    public required double PressedStrength {
        get; init;
    }

    /// <summary>
    /// Intensity of selection states.
    /// </summary>
    public required double SelectionStrength {
        get; init;
    }

    /// <summary>
    /// Visibility of control borders.
    /// </summary>
    public required double BorderStrength {
        get; init;
    }

    /// <summary>
    /// Visibility of disabled controls.
    /// </summary>
    public required double DisabledStrength {
        get; init;
    }

    /// <summary>
    /// Creates a balanced control profile.
    /// </summary>
    public static ControlProfile Balanced {
        get {
            return new() {
                Emphasis = 1.0,
                HoverStrength = 1.0,
                PressedStrength = 1.0,
                SelectionStrength = 1.0,
                BorderStrength = 1.0,
                DisabledStrength = 1.0
            };
        }
    }
}