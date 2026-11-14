using EZSong.MIDI;
using EZSong.Model;

namespace EZSong.Tests;

/// <summary>
/// Self-tests for the MIDI import quantization: adaptive grid selection,
/// rhythm/tie construction, melody/rhythm consistency and measure map.
/// </summary>
public static class MidiQuantizationSelfTest {

    private const long TicksPerQuarter = 480;

    public static void RunAll() {
        TestGridSelection();
        TestGridSelectionIsBeatCompatible();
        TestMeasureSpansFollowTimeSignatureChanges();
        TestQuarterNotesProduceOneAttackPerBeat();
        TestNoteAcrossMeasuresUsesTies();
        TestOverlappingNotesAreTruncatedInsteadOfLost();
        TestCompoundBeatUsesDottedDuration();
        TestRandomizedInvariants();
        Console.WriteLine("MidiQuantizationSelfTest: ALL OK");
    }

    private static void TestGridSelection() {
        List<MidiQuantizer.Note> quarters = BuildNotes(spacing: 480, count: 8);
        Expect(MidiQuantizer.ChooseGridSubdivision(quarters, TicksPerQuarter, new TimeSignature(4, 4)) == 1,
            "Quarter-note motion should use a quarter-note grid.");

        List<MidiQuantizer.Note> eighths = BuildNotes(spacing: 240, count: 16);
        Expect(MidiQuantizer.ChooseGridSubdivision(eighths, TicksPerQuarter, new TimeSignature(4, 4)) == 2,
            "Eighth-note motion should use an eighth-note grid.");

        List<MidiQuantizer.Note> sixteenths = BuildNotes(spacing: 120, count: 32);
        Expect(MidiQuantizer.ChooseGridSubdivision(sixteenths, TicksPerQuarter, new TimeSignature(4, 4)) == 4,
            "Sixteenth-note motion should use a sixteenth-note grid.");

        // Slightly humanized eighth notes must stay on the eighth-note grid
        // instead of being forced onto a quarter-note grid.
        List<MidiQuantizer.Note> humanized = new();
        for (int i = 0; i < 16; i++) {
            long jitter = i % 2 == 0 ? 20 : -20;
            long start = i * 240 + jitter;
            humanized.Add(new MidiQuantizer.Note(start, start + 240, 60));
        }
        Expect(MidiQuantizer.ChooseGridSubdivision(humanized, TicksPerQuarter, new TimeSignature(4, 4)) == 2,
            "Humanized eighth notes should keep the eighth-note grid.");
    }

    private static void TestGridSelectionIsBeatCompatible() {
        // A dotted-quarter grid is incompatible with 6/8: the coarsest valid
        // grid is an eighth note, even when only dotted quarters are played.
        List<MidiQuantizer.Note> dottedQuarters = BuildNotes(spacing: 720, count: 4);
        Expect(MidiQuantizer.ChooseGridSubdivision(dottedQuarters, TicksPerQuarter, new TimeSignature(6, 8)) == 2,
            "6/8 must not select a grid that cannot divide its beat.");
    }

    private static void TestMeasureSpansFollowTimeSignatureChanges() {
        List<MidiQuantizer.TimeSignatureChange> changes = new() {
            new MidiQuantizer.TimeSignatureChange(0, 4, 4),
            new MidiQuantizer.TimeSignatureChange(1920, 3, 4)
        };
        List<MidiQuantizer.MeasureSpan> spans = MidiQuantizer.BuildMeasureSpans(
            changes, lastTick: 3360, TicksPerQuarter, new TimeSignature(4, 4));

        Expect(spans.Count == 2, "Two measures are expected for a 4/4 then 3/4 file.");
        Expect(spans[0].StartTicks == 0 && spans[0].EndTicks == 1920,
            "The first measure should span four quarter notes.");
        Expect(spans[1].StartTicks == 1920 && spans[1].EndTicks == 3360,
            "The second measure should span three quarter notes.");
        Expect(spans[1].TimeSignature.Beats == 3 && spans[1].TimeSignature.BeatUnit == 4,
            "The second measure should use the 3/4 time signature.");

        // A change inside a measure is applied at the next bar line so that the
        // current measure keeps a complete duration.
        changes = new List<MidiQuantizer.TimeSignatureChange> {
            new MidiQuantizer.TimeSignatureChange(0, 4, 4),
            new MidiQuantizer.TimeSignatureChange(2400, 6, 8)
        };
        spans = MidiQuantizer.BuildMeasureSpans(changes, lastTick: 3841, TicksPerQuarter, new TimeSignature(4, 4));
        Expect(spans.Count == 3, "A mid-measure change should create a third complete measure.");
        Expect(spans[1].EndTicks == 3840 && spans[2].StartTicks == 3840,
            "The mid-measure time signature change should take effect on the next bar line.");
        Expect(spans[2].TimeSignature.Beats == 6 && spans[2].TimeSignature.BeatUnit == 8,
            "The new 6/8 signature should be used after the bar line.");
    }

    private static void TestQuarterNotesProduceOneAttackPerBeat() {
        MidiQuantizer.MeasureSpan span = new(0, 1920, new TimeSignature(4, 4));
        List<MidiQuantizer.Note> notes = new() {
            new MidiQuantizer.Note(0, 480, 60),
            new MidiQuantizer.Note(480, 960, 62),
            new MidiQuantizer.Note(960, 1440, 64),
            new MidiQuantizer.Note(1440, 1920, 65)
        };
        MidiQuantizer.StaffQuantization staff = MidiQuantizer.Quantize(notes, TicksPerQuarter, 4);
        MeasureRhythmPattern pattern = staff.BuildPattern(0, span);
        List<MelodyChord> melody = staff.BuildMelody(span);

        Expect(pattern.IsDurationValid(), "Quarter-note pattern should have the expected measure duration.");
        Expect(pattern.AreBeatsValid(), "Every beat should have the expected duration.");
        Expect(melody.Count == 4, "Four attacks should produce four melody chords.");
        Expect(pattern.GetAttackCount(null) == melody.Count,
            "The rhythm attack count must match the melody chord count.");
    }

    private static void TestNoteAcrossMeasuresUsesTies() {
        TimeSignature timeSignature = new(4, 4);
        MidiQuantizer.MeasureSpan firstSpan = new(0, 1920, timeSignature);
        MidiQuantizer.MeasureSpan secondSpan = new(1920, 3840, timeSignature);
        List<MidiQuantizer.Note> notes = new() {
            new MidiQuantizer.Note(0, 2400, 60)
        };
        MidiQuantizer.StaffQuantization staff = MidiQuantizer.Quantize(notes, TicksPerQuarter, 4);

        MeasureRhythmPattern firstPattern = staff.BuildPattern(0, firstSpan);
        MeasureRhythmPattern secondPattern = staff.BuildPattern(0, secondSpan);
        List<MelodyChord> firstMelody = staff.BuildMelody(firstSpan);
        List<MelodyChord> secondMelody = staff.BuildMelody(secondSpan);

        Expect(firstPattern.EndsWithTie(), "A note crossing the bar line must end the measure with a tie.");
        Expect(secondPattern.Beats[0].Elements[0] is RhythmSimpleElement firstElement && !firstElement.IsRest(),
            "The next measure must start with the tied continuation, not a rest.");
        Expect(firstMelody.Count == 1, "The attacked note belongs to the first measure.");
        Expect(secondMelody.Count == 0, "A tied continuation is not a new attack and has no melody chord.");

        MeasureData firstMeasure = BuildMeasure(0, firstSpan, firstPattern, firstMelody, null);
        MeasureData secondMeasure = BuildMeasure(1, secondSpan, secondPattern, secondMelody, firstMeasure);
        Expect(firstPattern.IsCompatibleWithNoteCount(firstMeasure.Staffs[0].Melody, null),
            "The first measure should match its melody chord count.");
        Expect(secondPattern.IsCompatibleWithNoteCount(secondMeasure.Staffs[0].Melody, firstMeasure),
            "The tied continuation must not be counted as an attack in the next measure.");
    }

    private static void TestOverlappingNotesAreTruncatedInsteadOfLost() {
        MidiQuantizer.MeasureSpan span = new(0, 1920, new TimeSignature(4, 4));
        List<MidiQuantizer.Note> notes = new() {
            new MidiQuantizer.Note(0, 960, 60),
            new MidiQuantizer.Note(240, 480, 64),
            new MidiQuantizer.Note(480, 720, 67),
            new MidiQuantizer.Note(960, 1440, 72)
        };
        MidiQuantizer.StaffQuantization staff = MidiQuantizer.Quantize(notes, TicksPerQuarter, 4);
        MeasureRhythmPattern pattern = staff.BuildPattern(0, span);
        List<MelodyChord> melody = staff.BuildMelody(span);

        Expect(staff.Notes.Count == 4, "Four distinct attacks are expected after quantization.");
        Expect(melody.Count == 4, "Every attack must keep a melody chord, even when notes overlap.");
        Expect(pattern.GetAttackCount(null) == melody.Count,
            "Truncating a sustained note must not drop the following attacks.");
        Expect(pattern.IsDurationValid() && pattern.AreBeatsValid(),
            "Overlapping notes must still produce a valid measure.");
    }

    private static void TestCompoundBeatUsesDottedDuration() {
        TimeSignature timeSignature = new(6, 8);
        MidiQuantizer.MeasureSpan span = new(0, 1440, timeSignature);
        List<MidiQuantizer.Note> notes = new() {
            new MidiQuantizer.Note(0, 720, 60)
        };
        MidiQuantizer.StaffQuantization staff = MidiQuantizer.Quantize(notes, TicksPerQuarter, 2);
        MeasureRhythmPattern pattern = staff.BuildPattern(0, span);

        Expect(pattern.Beats.Count == 2, "6/8 should contain two dotted-quarter beats.");
        RhythmSimpleElement noteElement = (RhythmSimpleElement)pattern.Beats[0].Elements[0];
        RhythmRationalDuration beatDuration = noteElement.GetEffectiveDuration();
        Expect(!noteElement.IsRest(), "The first beat should start with a note.");
        Expect(beatDuration.Dots == 1 && beatDuration.Denominator == 4,
            "A full 6/8 beat should be notated as a dotted quarter note.");
        Expect(pattern.IsDurationValid() && pattern.AreBeatsValid(),
            "The compound-beat pattern should be valid.");
    }

    private static void TestRandomizedInvariants() {
        Random random = new(12345);
        (TimeSignature Signature, long MeasureTicks)[] signatures = {
            (new TimeSignature(4, 4), 1920),
            (new TimeSignature(3, 4), 1440),
            (new TimeSignature(6, 8), 1440)
        };

        foreach ((TimeSignature signature, long measureTicks) in signatures) {
            for (int iteration = 0; iteration < 40; iteration++) {
                List<MidiQuantizer.Note> notes = new();
                for (int noteIndex = 0; noteIndex < 40; noteIndex++) {
                    long start = random.Next(0, (int)(measureTicks * 4));
                    long length = random.Next(1, (int)(measureTicks * 3));
                    notes.Add(new MidiQuantizer.Note(start, start + length, 48 + random.Next(36)));
                }

                int grid = MidiQuantizer.ChooseGridSubdivision(notes, TicksPerQuarter, signature);
                MidiQuantizer.StaffQuantization staff = MidiQuantizer.Quantize(notes, TicksPerQuarter, grid);
                double lastTick = staff.Notes.Count == 0
                    ? measureTicks
                    : staff.Notes.Max(note => note.EndUnit) * staff.GridTicks;
                List<MidiQuantizer.MeasureSpan> spans = MidiQuantizer.BuildMeasureSpans(
                    Array.Empty<MidiQuantizer.TimeSignatureChange>(), lastTick, TicksPerQuarter, signature);

                MeasureData? previousMeasure = null;
                int totalAttacks = 0;
                for (int spanIndex = 0; spanIndex < spans.Count; spanIndex++) {
                    MidiQuantizer.MeasureSpan span = spans[spanIndex];
                    MeasureRhythmPattern pattern = staff.BuildPattern(0, span);
                    List<MelodyChord> melody = staff.BuildMelody(span);
                    MeasureData measure = BuildMeasure(spanIndex, span, pattern, melody, previousMeasure);

                    Expect(pattern.IsDurationValid() && pattern.AreBeatsValid(),
                        $"Random pattern should be valid ({signature.Beats}/{signature.BeatUnit}).");
                    int attacks = pattern.GetAttackCount(previousMeasure);
                    Expect(attacks == melody.Count,
                        $"Random attacks ({attacks}) must match melody chords ({melody.Count}).");
                    totalAttacks += attacks;
                    previousMeasure = measure;
                }

                Expect(totalAttacks == staff.Notes.Count,
                    $"Every quantized group must produce exactly one attack ({totalAttacks} vs {staff.Notes.Count}).");
            }
        }
    }

    private static List<MidiQuantizer.Note> BuildNotes(long spacing, int count) {
        List<MidiQuantizer.Note> notes = new();
        for (int i = 0; i < count; i++) {
            notes.Add(new MidiQuantizer.Note(i * spacing, (i + 1) * spacing, 60));
        }
        return notes;
    }

    private static MeasureData BuildMeasure(int index, MidiQuantizer.MeasureSpan span,
        MeasureRhythmPattern pattern, List<MelodyChord> melody, MeasureData? precedingMeasure) {
        MeasureGlobalMelody global = new(0);
        global.Pattern = pattern;
        global.Melody = new MeasureMelody(0, melody);
        MeasureData measure = new(index, new SongSettings(), span.TimeSignature, new KeySignature(),
            new ChordSequence(), new List<MeasureGlobalMelody> { global }, string.Empty);
        measure.PrecedingMeasure = precedingMeasure;
        return measure;
    }

    private static void Expect(bool condition, string message) {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}