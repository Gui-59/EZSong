using EZSong.Model;

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

    public IEnumerable<VoicingPreference> Find(Chord firstChord, Chord secondChord)
    {
        ArgumentNullException.ThrowIfNull(firstChord);
        ArgumentNullException.ThrowIfNull(secondChord);

        return Preferences.Where(p =>
            p.FirstChord is not null &&
            p.SecondChord is not null &&
            SameChord(p.FirstChord.Chord, firstChord) &&
            SameChord(p.SecondChord.Chord, secondChord));
    }

    private static bool SameChord(Chord a, Chord b)
    {
        return a.IsSilentChord == b.IsSilentChord &&
               a.RootNote == b.RootNote &&
               a.RootNoteAlteration == b.RootNoteAlteration &&
               a.ChordType == b.ChordType &&
               a.BassNote == b.BassNote &&
               a.BassNoteAlteration == b.BassNoteAlteration;
    }
}
