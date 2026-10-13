using System.Text.Json;
using System.Text.RegularExpressions;
using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Reads and writes user voicing preferences in a deliberately simple JSON format.
///
/// Example:
/// {
///   "preferences": [
///     {
///       "from": "C",
///       "fromNotes": ["E1", "G1", "C2"],
///       "to": "Am",
///       "toNotes": ["E1", "A1", "C2"]
///     }
///   ]
/// }
///
/// The chord names use the same notation as ChordProgressionParser.
/// Pitch octaves follow EZSong's convention: C0 = 0, C1 = 12, C2 = 24, ...
/// </summary>
public static class VoicingPreferenceJson
{
    private sealed class FileModel
    {
        public List<PreferenceModel> Preferences { get; set; } = new();
    }

    private sealed class PreferenceModel
    {
        public string? From { get; set; }
        public string? To { get; set; }
        public List<string>? FromNotes { get; set; }
        public List<string>? ToNotes { get; set; }
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex PitchRegex = new(
        @"^(?<note>[A-Ga-g])(?<accidental>[#b♯♭]?)(?<octave>[0-9]+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static VoicingPreferenceStore Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return new VoicingPreferenceStore();
        }

        string json = File.ReadAllText(path);
        FileModel model = JsonSerializer.Deserialize<FileModel>(json, Options)
            ?? new FileModel();

        VoicingPreferenceStore store = new();

        foreach (PreferenceModel item in model.Preferences)
        {
            if (string.IsNullOrWhiteSpace(item.From) ||
                string.IsNullOrWhiteSpace(item.To) ||
                item.FromNotes is null ||
                item.ToNotes is null)
            {
                throw new FormatException(
                    "A voicing preference must contain 'from', 'fromNotes', 'to', and 'toNotes'.");
            }

            if (item.FromNotes.Count < 2 || item.FromNotes.Count > 5 ||
                item.ToNotes.Count < 2 || item.ToNotes.Count > 5)
            {
                throw new FormatException(
                    $"Preferred voicing '{item.From} -> {item.To}' must contain between 2 and 5 notes.");
            }

            Chord fromChord = ChordProgressionParser.ParseChord(item.From);
            Chord toChord = ChordProgressionParser.ParseChord(item.To);
            Pitch[] fromNotes = item.FromNotes.Select(ParsePitch).ToArray();
            Pitch[] toNotes = item.ToNotes.Select(ParsePitch).ToArray();

            store.Preferences.Add(new VoicingPreference
            {
                FirstChord = new ChordVoicing(fromChord, fromNotes),
                SecondChord = new ChordVoicing(toChord, toNotes)
            });
        }

        return store;
    }

    public static void Save(string path, VoicingPreferenceStore store)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(store);

        FileModel model = new();

        foreach (VoicingPreference preference in store.Preferences)
        {
            if (preference.FirstChord is null || preference.SecondChord is null)
            {
                continue;
            }

            model.Preferences.Add(new PreferenceModel
            {
                From = preference.FirstChord.Chord.ToHumanString(),
                To = preference.SecondChord.Chord.ToHumanString(),
                FromNotes = preference.FirstChord.Notes.Select(FormatPitch).ToList(),
                ToNotes = preference.SecondChord.Notes.Select(FormatPitch).ToList()
            });
        }

        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(model, Options));
    }

    private static Pitch ParsePitch(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FormatException("A preferred MIDI pitch cannot be empty.");
        }

        Match match = PitchRegex.Match(text.Trim());
        if (!match.Success)
        {
            throw new FormatException(
                $"Invalid pitch '{text}'. Expected values such as E1, Bb2, or C#3.");
        }

        NoteStep note = match.Groups["note"].Value[0] switch
        {
            'C' or 'c' => NoteStep.C,
            'D' or 'd' => NoteStep.D,
            'E' or 'e' => NoteStep.E,
            'F' or 'f' => NoteStep.F,
            'G' or 'g' => NoteStep.G,
            'A' or 'a' => NoteStep.A,
            'B' or 'b' => NoteStep.B,
            _ => throw new FormatException($"Invalid pitch '{text}'.")
        };

        Alteration alteration = match.Groups["accidental"].Value switch
        {
            "#" or "♯" => Alteration.sharp,
            "b" or "♭" => Alteration.flat,
            _ => Alteration.neutral
        };

        int octave = int.Parse(match.Groups["octave"].Value);
        return new Pitch(note, alteration, octave);
    }

    private static string FormatPitch(Pitch pitch)
    {
        string accidental = pitch.Alteration switch
        {
            Alteration.sharp => "#",
            Alteration.flat => "b",
            _ => string.Empty
        };

        return $"{pitch.Note}{accidental}{pitch.MidiOctave}";
    }
}
