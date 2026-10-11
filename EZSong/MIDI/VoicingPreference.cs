
namespace EZSong.MIDI;

/// <summary>
/// A user's preferred transition. It is a preference, not a hard constraint.
/// </summary>
public sealed class VoicingPreference
{
    public ChordVoicing? FirstChord { get; set; }
    public ChordVoicing? SecondChord { get; set; }
}

public sealed class VoicingPreferenceStore
{
    public List<VoicingPreference> Preferences { get; set; } = new();

    public IEnumerable<VoicingPreference> Find(Model.Chord firstChord, Model.Chord secondChord)
    {
        return Preferences.Where(p =>
            p.FirstChord?.Chord == firstChord &&
            p.SecondChord?.Chord == secondChord);
    }
}
