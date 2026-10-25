using EZSong.Model;

namespace EZSong.MIDI;

internal static class VoicingMidiExportValidation {
    public static void Validate(VoicingProgression progression, string outputPath, int velocity) {
        ArgumentNullException.ThrowIfNull(progression);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (progression.Entries.Count == 0) {
            throw new ArgumentException("At least one progression entry is required.", nameof(progression));
        }

        ValidateVelocity(velocity);
    }

    public static void Validate(TwoHandVoicingProgression progression, string outputPath, int velocity) {
        ArgumentNullException.ThrowIfNull(progression);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (progression.Entries.Count == 0) {
            throw new ArgumentException("At least one progression entry is required.", nameof(progression));
        }

        ValidateVelocity(velocity);
    }

    private static void ValidateVelocity(int velocity) {
        if (velocity < 1 || velocity > 127) {
            throw new ArgumentOutOfRangeException(nameof(velocity));
        }
    }

    public static long DurationToTicks(RhythmRationalDuration duration) {
        RhythmRationalDuration normalized = duration.ApplyDots().Normalize();
        return (long)Math.Round(
            MidiChordExporter.TicksPerQuarterNote * 4.0 * normalized.Numerator / normalized.Denominator,
            MidpointRounding.AwayFromZero);
    }
}
