# V15 — Persistent voice identity

V15 builds on the transition-level voice correspondence introduced in V13.
It assigns a stable `VoiceId` to each voice and follows that identity through
a complete generated progression.

## Main API

```csharp
VoiceTrackingResult tracking = engine.TrackVoices(progression);
```

or, when generating MIDI directly:

```csharp
VoiceTrackingResult tracking = generator.GenerateTracked(
    firstChord,
    followingChords,
    "output.mid");
```

`ChordVoicingEngine.TrackVoices(...)` uses the same `MaxVoiceMovement` setting
as the voicing engine.

## What is tracked

`VoiceTrackingResult` exposes:

- `Voicings`: every chord with its notes assigned to `TrackedVoice` objects;
- `VoicePaths`: one `VoicePath` per persistent voice identity.

Each `VoicePath` contains one `VoicePathPoint` per chord position. A null note
means that the voice is absent at that position.

Example:

```text
Chord 0: C    [E1 G1 C2]
Chord 1: Am   [E1 A1 C2]
Chord 2: Am7  [E1 A1 C2 G2]

Voice 1: E1   E1   E1
Voice 2: G1   A1   A1
Voice 3: C2   C2   C2
Voice 4: -    -    G2
```

The new G2 receives a new identity instead of stealing an existing voice.

## Identity rules

- The first chord creates `Voice 1`, `Voice 2`, etc., in pitch order.
- A continued voice keeps its existing ID.
- A removed voice becomes absent (`null`) for the remaining progression.
- A later added note receives a new ID; a removed voice is not resurrected.
- The same optimal `VoiceLeadingMatcher` used by V13/V14 determines the
  correspondence between successive chords.
- Existing voicing optimisation, chord-tone priority, slash-bass handling and
  user preferences are unchanged.

V15 is intentionally a tracking layer: it does not alter the chosen voicings
or the generated MIDI notes.

## MIDI

The persistent IDs are metadata only. `MidiChordExporter` and
`TwoHandMidiExporter` continue to write exactly the same MIDI notes and timing
as before. This keeps V15 compatible with Hand-thesia and the existing EZSong
MIDI workflow.
