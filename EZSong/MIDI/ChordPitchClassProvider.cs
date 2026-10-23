using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>Single source of truth for the pitch classes belonging to each chord type.</summary>
internal static class ChordPitchClassProvider
{
    public static IReadOnlyList<Pitch> Get(Chord chord)
    {
        ArgumentNullException.ThrowIfNull(chord);

        int root = ((int)chord.RootNote + (int)chord.RootNoteAlteration + 12) % 12;
        int[] intervals = chord.ChordType switch
        {
            ChordType.NoneOrMajor => [0, 4, 7],
            ChordType.Minor => [0, 3, 7],
            ChordType.Seventh => [0, 4, 7, 10],
            ChordType.MinorSeventh => [0, 3, 7, 10],
            ChordType.MajorSeventh => [0, 4, 7, 11],
            ChordType.PowerChord => [0, 7],
            ChordType.Sixth => [0, 4, 7, 9],
            ChordType.MinorSixth => [0, 3, 7, 9],
            ChordType.SuspendedSecond => [0, 2, 7],
            ChordType.SuspendedFourth => [0, 5, 7],
            ChordType.Diminished => [0, 3, 6],
            ChordType.Augmented => [0, 4, 8],
            ChordType.DiminishedSeventh => [0, 3, 6, 9],
            ChordType.AugmentedSeventh => [0, 4, 8, 10],
            ChordType.AddSecond => [0, 2, 4, 7],
            ChordType.AddFourth => [0, 4, 5, 7],
            ChordType.AddSixth => [0, 4, 7, 9],
            ChordType.AddNinth => [0, 4, 7, 2],
            ChordType.Ninth => [0, 4, 7, 10, 2],
            ChordType.MinorNinth => [0, 3, 7, 10, 2],
            ChordType.MajorNinth => [0, 4, 7, 11, 2],
            ChordType.Eleventh => [0, 4, 7, 10, 2, 5],
            ChordType.MinorEleventh => [0, 3, 7, 10, 2, 5],
            ChordType.MajorEleventh => [0, 4, 7, 11, 2, 5],
            ChordType.Thirteenth => [0, 4, 7, 10, 2, 9],
            ChordType.MinorThirteenth => [0, 3, 7, 10, 2, 9],
            ChordType.MajorThirteenth => [0, 4, 7, 11, 2, 9],
            ChordType.MinorMajorSeventh => [0, 3, 7, 11],
            ChordType.SixthNinth => [0, 4, 7, 9, 2],
            ChordType.SeventhMinusFive => [0, 4, 6, 10],
            ChordType.SeventhPlusFive => [0, 4, 8, 10],
            ChordType.MinorSeventhFlatFive => [0, 3, 6, 10],
            _ => throw new ArgumentOutOfRangeException(nameof(chord.ChordType), chord.ChordType, "Unsupported chord type.")
        };

        return intervals
            .Select(interval => Pitch.FromMidi((root + interval) % 12))
            .Distinct()
            .OrderBy(pitch => pitch)
            .ToArray();
    }
}
