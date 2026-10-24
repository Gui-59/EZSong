using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Centralises transition analysis and scoring shared by one-hand and true
/// two-hand voicing engines. It contains no candidate-generation logic.
/// </summary>
internal static class VoicingTransitionScorer
{
    public static VoicingTransitionAnalysis Analyze(
        Chord previousChord,
        IReadOnlyList<Pitch> previousNotes,
        Chord currentChord,
        IReadOnlyList<Pitch> currentNotes,
        VoicingOptimizationSettings settings,
        VoicingPreferenceStore? preferences = null)
    {
        ArgumentNullException.ThrowIfNull(previousChord);
        ArgumentNullException.ThrowIfNull(previousNotes);
        ArgumentNullException.ThrowIfNull(currentChord);
        ArgumentNullException.ThrowIfNull(currentNotes);
        ArgumentNullException.ThrowIfNull(settings);

        Pitch[] previous = previousNotes.OrderBy(x => x).ToArray();
        Pitch[] current = currentNotes.OrderBy(x => x).ToArray();

        if (previous.Length == 0 || current.Length == 0)
        {
            return Invalid(Array.Empty<Pitch>());
        }

        Pitch[] commonNotes = previous.Intersect(current).OrderBy(x => x).ToArray();
        VoiceLeadingMatch matching = VoiceLeadingMatcher.Match(
            previous,
            current,
            settings.MaxVoiceMovement);

        if (!matching.IsValid)
        {
            return Invalid(commonNotes, matching.TotalMovement);
        }

        int bassMovement = Math.Abs(current[0] - previous[0]);
        if (bassMovement > settings.MaxVoiceMovement)
        {
            return new VoicingTransitionAnalysis(
                commonNotes,
                matching.TotalMovement,
                bassMovement,
                int.MaxValue,
                false,
                matching.Movements);
        }

        int score =
            -commonNotes.Length * settings.CommonNoteWeight
            + matching.TotalMovement * settings.MovementWeight
            + bassMovement * settings.BassMovementWeight;

        if (preferences is not null)
        {
            foreach (VoicingPreference preference in preferences.Find(previousChord, currentChord))
            {
                if (preference.FirstChord is not null &&
                    preference.SecondChord is not null &&
                    preference.FirstChord.Pitches.SequenceEqual(previous) &&
                    preference.SecondChord.Pitches.SequenceEqual(current))
                {
                    score -= settings.UserPreferenceWeight;
                    break;
                }
            }
        }

        return new VoicingTransitionAnalysis(
            commonNotes,
            matching.TotalMovement,
            bassMovement,
            score,
            true,
            matching.Movements);
    }

    private static VoicingTransitionAnalysis Invalid(
        IReadOnlyList<Pitch> commonNotes,
        int totalMovement = 0) =>
        new(
            commonNotes,
            totalMovement,
            0,
            int.MaxValue,
            false);
}
