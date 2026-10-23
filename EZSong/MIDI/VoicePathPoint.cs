using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>One point of a persistent voice path across a chord progression.</summary>
public sealed class VoicePathPoint
{
    public int ChordIndex { get; }
    public Chord Chord { get; }
    public Pitch? Note { get; }

    public bool IsPresent {
        get {
            return Note is not null;
        }
    }

    public VoicePathPoint(int chordIndex, Chord chord, Pitch? note)
    {
        if (chordIndex < 0) {
            throw new ArgumentOutOfRangeException(nameof(chordIndex));
        }

        Chord = chord ?? throw new ArgumentNullException(nameof(chord));
        ChordIndex = chordIndex;
        Note = note;
    }

    public override string ToString() {
        return Note is null ? $"[{ChordIndex}] {Chord.ToHumanString()}: -" :
        $"[{ChordIndex}] {Chord.ToHumanString()}: {Note}";
    }
}
