using EZSong.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EZSong.MIDI {

    /// <summary>
    /// Détecte l'accord le plus probable à partir des notes MIDI réellement
    /// jouées pendant une fenêtre rythmique (en général un temps).
    ///
    /// Principe :
    /// - les notes présentes dans le temps pèsent plus que celles de la mesure
    ///   (qui servent de contexte pour les arpèges) ;
    /// - un accord n'est proposé que si ses notes caractéristiques sont
    ///   réellement présentes et si une part suffisante des notes est expliquée ;
    /// - la note la plus grave sert à détecter les renversements (accord/basse) ;
    /// - en cas d'ambiguïté (C6 / Am7, par exemple) la basse et la
    ///   fondamentale départagent les candidats.
    /// </summary>
    public static class MidiChordAnalyzer {

        /// <summary>
        /// Note MIDI avec ses bornes absolues en ticks, sa piste d'origine et
        /// son poids harmonique. Un poids réduit atténue une note sans la
        /// supprimer : c'est le cas des notes mélodiques, qui ne doivent pas
        /// imposer leur couleur aux accords tout en restant une matière de
        /// secours lorsqu'un passage ne contient qu'elles.
        /// </summary>
        public readonly record struct MidiNote(long Start, long End, int Pitch, int Track, double Weight = 1.0);

        /// <summary>
        /// Accord détecté pour une fenêtre rythmique. <paramref name="BassPitchClass"/>
        /// n'est renseigné que lorsqu'il s'agit d'un renversement (basse
        /// différente de la fondamentale).
        /// </summary>
        public sealed record DetectedChord(int RootPitchClass, ChordType Type, int? BassPitchClass);

        private sealed record ChordShape(ChordType Type, int[] Intervals);

        /// <summary>
        /// Vocabulaire d'accords reconnu. Seules les formes courantes sont
        /// incluses : ajouter des formes très riches rendrait la détection
        /// ambiguë sans améliorer la fidélité aux notes jouées.
        /// </summary>
        private static readonly ChordShape[] _shapes = {
            new(ChordType.NoneOrMajor, new[] { 0, 4, 7 }),           // X
            new(ChordType.Minor, new[] { 0, 3, 7 }),                 // Xm
            new(ChordType.PowerChord, new[] { 0, 7 }),               // X5
            new(ChordType.Diminished, new[] { 0, 3, 6 }),            // Xdim
            new(ChordType.Augmented, new[] { 0, 4, 8 }),             // Xaug
            new(ChordType.SuspendedSecond, new[] { 0, 2, 7 }),       // Xsus2
            new(ChordType.SuspendedFourth, new[] { 0, 5, 7 }),       // Xsus4
            new(ChordType.Sixth, new[] { 0, 4, 7, 9 }),              // X6
            new(ChordType.MinorSixth, new[] { 0, 3, 7, 9 }),         // Xm6
            new(ChordType.Seventh, new[] { 0, 4, 7, 10 }),           // X7
            new(ChordType.MajorSeventh, new[] { 0, 4, 7, 11 }),      // Xmaj7
            new(ChordType.MinorSeventh, new[] { 0, 3, 7, 10 }),      // Xm7
            new(ChordType.MinorMajorSeventh, new[] { 0, 3, 7, 11 }), // Xm(maj7)
            new(ChordType.DiminishedSeventh, new[] { 0, 3, 6, 9 }),  // Xdim7
            new(ChordType.MinorSeventhFlatFive, new[] { 0, 3, 6, 10 }), // Xm7b5
            new(ChordType.SeventhMinusFive, new[] { 0, 4, 6, 10 }),  // X7-5
            new(ChordType.SeventhPlusFive, new[] { 0, 4, 8, 10 }),   // X7+5
            new(ChordType.AddNinth, new[] { 0, 4, 7, 14 }),          // Xadd9
            new(ChordType.Ninth, new[] { 0, 4, 7, 10, 14 }),         // X9
            new(ChordType.MajorNinth, new[] { 0, 4, 7, 11, 14 }),    // Xmaj9
            new(ChordType.MinorNinth, new[] { 0, 3, 7, 10, 14 }),    // Xm9
            new(ChordType.SixthNinth, new[] { 0, 4, 7, 9, 14 })      // X6/9
        };

        /// <summary>
        /// Sous-ensemble d'accords "simples" (triades, 7e, sus, dim, aug, 6e)
        /// pour une analyse moins détaillée (pas de 9e/11e/13e/add9).
        /// </summary>
        private static readonly ChordShape[] _simpleShapes = _shapes.Where(s =>
            s.Type is not (ChordType.AddNinth or ChordType.Ninth or ChordType.MajorNinth
                or ChordType.MinorNinth or ChordType.SixthNinth or ChordType.Ninth)).ToArray();

        /// <summary>
        /// Analyse une fenêtre rythmique et renvoie l'accord qui explique le
        /// mieux les notes entendues, ou null si aucun accord n'est fiable.
        /// </summary>
        /// <param name="notes">Toutes les notes disponibles pour l'analyse.</param>
        /// <param name="beatStart">Début de la fenêtre (ticks).</param>
        /// <param name="beatEnd">Fin de la fenêtre (ticks).</param>
        /// <param name="measureStart">Début de la mesure (contexte).</param>
        /// <param name="measureEnd">Fin de la mesure (contexte).</param>
        /// <param name="previous">Accord retenu pour la fenêtre précédente, s'il existe.
        /// Il sert à prolonger un accord dont une note caractéristique n'est
        /// momentanément plus jouée (accord incomplet ou arpégé).</param>
        public static DetectedChord? Analyze(
            IReadOnlyList<MidiNote> notes,
            double beatStart,
            double beatEnd,
            double measureStart,
            double measureEnd,
            DetectedChord? previous = null,
            bool simpleOnly = false) {

            if (notes.Count == 0 ||
                beatEnd <= beatStart ||
                measureEnd <= measureStart) {
                return null;
            }

            double beatLength = beatEnd - beatStart;
            double measureLength = measureEnd - measureStart;
            double[] weights = new double[12];
            double totalWeight = 0;
            double lowestPitch = double.MaxValue;
            int bassPitchClass = -1;

            foreach (MidiNote note in notes) {
                // Une note de poids nul (mélodie résiduelle) est ignorée : elle
                // ne doit ni colorer l'accord ni gonfler le poids total.
                if (note.Weight <= 0.0) {
                    continue;
                }
                double beatOverlap = Overlap(note.Start, note.End, beatStart, beatEnd);
                if (beatOverlap <= 0) {
                    continue;
                }

                double measureOverlap = Overlap(note.Start, note.End, measureStart, measureEnd);
                double beatRatio = beatOverlap / beatLength;
                double measureRatio = measureOverlap / measureLength;

                // Une note déjà tenue au début du temps fait partie de
                // l'accord ; le contexte de mesure aide à reconnaître les
                // accords arpégés.
                bool held = note.Start < beatStart;
                double weight = beatRatio * (held ? 1.15 : 1.0)
                              + measureRatio * 0.25;

                // Les notes jugées mélodiques pèsent moins lourd : la ligne
                // chantée colore l'harmonie sans la définir.
                weight *= Math.Max(0.0, note.Weight);

                int pitchClass = Mod12(note.Pitch);
                weights[pitchClass] += weight;
                totalWeight += weight;

                // Seule une note réellement tenue sur une partie du temps
                // peut définir la basse (évite les notes de passage). Les notes
                // à poids nul (mélodies résiduelles) ne peuvent pas définir la
                // basse : elles n'apportent aucune information harmonique.
                // Seules les notes d'accompagnement (poids suffisant) peuvent
                // définir la basse : une mélodie résiduelle, même grave, ne
                // doit pas créer un renversement.
                if (note.Weight >= 0.5 && beatRatio >= 0.15 && note.Pitch < lowestPitch) {
                    lowestPitch = note.Pitch;
                    bassPitchClass = pitchClass;
                }
            }

            if (totalWeight <= 0 || weights.Count(weight => weight > 0) < 2) {
                return null;
            }

            DetectedChord? best = null;
            double bestScore = double.NegativeInfinity;

            ChordShape[] shapes = simpleOnly ? _simpleShapes : _shapes;
            for (int root = 0; root < 12; root++) {
                foreach (ChordShape shape in shapes) {
                    double score = ScoreShape(shape, root, weights, totalWeight,
                        bassPitchClass, previous, out int? inversionBass);
                    if (score > bestScore) {
                        bestScore = score;
                        best = new DetectedChord(root, shape.Type, inversionBass);
                    }
                }
            }

            return best;
        }

        /// <summary>
        /// Évalue un candidat. Renvoie -infini si l'accord ne correspond pas
        /// assez précisément aux notes jouées.
        /// </summary>
        private static double ScoreShape(
            ChordShape shape,
            int root,
            double[] weights,
            double totalWeight,
            int bassPitchClass,
            DetectedChord? previous,
            out int? inversionBass) {

            inversionBass = null;

            int[] tones = shape.Intervals
                .Select(interval => Mod12(root + interval))
                .Distinct()
                .ToArray();

            double insideWeight = 0;
            int matches = 0;
            foreach (int pitchClass in tones) {
                if (weights[pitchClass] > 0) {
                    insideWeight += weights[pitchClass];
                    matches++;
                }
            }

            // Un accord doit reposer sur au moins deux hauteurs distinctes.
            if (matches < 2) {
                return double.NegativeInfinity;
            }

            bool isPrevious = previous is not null
                && previous.RootPitchClass == root
                && previous.Type == shape.Type;

            // Toutes les notes caractéristiques doivent être présentes.
            // La quinte (7 demi-tons) est facultative : elle est souvent
            // omise au piano, contrairement à la tierce ou à la septième.
            // Exception : l'accord du temps précédent peut être prolongé avec
            // une note caractéristique momentanément absente, à condition que
            // les notes jouées soient toutes expliquées par cet accord.
            double incompletePenalty = 0;
            if (!isPrevious) {
                foreach (int interval in shape.Intervals) {
                    if (interval == 7) {
                        continue;
                    }

                    int pitchClass = Mod12(root + interval);
                    if (weights[pitchClass] >= totalWeight * 0.03) {
                        continue;
                    }

                    // Une triade majeure ou mineure dont la tierce manque
                    // reste identifiable si la fondamentale et la quinte sont
                    // réellement jouées (voicing incomplet, arpège coupé par
                    // la barre de temps). On la pénalise sans la rejeter.
                    bool isThird = interval == 3 || interval == 4;
                    int fifth = Mod12(root + 7);
                    if (isThird
                        && (shape.Type == ChordType.NoneOrMajor || shape.Type == ChordType.Minor)
                        && weights[fifth] >= totalWeight * 0.03) {
                        incompletePenalty += 8.0;
                        continue;
                    }

                    return double.NegativeInfinity;
                }
            }

            // Les notes jouées doivent être majoritairement expliquées par
            // l'accord : une note étrangère tenue sur tout le temps disqualifie
            // les accords qui l'ignorent. Un accord déjà retenu au temps
            // précédent est un peu plus tolérant, car une note de l'accord
            // peut cesser de sonner sans que l'harmonie change.
            double coverage = insideWeight / totalWeight;
            if (coverage < (isPrevious ? 0.55 : 0.70)) {
                return double.NegativeInfinity;
            }

            double rootShare = weights[Mod12(root)] / totalWeight;
            double score = coverage * 100.0
                         + matches * 7.0
                         + rootShare * 10.0
                         - tones.Length * 1.5
                         + TypePrior(shape.Type)
                         - incompletePenalty;

            if (isPrevious) {
                // Prolonger l'accord déjà entendu évite de changer d'accord
                // sur un temps où seule une partie du voicing est rejouée.
                score += 14.0;
            }

            // La basse réelle départage les accords enharmoniques
            // (C6 / Am7) et permet d'indiquer les renversements.
            if (bassPitchClass >= 0) {
                if (bassPitchClass == Mod12(root)) {
                    score += 8.0;
                } else if (tones.Contains(bassPitchClass)) {
                    score += 2.0;
                    inversionBass = bassPitchClass;
                } else {
                    score -= 8.0;
                }
            }

            return score;
        }

        /// <summary>
        /// Priorité donnée aux qualités d'accord les plus courantes. Les accords
        /// suspendus et les sixtes sont rares : à notes égales, un accord avec
        /// tierce (ou une septième) est préféré.
        /// </summary>
        private static double TypePrior(ChordType type) {
            return type switch {
                ChordType.NoneOrMajor => 4.0,
                ChordType.Minor => 4.0,
                ChordType.Seventh => 2.0,
                ChordType.MinorSeventh => 2.0,
                ChordType.MajorSeventh => 1.0,
                ChordType.PowerChord => -8.0,
                ChordType.SuspendedSecond => -6.0,
                ChordType.SuspendedFourth => -6.0,
                ChordType.Sixth => -8.0,
                ChordType.MinorSixth => -8.0,
                ChordType.SixthNinth => -6.0,
                ChordType.AddNinth => -1.0,
                ChordType.Ninth => -1.0,
                ChordType.MajorNinth => -1.0,
                ChordType.MinorNinth => -1.0,
                _ => 0.0
            };
        }

        private static double Overlap(long start, long end, double windowStart, double windowEnd) {
            return Math.Max(0, Math.Min(end, windowEnd) - Math.Max(start, windowStart));
        }

        private static int Mod12(int value) {
            return ((value % 12) + 12) % 12;
        }
    }
}
