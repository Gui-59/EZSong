namespace ColorForge.ThemeEngine;

/// <summary>
/// Visual states of an interactive element.
/// </summary>
public sealed record InteractiveTokens {
    public required ControlState Normal {
        get; init;
    }

    public required ControlState Hover {
        get; init;
    }

    public required ControlState Pressed {
        get; init;
    }

    public required ControlState Disabled {
        get; init;
    }

    public required ControlState Focused {
        get; init;
    }
}