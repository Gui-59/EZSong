using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// High-level entry point for generating a MIDI accompaniment from a chord progression.
/// The voicing engine deliberately ignores rhythm; the MIDI exporter uses each Chord's Duration.
/// </summary>
public sealed class ChordMidiGenerator
{
    private readonly ChordVoicingEngine _voicingEngine;

    public ChordMidiGenerator(
        VoicingSettings? settings = null,
        VoicingPreferenceStore? preferences = null)
    {
        _voicingEngine = new ChordVoicingEngine(settings, preferences);
    }

    public IReadOnlyList<ChordVoicing> Generate(
        ChordVoicingRequest firstChord,
        IEnumerable<ChordVoicingRequest> followingChords,
        string outputPath,
        int velocity = 90)
    {
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

    public IReadOnlyList<ChordVoicing> Generate(
        ChordVoicingRequest firstChord,
        IEnumerable<ChordVoicingRequest> followingChords,
        string outputPath,
        MidiChordExportSettings exportSettings,
        int velocity = 90)
    {
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

    public IReadOnlyList<ChordVoicing> Generate(
        ChordSequence sequence,
        ChordVoicingRequest firstChord,
        string outputPath,
        int velocity = 90)
    {
        ArgumentNullException.ThrowIfNull(sequence);

        ChordVoicingRequest[] requests = sequence.ChordBeats
            .SelectMany(beat => beat.Chords)
            .Where(chord => !chord.IsSilentChord)
            .Select(chord => new ChordVoicingRequest(chord))
            .ToArray();

        return Generate(firstChord, requests.Skip(1), outputPath, velocity);
    }
    /// <summary>Generates a progression using independent left/right hand ranges.</summary>
    public IReadOnlyList<TwoHandChordVoicing> GenerateTwoHands(
        ChordVoicingRequest firstChord,
        IEnumerable<ChordVoicingRequest> followingChords,
        string outputPath,
        TwoHandVoicingSettings voicingSettings,
        MidiChordExportSettings exportSettings,
        int velocity = 90,
        VoicingPreferenceStore? preferences = null)
    {
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

}

