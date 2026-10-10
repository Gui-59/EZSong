using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// A concrete MIDI realization of a chord. Notes are always ordered from low to high.
/// </summary>
public sealed class ChordVoicing
{
    public Chord Chord { get; }
    public IReadOnlyList<Pitch> Notes { get; }

    public ChordVoicing(Chord chord, IEnumerable<Pitch> notes)
    {
        Chord = chord ?? throw new ArgumentNullException(nameof(chord));
        Notes = notes.OrderBy(GetMidiNoteNumber).ToArray();
    }

    public IReadOnlyList<int> MidiNotes => Notes.Select(GetMidiNoteNumber).ToArray();

    public override string ToString() =>
        $"{Chord.ToHumanString()}: {string.Join(" ", Notes.Select(FormatPitch))}";

    private static int GetMidiNoteNumber(Pitch pitch) =>
        pitch.MidiOctave * 12 + (int)pitch.Note + (int)pitch.Alteration;

    private static string FormatPitch(Pitch pitch)
    {
        string accidental = pitch.Alteration switch
        {
            Alteration.sharp => "#",
            Alteration.flat => "b",
            _ => string.Empty
        };
        return $"{pitch.Note}{accidental}{pitch.MidiOctave}";
    }
}
