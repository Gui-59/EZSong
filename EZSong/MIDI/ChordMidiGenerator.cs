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

        List<ChordVoicing> result = new();
        ChordVoicing current = _voicingEngine.GenerateFirst(firstChord);
        result.Add(current);

        foreach (ChordVoicingRequest request in followingChords)
        {
            current = _voicingEngine.GenerateNext(request, current);
            result.Add(current);
        }

        MidiChordExporter.Export(result, outputPath, velocity);
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
}
