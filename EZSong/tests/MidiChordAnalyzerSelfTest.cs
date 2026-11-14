using EZSong.Enums;
using EZSong.MIDI;

namespace EZSong.Tests;

/// <summary>
/// Auto-tests de la détection d'accords : qualités courantes, renversements,
/// accords incomplets et prolongation d'un accord sur un temps voisin.
/// </summary>
public static class MidiChordAnalyzerSelfTest {

    private const double BeatLength = 480;
    private const double MeasureLength = 1920;

    public static void RunAll() {
        TestMajor();
        TestMinor();
        TestDominantSeventh();
        TestMajorSeventh();
        TestMinorSeventh();
        TestSuspendedSecond();
        TestSuspendedFourth();
        TestDiminished();
        TestHalfDiminishedSeventh();
        TestInversion();
        TestIncompleteTriad();
        TestSingleNoteHasNoChord();
        TestPreviousChordIsProlonged();
        Console.WriteLine("MidiChordAnalyzerSelfTest: ALL OK");
    }

    private static void TestMajor() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60, 64, 67); // C E G
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.NoneOrMajor,
            "Do mi sol doit donner Do majeur.");
    }

    private static void TestMinor() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60, 63, 67); // C Eb G
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.Minor,
            "Do mib sol doit donner Do mineur.");
    }

    private static void TestDominantSeventh() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(67, 71, 74, 77); // G B D F
        Expect(chord is not null
            && chord.RootPitchClass == 7
            && chord.Type == ChordType.Seventh,
            "Sol si ré fa doit donner Sol7.");
    }

    private static void TestMajorSeventh() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60, 64, 67, 71); // C E G B
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.MajorSeventh,
            "Do mi sol si doit donner Domaj7.");
    }

    private static void TestMinorSeventh() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(69, 72, 76, 79); // A C E G
        Expect(chord is not null
            && chord.RootPitchClass == 9
            && chord.Type == ChordType.MinorSeventh,
            "La do mi sol doit donner Lam7.");
    }

    private static void TestSuspendedSecond() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60, 62, 67); // C D G
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.SuspendedSecond,
            "Do ré sol doit donner Dosus2.");
    }

    private static void TestSuspendedFourth() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60, 65, 67); // C F G
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.SuspendedFourth,
            "Do fa sol doit donner Dosus4.");
    }

    private static void TestDiminished() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60, 63, 66); // C Eb Gb
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.Diminished,
            "Do mib solb doit donner Dodim.");
    }

    private static void TestHalfDiminishedSeventh() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60, 63, 66, 70); // C Eb Gb Bb
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.MinorSeventhFlatFive,
            "Do mib solb sib doit donner Dom7b5.");
    }

    private static void TestInversion() {
        // Mi grave, puis sol et do : Do majeur avec Mi à la basse.
        MidiChordAnalyzer.DetectedChord? chord = Analyze(52, 55, 60, 72);
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.NoneOrMajor
            && chord.BassPitchClass == 4,
            "Mi sol do do doit donner Do/Mi.");
    }

    private static void TestIncompleteTriad() {
        // Fondamentale et quinte seules : on préfère l'accord complet (Do)
        // à une quinte "Do5", plus rare dans une grille d'accords.
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60, 67); // C G
        Expect(chord is not null
            && chord.RootPitchClass == 0
            && chord.Type == ChordType.NoneOrMajor,
            "Do sol doit donner Do, même si la tierce n'est pas jouée.");
    }

    private static void TestSingleNoteHasNoChord() {
        MidiChordAnalyzer.DetectedChord? chord = Analyze(60);
        Expect(chord is null, "Une seule note ne doit produire aucun accord.");
    }

    private static void TestPreviousChordIsProlonged() {
        // Sol et ré seulement : la tierce de Sol mineur n'est plus jouée.
        // L'accord précédent doit être conservé plutôt que remplacé par un
        // accord majeur ou une quinte sans tierce.
        MidiChordAnalyzer.DetectedChord previous =
            new(7, ChordType.Minor, null);
        MidiChordAnalyzer.DetectedChord? chord = Analyze(previous, 55, 62); // G D
        Expect(chord is not null
            && chord.RootPitchClass == 7
            && chord.Type == ChordType.Minor,
            "L'accord précédent doit être prolongé sur un voicing incomplet.");
    }

    private static MidiChordAnalyzer.DetectedChord? Analyze(params int[] pitches) {
        return Analyze(null, pitches);
    }

    private static MidiChordAnalyzer.DetectedChord? Analyze(
        MidiChordAnalyzer.DetectedChord? previous, params int[] pitches) {
        List<MidiChordAnalyzer.MidiNote> notes = pitches
            .Select(pitch => new MidiChordAnalyzer.MidiNote(0, (long)MeasureLength, pitch, 0))
            .ToList();
        return MidiChordAnalyzer.Analyze(
            notes, 0, BeatLength, 0, MeasureLength, previous);
    }

    private static void Expect(bool condition, string message) {
        if (!condition) {
            throw new InvalidOperationException(message);
        }
    }
}
