using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>Builds the musical result layer from parsed progression entries and generated voicings.</summary>
public static class VoicingProgressionBuilder
{
    public static VoicingProgression Build(
        IReadOnlyList<ChordProgressionEntry> entries,
        IReadOnlyList<ChordVoicing> voicings,
        IReadOnlyList<VoicePath> voicePaths,
        int maxVoiceMovement = 11)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(voicings);
        ArgumentNullException.ThrowIfNull(voicePaths);
        if (entries.Count != voicings.Count)
            throw new ArgumentException("The progression entry count must match the voicing count.");

        List<VoicingProgressionEntry> result = new(entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            IReadOnlyList<VoiceMovement> movements = i == 0
                ? Array.Empty<VoiceMovement>()
                : VoiceLeadingMatcher.Match(voicings[i - 1].Pitches, voicings[i].Pitches, maxVoiceMovement).Movements;

            result.Add(new VoicingProgressionEntry(
                entries[i].MeasureNumber,
                entries[i].Chord,
                entries[i].Duration,
                voicings[i],
                movements,
                voicePaths));
        }

        return new VoicingProgression(result, voicePaths);
    }

    public static TwoHandVoicingProgression BuildTwoHands(
        IReadOnlyList<ChordProgressionEntry> entries,
        IReadOnlyList<TwoHandChordVoicing> voicings,
        IReadOnlyList<VoicePath> leftHandVoicePaths,
        IReadOnlyList<VoicePath> rightHandVoicePaths,
        int maxVoiceMovement = 11)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(voicings);
        ArgumentNullException.ThrowIfNull(leftHandVoicePaths);
        ArgumentNullException.ThrowIfNull(rightHandVoicePaths);
        if (entries.Count != voicings.Count)
            throw new ArgumentException("The progression entry count must match the voicing count.");

        List<TwoHandVoicingProgressionEntry> result = new(entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            IReadOnlyList<VoiceMovement> left = i == 0
                ? Array.Empty<VoiceMovement>()
                : VoiceLeadingMatcher.Match(voicings[i - 1].LeftHand, voicings[i].LeftHand, maxVoiceMovement).Movements;
            IReadOnlyList<VoiceMovement> right = i == 0
                ? Array.Empty<VoiceMovement>()
                : VoiceLeadingMatcher.Match(voicings[i - 1].RightHand, voicings[i].RightHand, maxVoiceMovement).Movements;

            result.Add(new TwoHandVoicingProgressionEntry(
                entries[i].MeasureNumber,
                entries[i].Chord,
                entries[i].Duration,
                voicings[i],
                left,
                right,
                leftHandVoicePaths,
                rightHandVoicePaths));
        }

        return new TwoHandVoicingProgression(result, leftHandVoicePaths, rightHandVoicePaths);
    }
}
