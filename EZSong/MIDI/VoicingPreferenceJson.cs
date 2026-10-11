using System.Text.Json;

namespace EZSong.MIDI;

public static class VoicingPreferenceJson
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static VoicingPreferenceStore Load(string path)
    {
        if (!File.Exists(path)) {
            return new VoicingPreferenceStore();
        }

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<VoicingPreferenceStore>(json, _options)
               ?? new VoicingPreferenceStore();
    }

    public static void Save(string path, VoicingPreferenceStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory)) {
            _ = Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(store, _options));
    }
}
