# V13 — Voice tracking

V13 makes the voice correspondence between two successive voicings explicit.

The optimisation rules are unchanged from V12. The engine now exposes the
minimum-distance correspondence that was already used internally for scoring.

## One hand

`ChordVoicingEngine.AnalyzeTransition(...)` now returns `VoiceMovements`.

Each `VoiceMovement` contains:

- `PreviousNote`
- `CurrentNote`
- `SemitoneMovement`
- `IsContinued`
- `IsAdded`
- `IsRemoved`

For example:

```text
C  [E1 G1 C2]
Am [E1 A1 C2]

E1 -> E1   0
G1 -> A1   1
C2 -> C2   0
```

When the number of voices changes, added and removed voices are explicit.

## Two hands

`TwoHandVoicingEngine.AnalyzeTransition(...)` exposes the same information
for the complete two-hand voicing. The matching is performed on the complete
set of notes, not independently per hand.

This keeps the V12 optimisation semantics intact while making the voice
correspondence observable and testable.

## Important

V13 does not yet assign a permanent identity such as `Voice 1`, `Voice 2`, etc.
to a voice across an entire progression. It exposes the optimal correspondence
for each transition. A persistent voice-path model can be built on top of this
in a later version without changing the existing optimisation API.
