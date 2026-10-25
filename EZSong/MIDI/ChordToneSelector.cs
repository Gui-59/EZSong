using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Selects the most useful distinct pitch classes when a chord has more tones
/// than the available voice count. The selection is deterministic and keeps a
/// slash-chord bass mandatory.
/// </summary>
internal static class ChordToneSelector {
    public static Pitch[] Select(
        Chord chord,
        IReadOnlyList<Pitch> pitchClasses,
        int maxVoices,
        PitchClass? bass,
        ChordTonePriorityPolicy policy)
    {
        if (pitchClasses.Count <= maxVoices) {
            return pitchClasses.Distinct().OrderBy(x => x).ToArray();
        }

        HashSet<Pitch> available = pitchClasses.Distinct().ToHashSet();
        List<Pitch> selected = new();

        if (bass is not null) {
            Pitch bassPitch = bass.Value.ChromaticPitch;
            if (!available.Contains(bassPitch)) {
                throw new InvalidOperationException(
                    $"Bass {bass.Value.Note} is not a note of chord {chord.ToHumanString()}.");
            }
            selected.Add(bassPitch);
        }

        IEnumerable<Pitch> ordered = available
            .Where(p => !selected.Contains(p))
            .OrderBy(p => Priority(chord, p, policy))
            .ThenBy(p => ExtensionOrder(chord, p))
            .ThenBy(p => p);

        foreach (Pitch pitch in ordered) {
            if (selected.Count >= maxVoices) {
                break;
            }
            selected.Add(pitch);
        }

        return selected.Distinct().ToArray();
    }

    private static int Priority(Chord chord, Pitch pitch, ChordTonePriorityPolicy policy) {
        int interval = IntervalFromRoot(chord, pitch);
        ChordToneRole role = Role(chord, interval);

        return policy switch {
            ChordTonePriorityPolicy.EssentialsThenFifth => role switch {
                ChordToneRole.Root => 0,
                ChordToneRole.Third => 10,
                ChordToneRole.Seventh => 20,
                ChordToneRole.Fifth => 30,
                ChordToneRole.Suspension => 40,
                ChordToneRole.Altered => 45,
                _ => 50,
            },
            _ => role switch {
                ChordToneRole.Root => 0,
                ChordToneRole.Third => 10,
                ChordToneRole.Seventh => 20,
                ChordToneRole.Suspension => 25,
                ChordToneRole.Altered => 30,
                ChordToneRole.Extension => 40,
                ChordToneRole.Fifth => 50,
                _ => 60,
            },
        };
    }

    private static int ExtensionOrder(Chord chord, Pitch pitch) {
        int interval = IntervalFromRoot(chord, pitch);
        return interval switch {
            2 => 0,   // 9th / add2
            5 => 1,   // 11th / add4
            9 => 2,   // 13th / 6th
            _ => 3,
        };
    }

    private static ChordToneRole Role(Chord chord, int interval) {
        if (interval == 0) {
            return ChordToneRole.Root;
        }

        bool suspended = chord.ChordType is ChordType.SuspendedSecond or ChordType.SuspendedFourth;
        if (suspended && (interval == 2 || interval == 5)) {
            return ChordToneRole.Suspension;
        }

        if (interval == 3 || interval == 4) {
            return ChordToneRole.Third;
        }

        if (interval == 7) {
            return ChordToneRole.Fifth;
        }

        if (interval is 10 or 11) {
            return ChordToneRole.Seventh;
        }

        if (interval is 2 or 5 or 9) {
            return ChordToneRole.Extension;
        }

        if (interval is 6 or 8) {
            return ChordToneRole.Altered;
        }

        return ChordToneRole.Extension;
    }

    private static int IntervalFromRoot(Chord chord, Pitch pitch) {
        int root = ((int)chord.RootNote + (int)chord.RootNoteAlteration) % 12;
        int value = pitch.ToMidiNoteNumber() % 12;
        return (value - root + 12) % 12;
    }

    private enum ChordToneRole {
        Root,
        Third,
        Fifth,
        Seventh,
        Suspension,
        Extension,
        Altered,
    }
}
