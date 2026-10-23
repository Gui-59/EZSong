using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>One concrete chord realization with notes assigned to two hands.</summary>
public sealed class TwoHandChordVoicing
{
    public Chord Chord { get; }
    public IReadOnlyList<Pitch> LeftHand { get; }
    public IReadOnlyList<Pitch> RightHand { get; }
    public IReadOnlyList<Pitch> Pitches { get; }

    public TwoHandChordVoicing(
        Chord chord,
        IEnumerable<Pitch> leftHand,
        IEnumerable<Pitch> rightHand)
    {
        Chord = chord ?? throw new ArgumentNullException(nameof(chord));
        LeftHand = leftHand?.OrderBy(x => x.ToMidiNoteNumber()).ToArray()
            ?? throw new ArgumentNullException(nameof(leftHand));
        RightHand = rightHand?.OrderBy(x => x.ToMidiNoteNumber()).ToArray()
            ?? throw new ArgumentNullException(nameof(rightHand));
        Pitches = LeftHand.Concat(RightHand).OrderBy(x => x.ToMidiNoteNumber()).ToArray();
    }

    public override string ToString() {
        return $"{Chord.ToHumanString()}: LH[{string.Join(' ', LeftHand)}] RH[{string.Join(' ', RightHand)}]";
    }
}
