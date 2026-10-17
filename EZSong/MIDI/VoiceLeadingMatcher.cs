using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Finds the minimum-distance correspondence between voices in two voicings.
/// The rules deliberately match the existing engine semantics:
/// - same number of voices: all voices are matched;
/// - fewer new voices: some previous voices may disappear;
/// - more new voices: all previous voices are matched and new voices are added;
/// - every matched movement must be <= maxMovement;
/// - added notes must be reachable from at least one previous note.
/// </summary>
internal static class VoiceLeadingMatcher
{
    private const int _impossible = 1_000_000;

    public static VoiceLeadingMatch Match(
        IReadOnlyList<Pitch> previous,
        IReadOnlyList<Pitch> current,
        int maxMovement)
    {
        if (previous.Count == 0 || current.Count == 0) {
            return VoiceLeadingMatch.Invalid;
        }

        Pitch[] oldNotes = previous.OrderBy(x => x.ToMidiNoteNumber()).ToArray();
        Pitch[] newNotes = current.OrderBy(x => x.ToMidiNoteNumber()).ToArray();

        if (newNotes.Length > oldNotes.Length)
        {
            foreach (Pitch added in newNotes)
            {
                if (!oldNotes.Any(old => Math.Abs(old - added) <= maxMovement)) {
                    return VoiceLeadingMatch.Invalid;
                }
            }
        }

        int oldCount = oldNotes.Length;
        int newCount = newNotes.Length;
        int[,] memo = new int[oldCount + 1, 1 << newCount];
        bool[,] seen = new bool[oldCount + 1, 1 << newCount];

        int Solve(int index, int usedMask)
        {
            if (index == oldCount) {
                return 0;
            }

            if (seen[index, usedMask]) {
                return memo[index, usedMask];
            }

            seen[index, usedMask] = true;
            int best = _impossible;

            // A previous voice may disappear when the new voicing has fewer voices.
            if (oldCount > newCount) {
                best = Math.Min(best, Solve(index + 1, usedMask));
            }

            for (int j = 0; j < newCount; j++)
            {
                if ((usedMask & (1 << j)) != 0) {
                    continue;
                }

                int distance = Math.Abs(oldNotes[index] - newNotes[j]);
                if (distance > maxMovement) {
                    continue;
                }

                int rest = Solve(index + 1, usedMask | (1 << j));
                if (rest < _impossible) {
                    best = Math.Min(best, distance + rest);
                }
            }

            memo[index, usedMask] = best;
            return best;
        }

        int total = Solve(0, 0);
        if (total >= _impossible) {
            return VoiceLeadingMatch.Invalid;
        }

        List<(int Previous, int Current)> pairs = new();
        int row = 0;
        int mask = 0;
        while (row < oldCount)
        {
            int target = memo[row, mask];
            bool chosen = false;

            // Prefer a real voice continuation over removing a voice when both
            // have exactly the same optimal cost. This keeps identities stable.
            for (int j = 0; j < newCount; j++)
            {
                if ((mask & (1 << j)) != 0) {
                    continue;
                }

                int distance = Math.Abs(oldNotes[row] - newNotes[j]);
                if (distance > maxMovement) {
                    continue;
                }

                int rest = Solve(row + 1, mask | (1 << j));
                if (rest < _impossible && distance + rest == target)
                {
                    pairs.Add((row, j));
                    mask |= 1 << j;
                    row++;
                    chosen = true;
                    break;
                }
            }

            if (chosen) {
                continue;
            }

            // The only remaining optimal choice can be disappearance of this voice.
            if (oldCount > newCount && Solve(row + 1, mask) == target)
            {
                row++;
                continue;
            }

            return VoiceLeadingMatch.Invalid;
        }

        bool[] matchedCurrent = new bool[newCount];
        List<VoiceMovement> movements = new(oldCount + newCount);
        foreach ((int previousIndex, int currentIndex) in pairs)
        {
            matchedCurrent[currentIndex] = true;
            movements.Add(new VoiceMovement(oldNotes[previousIndex], newNotes[currentIndex]));
        }

        // Preserve the old voice order for diagnostics; removed voices are explicit.
        bool[] matchedPrevious = new bool[oldCount];
        foreach ((int previousIndex, _) in pairs) {
            matchedPrevious[previousIndex] = true;
        }

        for (int i = 0; i < oldCount; i++)
        {
            if (!matchedPrevious[i]) {
                movements.Add(new VoiceMovement(oldNotes[i], null));
            }
        }

        for (int i = 0; i < newCount; i++)
        {
            if (!matchedCurrent[i]) {
                movements.Add(new VoiceMovement(null, newNotes[i]));
            }
        }

        // Stable display order: continued/removed voices by previous pitch, then added voices.
        movements = movements
            .OrderBy(m => m.PreviousNote is null ? 1 : 0)
            .ThenBy(m => (m.PreviousNote ?? m.CurrentNote)!.ToMidiNoteNumber())
            .ToList();

        return new VoiceLeadingMatch(total, movements);
    }
}

internal sealed class VoiceLeadingMatch
{
    public static VoiceLeadingMatch Invalid { get; } = new(0, Array.Empty<VoiceMovement>(), false);

    public int TotalMovement { get; }
    public bool IsValid { get; }
    public IReadOnlyList<VoiceMovement> Movements { get; }

    public VoiceLeadingMatch(int totalMovement, IReadOnlyList<VoiceMovement> movements, bool isValid = true)
    {
        TotalMovement = totalMovement;
        Movements = movements;
        IsValid = isValid;
    }
}
