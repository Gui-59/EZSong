using EZSong.Enums;
using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EZSong.Model {

    /// <summary>
    /// Détecte la tonalité la plus probable à partir des notes et/ou des
    /// accords d'une mesure ou d'un ensemble de notes MIDI. La détection
    /// repose sur l'ajustement des hauteurs observées aux gammes diatoniques
    /// des 30 tonalités standard (cercle des quintes).
    /// </summary>
    public static class KeySignatureDetector {

        /// <summary>
        /// Tonalités candidates (30 tonalités standard). Partagées avec
        /// SelectableValues pour garantir la cohérence des libellés.
        /// </summary>
        public static readonly List<KeySignature> CandidateKeys = BuildCandidateKeys();

        private static List<KeySignature> BuildCandidateKeys() {
            List<KeySignature> list = new();
            // Doit rester synchronisé avec SelectableValues.
            (NoteStep Note, Alteration Alteration)[] majorTonics = {
                (NoteStep.C, Alteration.neutral), (NoteStep.G, Alteration.neutral),
                (NoteStep.D, Alteration.neutral), (NoteStep.A, Alteration.neutral),
                (NoteStep.E, Alteration.neutral), (NoteStep.B, Alteration.neutral),
                (NoteStep.F, Alteration.sharp), (NoteStep.C, Alteration.sharp),
                (NoteStep.F, Alteration.neutral), (NoteStep.B, Alteration.flat),
                (NoteStep.E, Alteration.flat), (NoteStep.A, Alteration.flat),
                (NoteStep.D, Alteration.flat), (NoteStep.G, Alteration.flat),
                (NoteStep.C, Alteration.flat)
            };
            (NoteStep Note, Alteration Alteration)[] minorTonics = {
                (NoteStep.A, Alteration.neutral), (NoteStep.E, Alteration.neutral),
                (NoteStep.B, Alteration.neutral), (NoteStep.F, Alteration.sharp),
                (NoteStep.C, Alteration.sharp), (NoteStep.G, Alteration.sharp),
                (NoteStep.D, Alteration.sharp), (NoteStep.D, Alteration.neutral),
                (NoteStep.G, Alteration.neutral), (NoteStep.C, Alteration.neutral),
                (NoteStep.F, Alteration.neutral), (NoteStep.B, Alteration.flat),
                (NoteStep.E, Alteration.flat), (NoteStep.A, Alteration.flat),
                (NoteStep.D, Alteration.flat)
            };
            foreach (var t in majorTonics) list.Add(new KeySignature(t.Note, t.Alteration, SongMode.major));
            foreach (var t in minorTonics) list.Add(new KeySignature(t.Note, t.Alteration, SongMode.minor));
            return list;
        }

        /// <summary>
        /// Convertit une NoteStep + altération en classe de hauteur (0-11).
        /// </summary>
        public static int NoteToPitchClass(NoteStep note, Alteration alteration) {
            int pc = note switch {
                NoteStep.C => 0,
                NoteStep.D => 2,
                NoteStep.E => 4,
                NoteStep.F => 5,
                NoteStep.G => 7,
                NoteStep.A => 9,
                NoteStep.B => 11,
                _ => 0
            };
            if (alteration == Alteration.flat) pc = (pc + 11) % 12;
            else if (alteration == Alteration.sharp) pc = (pc + 1) % 12;
            return pc;
        }

        private static int[] DiatonicPitchClasses(KeySignature key) {
            int tonic = NoteToPitchClass(key.Note, key.Alteration);
            int[] major = { 0, 2, 4, 5, 7, 9, 11 };
            int[] minor = { 0, 2, 3, 5, 7, 8, 10 };
            int[] intervals = key.Mode == SongMode.major ? major : minor;
            return intervals.Select(i => (tonic + i) % 12).ToArray();
        }

        /// <summary>
        /// Détecte la tonalité à partir d'une liste de classes de hauteur
        /// pondérées (pitchClass -> poids). Retourne null si aucune donnée.
        /// </summary>
        public static KeySignature? DetectFromPitchWeights(Dictionary<int, double> pitchWeights) {
            if (pitchWeights is null || pitchWeights.Count == 0) return null;
            double total = pitchWeights.Values.Sum();
            if (total <= 0) return null;

            KeySignature? best = null;
            double bestScore = double.NegativeInfinity;
            foreach (KeySignature candidate in CandidateKeys) {
                int[] diatonic = DiatonicPitchClasses(candidate);
                double score = 0;
                foreach (KeyValuePair<int, double> kv in pitchWeights) {
                    if (diatonic.Contains(kv.Key)) {
                        score += kv.Value;
                    } else {
                        score -= kv.Value * 0.3;
                    }
                }
                // Bonus pour la présence de la tonique.
                if (pitchWeights.TryGetValue(NoteToPitchClass(candidate.Note, candidate.Alteration), out double tonicWeight)) {
                    score += tonicWeight * 0.5;
                }
                if (score > bestScore) {
                    bestScore = score;
                    best = candidate;
                }
            }
            return best;
        }

        /// <summary>
        /// Détecte la tonalité à partir de notes MIDI (classes de hauteur).
        /// </summary>
        public static KeySignature? DetectFromNotes(
            IEnumerable<(long Start, long End, int Pitch, int Track)> notes) {
            Dictionary<int, double> weights = new();
            foreach (var n in notes) {
                int pc = ((n.Pitch % 12) + 12) % 12;
                weights[pc] = weights.GetValueOrDefault(pc) + 1.0;
            }
            return DetectFromPitchWeights(weights);
        }

        /// <summary>
        /// Détecte la tonalité à partir d'une MeasureData (accords + mélodie).
        /// </summary>
        public static KeySignature? DetectFromMeasure(MeasureData measure) {
            Dictionary<int, double> weights = new();
            // Accords : la fondamentale compte beaucoup, la tierce confirme le mode.
            foreach (ChordBeat beat in measure.ChordSequence.ChordBeats) {
                foreach (Chord chord in beat.Chords) {
                    if (chord.IsSilentChord) continue;
                    int rootPc = NoteToPitchClass(chord.RootNote, chord.RootNoteAlteration);
                    weights[rootPc] = weights.GetValueOrDefault(rootPc) + 5.0;
                    // Tierce : majeure (4) ou mineure (3) selon le type.
                    bool isMinor = chord.ChordType.ToString().ToLowerInvariant().Contains("minor")
                        || chord.ChordType == ChordType.Diminished
                        || chord.ChordType == ChordType.MinorSeventhFlatFive;
                    int third = isMinor ? 3 : 4;
                    int thirdPc = (rootPc + third) % 12;
                    weights[thirdPc] = weights.GetValueOrDefault(thirdPc) + 2.0;
                    // Quinte.
                    int fifthPc = (rootPc + 7) % 12;
                    weights[fifthPc] = weights.GetValueOrDefault(fifthPc) + 1.0;
                }
            }
            // Mélodie : notes des portées.
            foreach (MeasureGlobalMelody global in measure.Staffs) {
                if (global.Melody is null) continue;
                foreach (MelodyChord mc in global.Melody.MelodyChords) {
                    foreach (Pitch pitch in mc.Pitches) {
                        int pc = ((pitch.ToMidiNoteNumber() % 12) + 12) % 12;
                        weights[pc] = weights.GetValueOrDefault(pc) + 0.5;
                    }
                }
            }
            return DetectFromPitchWeights(weights);
        }

        /// <summary>
        /// Détecte la tonalité à partir de plusieurs mesures combinées.
        /// </summary>
        public static KeySignature? DetectFromMeasures(IEnumerable<MeasureData> measures) {
            Dictionary<int, double> weights = new();
            foreach (MeasureData m in measures) {
                KeySignature? single = DetectFromMeasure(m);
                // On combine les poids en re-détectant depuis les notes/accords
                // de chaque mesure pour garder le détail.
                Dictionary<int, double> w = new();
                foreach (ChordBeat beat in m.ChordSequence.ChordBeats) {
                    foreach (Chord chord in beat.Chords) {
                        if (chord.IsSilentChord) continue;
                        int rootPc = NoteToPitchClass(chord.RootNote, chord.RootNoteAlteration);
                        w[rootPc] = w.GetValueOrDefault(rootPc) + 5.0;
                        bool isMinor = chord.ChordType.ToString().ToLowerInvariant().Contains("minor")
                            || chord.ChordType == ChordType.Diminished
                            || chord.ChordType == ChordType.MinorSeventhFlatFive;
                        int third = isMinor ? 3 : 4;
                        int thirdPc = (rootPc + third) % 12;
                        w[thirdPc] = w.GetValueOrDefault(thirdPc) + 2.0;
                        int fifthPc = (rootPc + 7) % 12;
                        w[fifthPc] = w.GetValueOrDefault(fifthPc) + 1.0;
                    }
                }
                foreach (MeasureGlobalMelody global in m.Staffs) {
                    if (global.Melody is null) continue;
                    foreach (MelodyChord mc in global.Melody.MelodyChords) {
                        foreach (Pitch pitch in mc.Pitches) {
                            int pc = ((pitch.ToMidiNoteNumber() % 12) + 12) % 12;
                            w[pc] = w.GetValueOrDefault(pc) + 0.5;
                        }
                    }
                }
                foreach (KeyValuePair<int, double> kv in w) {
                    weights[kv.Key] = weights.GetValueOrDefault(kv.Key) + kv.Value;
                }
            }
            return DetectFromPitchWeights(weights);
        }

        /// <summary>
        /// Indique si la tonalité déclarée pour la mesure semble cohérente
        /// avec ses notes et accords (best key == declared key).
        /// </summary>
        public static bool IsConsistent(MeasureData measure) {
            KeySignature? detected = DetectFromMeasure(measure);
            if (detected is null) return true; // pas assez de données : pas d'alerte
            KeySignature declared = measure.KeySignature;
            return declared.Note == detected.Note
                && declared.Alteration == detected.Alteration
                && declared.Mode == detected.Mode;
        }
    }
}
