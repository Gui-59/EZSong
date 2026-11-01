using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// High-level entry point for generating a MIDI accompaniment from a chord progression.
/// The voicing engine deliberately ignores rhythm; the MIDI exporter uses each Chord's Duration.
/// </summary>
public sealed class ChordMidiGenerator {
    private readonly ChordVoicingEngine _voicingEngine;

    public ChordMidiGenerator(VoicingSettings? settings = null, VoicingPreferenceStore? preferences = null) {
        _voicingEngine = new ChordVoicingEngine(settings, preferences);
    }

    /// <summary>
    /// Generates a MIDI file from a chord progression, starting with the first chord and continuing with the following chords.
    /// </summary>
    /// <param name="firstChord">The first chord in the progression.</param>
    /// <param name="followingChords">The chords that follow the first chord in the progression.</param>
    /// <param name="outputPath">The file path where the generated MIDI file will be saved.</param>
    /// <param name="velocity">The velocity (volume) of the MIDI notes.</param>
    /// <returns>A read-only list of generated chord voicings.</returns>
    public IReadOnlyList<ChordVoicing> Generate(
        ChordVoicingRequest firstChord,
        IEnumerable<ChordVoicingRequest> followingChords,
        string outputPath,
        int velocity = 90) {
        ArgumentNullException.ThrowIfNull(firstChord);
        ArgumentNullException.ThrowIfNull(followingChords);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        ChordVoicingRequest[] requests = new[] { firstChord }
            .Concat(followingChords)
            .ToArray();

        IReadOnlyList<ChordVoicing> result =
            _voicingEngine.GenerateProgression(requests);

        MidiChordExporter.Export(result, outputPath, velocity);
        return result;
    }

    /// <summary>
    /// Generates a MIDI file from a chord progression with specified export settings, 
    /// starting with the first chord and continuing with the following chords.
    /// </summary>
    /// <param name="firstChord">The first chord in the progression.</param>
    /// <param name="followingChords">The chords that follow the first chord in the progression.</param>
    /// <param name="outputPath">The file path where the generated MIDI file will be saved.</param>
    /// <param name="exportSettings">The export settings for the MIDI file.</param>
    /// <param name="velocity">The velocity (volume) of the MIDI notes.</param>
    /// <returns>A read-only list of generated chord voicings.</returns>
    public IReadOnlyList<ChordVoicing> Generate(
        ChordVoicingRequest firstChord,
        IEnumerable<ChordVoicingRequest> followingChords,
        string outputPath,
        MidiChordExportSettings exportSettings,
        int velocity = 90) {
        ArgumentNullException.ThrowIfNull(exportSettings);
        ArgumentNullException.ThrowIfNull(firstChord);
        ArgumentNullException.ThrowIfNull(followingChords);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        ChordVoicingRequest[] requests = new[] { firstChord }
            .Concat(followingChords)
            .ToArray();

        IReadOnlyList<ChordVoicing> result =
            _voicingEngine.GenerateProgression(requests);

        MidiChordExporter.Export(result, outputPath, exportSettings, velocity);
        return result;
    }

    /// <summary>
    /// Generates a MIDI file from a chord progression and tracks the voices across the progression, 
    /// starting with the first chord and continuing with the following chords.
    /// </summary>
    /// <param name="firstChord">The first chord in the progression.</param>
    /// <param name="followingChords">The chords that follow the first chord in the progression.</param>
    /// <param name="outputPath">The file path where the generated MIDI file will be saved.</param>
    /// <param name="velocity">The velocity (volume) of the MIDI notes.</param>
    /// <returns>A VoiceTrackingResult containing the tracked voices.</returns>
    public VoiceTrackingResult GenerateTracked(
        ChordVoicingRequest firstChord,
        IEnumerable<ChordVoicingRequest> followingChords,
        string outputPath,
        int velocity = 90) {
        IReadOnlyList<ChordVoicing> voicings = Generate(
            firstChord, followingChords, outputPath, velocity);
        return _voicingEngine.TrackVoices(voicings);
    }

    /// <summary>
    /// Generates a MIDI file from a chord progression with specified export settings and tracks the voices across the progression,
    /// starting with the first chord and continuing with the following chords.
    /// </summary>
    /// <param name="firstChord">The first chord in the progression.</param>
    /// <param name="followingChords">The chords that follow the first chord in the progression.</param>
    /// <param name="outputPath">The file path where the generated MIDI file will be saved.</param>
    /// <param name="exportSettings">The export settings for the MIDI file.</param>
    /// <param name="velocity">The velocity (volume) of the MIDI notes.</param>
    /// <returns>A VoiceTrackingResult containing the tracked voices.</returns>
    public VoiceTrackingResult GenerateTracked(
        ChordVoicingRequest firstChord,
        IEnumerable<ChordVoicingRequest> followingChords,
        string outputPath,
        MidiChordExportSettings exportSettings,
        int velocity = 90) {
        IReadOnlyList<ChordVoicing> voicings = Generate(
            firstChord, followingChords, outputPath, exportSettings, velocity);
        return _voicingEngine.TrackVoices(voicings);
    }

    /// <summary>
    /// Generates a MIDI file from a ChordSequence, 
    /// starting with the first chord and continuing with the following chords.
    /// </summary>
    /// <param name="sequence">The ChordSequence containing the chords to generate.</param>
    /// <param name="firstChord">The first chord in the progression.</param>
    /// <param name="outputPath">The file path where the generated MIDI file will be saved.</param>
    /// <param name="velocity">The velocity (volume) of the MIDI notes.</param>
    /// <returns>A list of generated ChordVoicings.</returns>
    public IReadOnlyList<ChordVoicing> Generate(
        ChordSequence sequence,
        ChordVoicingRequest firstChord,
        string outputPath,
        int velocity = 90) {
        ArgumentNullException.ThrowIfNull(sequence);

        ChordVoicingRequest[] requests = sequence.ChordBeats
            .SelectMany(beat => beat.Chords)
            .Where(chord => !chord.IsSilentChord)
            .Select(chord => new ChordVoicingRequest(chord))
            .ToArray();

        return Generate(firstChord, requests.Skip(1), outputPath, velocity);
    }

    /// <summary>
    /// Generates a MIDI file (for two-hand voicing) from a ChordSequence with specified export settings,
    /// starting with the first chord and continuing with the following chords.
    /// </summary>
    /// <param name="firstChord">The first chord in the progression.</param>
    /// <param name="followingChords">The chords that follow the first chord in the progression.</param>
    /// <param name="outputPath">The file path where the generated MIDI file will be saved.</param>
    /// <param name="voicingSettings">The settings for two-hand voicing.</param>
    /// <param name="exportSettings">The export settings for the MIDI file.</param>
    /// <param name="velocity">The velocity (volume) of the MIDI notes.</param>
    /// <param name="preferences">The voicing preferences.</param>
    /// <returns>A list of generated TwoHandChordVoicings.</returns>
    public IReadOnlyList<TwoHandChordVoicing> GenerateTwoHands(
        ChordVoicingRequest firstChord,
        IEnumerable<ChordVoicingRequest> followingChords,
        string outputPath,
        TwoHandVoicingSettings voicingSettings,
        MidiChordExportSettings exportSettings,
        int velocity = 90,
        VoicingPreferenceStore? preferences = null) {
        ArgumentNullException.ThrowIfNull(firstChord);
        ArgumentNullException.ThrowIfNull(followingChords);
        ArgumentNullException.ThrowIfNull(voicingSettings);
        ArgumentNullException.ThrowIfNull(exportSettings);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        ChordVoicingRequest[] requests = new[] { firstChord }
            .Concat(followingChords)
            .ToArray();

        TwoHandVoicingEngine engine = new(voicingSettings, preferences);
        IReadOnlyList<TwoHandChordVoicing> result = engine.GenerateProgression(requests);
        TwoHandMidiExporter.Export(result, outputPath, exportSettings, velocity);
        return result;
    }

    /// <summary>
    /// Exports an already-generated true musical result (VoicingProgression) to a MIDI file with specified export settings.
    /// </summary>
    /// <param name="progression">The progression to export.</param>
    /// <param name="outputPath">The file path where the generated MIDI file will be saved.</param>
    /// <param name="exportSettings">The export settings for the MIDI file.</param>
    /// <param name="velocity">The velocity (volume) of the MIDI notes.</param>
    /// <returns>The exported VoicingProgression.</returns>
    public VoicingProgression Export(
        VoicingProgression progression,
        string outputPath,
        MidiChordExportSettings exportSettings,
        int velocity = 90) {
        MidiChordExporter.Export(progression, outputPath, exportSettings, velocity);
        return progression;
    }

    /// <summary>
    /// Exports an already-generated two-hand voicing progression to a MIDI file with specified export settings.
    /// </summary>
    /// <param name="progression">The progression to export.</param>
    /// <param name="outputPath">The file path where the generated MIDI file will be saved.</param>
    /// <param name="exportSettings">The export settings for the MIDI file.</param>
    /// <param name="velocity">The velocity (volume) of the MIDI notes.</param>
    /// <returns>The exported TwoHandVoicingProgression.</returns>
    public TwoHandVoicingProgression ExportTwoHands(
        TwoHandVoicingProgression progression,
        string outputPath,
        MidiChordExportSettings exportSettings,
        int velocity = 90) {
        TwoHandMidiExporter.Export(progression, outputPath, exportSettings, velocity);
        return progression;
    }

}

