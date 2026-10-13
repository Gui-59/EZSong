# EZSong
Minimalist Arranger Music Sheet Editor

## Voicing preferences

EZSong can store preferred voice-leading transitions in a small JSON file. A preference is a **strong preference, not a hard rule**: it only receives an additional score bonus, so normal register and movement constraints still apply.

Example `voicing-preferences.json`:

```json
{
  "preferences": [
    {
      "from": "C",
      "fromNotes": ["E1", "G1", "C2"],
      "to": "Am",
      "toNotes": ["E1", "A1", "C2"]
    },
    {
      "from": "F",
      "fromNotes": ["A1", "C2", "F2"],
      "to": "G7",
      "toNotes": ["G1", "B1", "D2", "F2"]
    }
  ]
}
```

Octaves follow EZSong's convention: `C0 = 0`, `C1 = 12`, `C2 = 24`, etc.

Load the preferences and pass them to the voicing engine or MIDI generator:

```csharp
VoicingPreferenceStore preferences =
    VoicingPreferenceJson.Load("voicing-preferences.json");

ChordMidiGenerator generator = new(
    settings: new VoicingSettings(),
    preferences: preferences);
```

Preference matching is structural: independently parsed `Chord` instances representing the same chord are considered equal for preference lookup. The preferred transition is also tied to the specified starting voicing, so a preference does not unexpectedly apply to a different physical hand position.
