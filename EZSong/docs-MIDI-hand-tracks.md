# EZSong — MIDI main gauche / main droite

Cette fonctionnalité reste entièrement dans EZSong. Aucun composant Hand-thesia
n'est intégré au projet.

## Principe

Le moteur de voicing continue de calculer un voicing unique et optimisé. Lors
de l'export MIDI, `MidiChordExportSettings.SplitPoint` permet de répartir les
notes :

- note strictement inférieure au split : piste `Main gauche` ;
- note égale ou supérieure au split : piste `Main droite`.

La séparation ne modifie donc jamais le voicing calculé.

## Exemple

```csharp
MidiChordExportSettings exportSettings = new()
{
    SplitPoint = new Pitch(NoteStep.C, Alteration.neutral, 3),
    LeftHandChannel = 0,
    RightHandChannel = 1,
    LeftHandTrackName = "Main gauche",
    RightHandTrackName = "Main droite"
};

ChordMidiGenerator generator = new(
    settings: new VoicingSettings(),
    preferences: preferences);

generator.Generate(
    firstRequest,
    followingRequests,
    "accompagnement-2-mains.mid",
    exportSettings);
```

## Compatibilité

L'ancien appel à `MidiChordExporter.Export(voicings, path, velocity)` reste
inchangé et produit toujours un seul track MIDI.

Le nouvel overload permet d'obtenir deux tracks sans changer l'algorithme de
voice-leading.
