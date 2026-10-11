using System.Text.RegularExpressions;
using EZSong.Enums;
using EZSong.Model;

namespace EZSong.MIDI;

/// <summary>
/// Parses a text chord progression (one chord per line) and resolves it to
/// playable voicings using <see cref="ChordVoicingEngine"/>.
///
/// Example:
/// <code>
/// C
/// G/B
/// Am
/// F
/// G7
/// C
/// </code>
/// </summary>
public sealed class ChordProgressionParser
{
    private static readonly Regex _chordRootRegex = new(
        @"^\s*(?<root>[A-Ga-g](?:#|b|♯|♭)?)(?<rest>.*?)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _slashBassRegex = new(
        @"/(?<bass>[A-Ga-g](?:#|b|♯|♭)?)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ChordVoicingEngine _voicingEngine;

    public ChordProgressionParser(ChordVoicingEngine? voicingEngine = null)
    {
        _voicingEngine = voicingEngine ?? new ChordVoicingEngine();
    }

    /// <summary>
    /// Parses the supplied text into EZSong <see cref="Chord"/> instances.
    /// Empty lines and lines beginning with // are ignored.
    /// </summary>
    public IReadOnlyList<Chord> ParseChords(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        List<Chord> chords = new();
        string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal)) {
                continue;
            }

            try
            {
                chords.Add(ParseChord(line));
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Invalid chord at line {i + 1}: '{line}'. {ex.Message}", ex);
            }
        }

        return chords;
    }

    /// <summary>
    /// Parses the progression and generates one voicing for each chord.
    /// The first chord is generated automatically unless an exact first voicing
    /// is supplied.
    /// </summary>
    public IReadOnlyList<ChordVoicing> ParseToVoicings(
        string text,
        IEnumerable<Pitch>? exactFirstVoicing = null)
    {
        IReadOnlyList<Chord> chords = ParseChords(text);

        if (chords.Count == 0) {
            return Array.Empty<ChordVoicing>();
        }

        Pitch[]? exact = exactFirstVoicing?.ToArray();
        List<ChordVoicingRequest> requests = new(chords.Count);

        for (int i = 0; i < chords.Count; i++)
        {
            requests.Add(i == 0 && exact is not null
                ? ChordVoicingRequest.FromExactVoicing(chords[i], exact)
                : new ChordVoicingRequest(chords[i]));
        }

        return _voicingEngine.GenerateProgression(requests);
    }

    /// <summary>
    /// Parses a single chord name.
    /// </summary>
    public static Chord ParseChord(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        string value = text.Trim();
        Match match = _chordRootRegex.Match(value);
        if (!match.Success)
        {
            throw new FormatException(
                "Expected a chord such as C, Am, G7, Bbmaj7, or G/B.");
        }

        string rootText = match.Groups["root"].Value;
        string rest = match.Groups["rest"].Value.Trim();

        // A slash followed by a note name is a slash-chord bass. This deliberately
        // leaves suffixes such as 6/9 intact.
        Match bassMatch = _slashBassRegex.Match(rest);
        string suffix;
        string bassText = string.Empty;
        if (bassMatch.Success)
        {
            suffix = rest[..bassMatch.Index].Trim();
            bassText = bassMatch.Groups["bass"].Value;
        }
        else
        {
            suffix = rest;
        }

        (NoteStep rootNote, Alteration rootAlteration) = ParseNoteName(rootText);
        ChordType chordType = ParseChordType(suffix);

        NoteStep? bassNote = null;
        Alteration bassAlteration = Alteration.neutral;

        if (bassText.Length > 0)
        {
            (bassNote, bassAlteration) = ParseNoteName(bassText);
        }

        return new Chord(
            false,
            new RhythmRationalDuration(1, 1, 0),
            rootNote,
            rootAlteration,
            chordType,
            bassNote,
            bassAlteration);
    }

    private static (NoteStep Note, Alteration Alteration) ParseNoteName(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) {
            throw new FormatException("Missing note name.");
        }

        NoteStep note = text[0] switch
        {
            'C' or 'c' => NoteStep.C,
            'D' or 'd' => NoteStep.D,
            'E' or 'e' => NoteStep.E,
            'F' or 'f' => NoteStep.F,
            'G' or 'g' => NoteStep.G,
            'A' or 'a' => NoteStep.A,
            'B' or 'b' => NoteStep.B,
            _ => throw new FormatException($"Unknown note '{text}'.")
        };

        Alteration alteration = Alteration.neutral;

        if (text.Length == 2)
        {
            alteration = text[1] switch
            {
                '#' or '♯' => Alteration.sharp,
                'b' or '♭' => Alteration.flat,
                _ => throw new FormatException($"Unknown accidental in '{text}'.")
            };
        }
        else if (text.Length > 2)
        {
            throw new FormatException($"Invalid note name '{text}'.");
        }

        return (note, alteration);
    }

    private static ChordType ParseChordType(string suffix)
    {
        string trimmed = suffix.Trim();

        // Keep the common uppercase M notation distinct from lowercase m.
        if (trimmed == "M7") {
            return ChordType.MajorSeventh;
        }

        string normalized = trimmed.ToLowerInvariant();

        return normalized switch
        {
            "" or "maj" or "major" => ChordType.NoneOrMajor,
            "m" or "min" or "-" => ChordType.Minor,
            "7" => ChordType.Seventh,
            "m7" or "min7" => ChordType.MinorSeventh,
            "maj7" or "major7" => ChordType.MajorSeventh,
            "5" => ChordType.PowerChord,
            "6" => ChordType.Sixth,
            "m6" or "min6" => ChordType.MinorSixth,
            "sus2" => ChordType.SuspendedSecond,
            "sus4" or "sus" => ChordType.SuspendedFourth,
            "dim" or "o" => ChordType.Diminished,
            "aug" or "+" => ChordType.Augmented,
            "dim7" or "o7" => ChordType.DiminishedSeventh,
            "aug7" or "+7" => ChordType.AugmentedSeventh,
            "add2" => ChordType.AddSecond,
            "add4" => ChordType.AddFourth,
            "add6" => ChordType.AddSixth,
            "add9" => ChordType.AddNinth,
            "9" => ChordType.Ninth,
            "m9" or "min9" => ChordType.MinorNinth,
            "maj9" or "major9" => ChordType.MajorNinth,
            "11" => ChordType.Eleventh,
            "m11" or "min11" => ChordType.MinorEleventh,
            "maj11" or "major11" => ChordType.MajorEleventh,
            "13" => ChordType.Thirteenth,
            "m13" or "min13" => ChordType.MinorThirteenth,
            "maj13" or "major13" => ChordType.MajorThirteenth,
            "m(maj7)" or "min(maj7)" => ChordType.MinorMajorSeventh,
            "6/9" => ChordType.SixthNinth,
            "7-5" or "7b5" => ChordType.SeventhMinusFive,
            "7+5" or "7#5" => ChordType.SeventhPlusFive,
            "m7b5" or "min7b5" => ChordType.MinorSeventhFlatFive,
            _ => throw new FormatException($"Unknown chord suffix '{suffix}'.")
        };
    }
}
