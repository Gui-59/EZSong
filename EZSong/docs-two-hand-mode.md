# EZSong V11 — true two-hand voicing

V11 adds a genuine two-hand voicing engine. Unlike the V10 MIDI split, the
left/right ranges are constraints during voicing generation and therefore
influence the global voice-leading optimisation.

Example:

```csharp
var voicingSettings = new TwoHandVoicingSettings
{
    MinLeftHandNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
    MaxLeftHandNote = new Pitch(NoteStep.B, Alteration.neutral, 2),
    MinRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
    MaxRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 4),
    MinLeftHandVoices = 1,
    MaxLeftHandVoices = 2,
    MinRightHandVoices = 1,
    MaxRightHandVoices = 2,
    MaxTotalVoices = 4
};

var exportSettings = new MidiChordExportSettings();

var preferences = VoicingPreferenceJson.Load("voicing-preferences.json");
var generator = new ChordMidiGenerator();
var voicings = generator.GenerateTwoHands(
    firstChord,
    followingChords,
    "two-hands.mid",
    voicingSettings,
    exportSettings,
    preferences: preferences);
```

The resulting MIDI has two tracks, but the important distinction is that the
notes were already assigned to the hands by the voicing engine.

Hand-thesia remains completely external to EZSong.
