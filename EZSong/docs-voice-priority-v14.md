# V14 — Intelligent chord-tone reduction

V14 adds an explicit policy for chords that contain more distinct chord tones
than the configured maximum number of voices (2–5).

## Default policy

`ChordTonePriorityPolicy.EssentialsThenExtensions` keeps, in order:

1. root;
2. third;
3. seventh;
4. suspension / altered tone when applicable;
5. extensions (9th, 11th, 13th);
6. fifth last.

This means a `C13` reduced to five voices keeps the root, 3rd, 7th, 9th and
13th, omitting the 5th.

## Alternative policy

`ChordTonePriorityPolicy.EssentialsThenFifth` keeps the fifth before extensions.
For `C13` with five voices, that gives root, 3rd, 5th, 7th and 9th.

## Slash chords

The requested slash bass is mandatory during reduction. If necessary, it takes
one of the available voice slots before the normal priority ordering is applied.

## Shared behaviour

The same selector is used by one-hand and true two-hand voicing engines, so the
musical omission policy does not change merely because the output is generated
for two hands.

Exact voicings remain sacred and are not reduced.
