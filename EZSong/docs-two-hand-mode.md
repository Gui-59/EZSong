# EZSong V12 — configurable voicing constraints

V12 keeps the V11 two-hand architecture and makes the main voicing constraints
and optimisation rules explicit in the settings objects. Existing default
values are preserved.

## One hand

```csharp
var settings = new VoicingSettings
{
    MinChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
    MaxChordAreaNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
    ChordMinKeyCount = 2,
    ChordMaxKeyCount = 5,
    MaxVoiceMovement = 11,
    CommonNoteWeight = 100,
    MovementWeight = 1,
    BassMovementWeight = 3,
    UserPreferenceWeight = 75,
    PreferCompactInitialVoicing = true
};
```

A weight of `0` disables that preference without disabling the corresponding
hard constraint. Negative weights are rejected.

## Two hands

```csharp
var settings = new TwoHandVoicingSettings
{
    MinLeftHandNote = new Pitch(NoteStep.C, Alteration.neutral, 1),
    MaxLeftHandNote = new Pitch(NoteStep.B, Alteration.neutral, 2),
    MinRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 3),
    MaxRightHandNote = new Pitch(NoteStep.C, Alteration.neutral, 4),
    MinLeftHandVoices = 1,
    MaxLeftHandVoices = 2,
    MinRightHandVoices = 1,
    MaxRightHandVoices = 2,
    MinTotalVoices = 2,
    MaxTotalVoices = 4,
    MaxVoiceMovement = 11,
    CommonNoteWeight = 100,
    MovementWeight = 1,
    BassMovementWeight = 3,
    UserPreferenceWeight = 75,
    PreferCompactInitialVoicing = true
};
```

`MinTotalVoices` and `MaxTotalVoices` constrain the number of distinct chord
tones. No note doubling is introduced to satisfy a voice-count constraint.

## Compatibility

All V11 default values remain unchanged, so existing code can continue to
construct `VoicingSettings` and `TwoHandVoicingSettings` without modification.

Hand-thesia remains completely external to EZSong.
