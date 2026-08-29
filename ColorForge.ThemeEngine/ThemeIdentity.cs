using ColorForge.Core;

namespace ColorForge.ThemeEngine;

public sealed record ThemeIdentity {
    public ThemeIdentity(
        ThemeColor accent,
        ThemeMode mode,
        ThemeProfile profile) {
        Accent = accent;
        Mode = mode;
        Profile = profile;
    }

    /// <summary>
    /// Base accent color chosen by the application.
    /// </summary>
    public ThemeColor Accent {
        get;
    }

    /// <summary>
    /// Light or Dark theme.
    /// </summary>
    public ThemeMode Mode {
        get;
    }

    /// <summary>
    /// Visual profile used to generate the theme.
    /// </summary>
    public ThemeProfile Profile {
        get;
    }
}