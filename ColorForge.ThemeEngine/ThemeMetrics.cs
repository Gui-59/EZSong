namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual metrics shared by every control.
/// </summary>
public sealed record ThemeMetrics {
    public double CornerRadius {
        get; init;
    }

    public double BorderWidth {
        get; init;
    }

    public double FocusThickness {
        get; init;
    }

    public double FocusOffset {
        get; init;
    }

    public double DisabledOpacity {
        get; init;
    }

    public double HoverOpacity {
        get; init;
    }

    public double PressedOpacity {
        get; init;
    }

    public double SelectionOpacity {
        get; init;
    }
}