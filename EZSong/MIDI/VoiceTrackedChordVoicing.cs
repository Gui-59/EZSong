using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>A concrete voicing whose notes have persistent voice identities.</summary>
public sealed class VoiceTrackedChordVoicing
{
    public ChordVoicing Voicing { get; }
    public IReadOnlyList<TrackedVoice> Voices { get; }

    public Chord Chord => Voicing.Chord;
    public IReadOnlyList<Pitch> Pitches => Voicing.Pitches;

    public VoiceTrackedChordVoicing(ChordVoicing voicing, IEnumerable<TrackedVoice> voices)
    {
        Voicing = voicing ?? throw new ArgumentNullException(nameof(voicing));
        Voices = voices?.OrderBy(v => v.Note.ToMidiNoteNumber()).ToArray()
            ?? throw new ArgumentNullException(nameof(voices));

        if (Voices.Count != Voicing.Pitches.Count)
            throw new ArgumentException("The number of tracked voices must match the voicing note count.", nameof(voices));

        if (Voices.Select(v => v.Note).SequenceEqual(Voicing.Pitches) == false)
            throw new ArgumentException("Tracked voice notes must match the voicing notes.", nameof(voices));
    }

    public override string ToString() =>
        $"{Chord.ToHumanString()}: {string.Join(" ", Voices.Select(v => $"V{v.VoiceId}={v.Note}"))}";
}
