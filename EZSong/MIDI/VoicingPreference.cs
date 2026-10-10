namespace EZSong.MIDI;

/// <summary>
/// A user's preferred transition. It is a preference, not a hard constraint.
/// </summary>
public sealed class VoicingPreference
{
    public string FromChord { get; set; } = string.Empty;
    public string ToChord { get; set; } = string.Empty;
    public List<int> FromMidiNotes { get; set; } = new();
    public List<int> ToMidiNotes { get; set; } = new();
}

public sealed class VoicingPreferenceStore
{
    public List<VoicingPreference> Preferences { get; set; } = new();

    public IEnumerable<VoicingPreference> Find(string fromChord, string toChord) =>
        Preferences.Where(p =>
            string.Equals(p.FromChord, fromChord, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.ToChord, toChord, StringComparison.OrdinalIgnoreCase));
}
