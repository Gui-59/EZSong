using EZSong.Model;

namespace EZSong.MIDI {

    /// <summary>
    /// Quantifies MIDI notes on a musical grid and converts them into the
    /// rhythm patterns and melody chords used by the EZSong model.
    ///
    /// The grid is chosen automatically (quarter, eighth or sixteenth note)
    /// according to the notes played in the file. Both melody chords and
    /// rhythm elements are then built from the same quantized note list, which
    /// guarantees that the number of melody chords always matches the number
    /// of attacks in the rhythm pattern.
    /// </summary>
    public static class MidiQuantizer {

        /// <summary>Finest supported grid: 1/16 note (4 subdivisions per quarter).</summary>
        public const int DefaultGridSubdivision = 4;

        private const double AlignmentTolerance = 0.20;
        private const double AlignmentRatio = 0.95;
        private static readonly int[] GridCandidates = { 1, 2, 4 };

        /// <summary>
        /// A source note expressed in MIDI ticks.
        /// </summary>
        public readonly record struct Note(long Start, long End, int Pitch);

        /// <summary>
        /// A time signature event expressed in absolute MIDI ticks.
        /// </summary>
        public readonly record struct TimeSignatureChange(long Time, int Beats, int BeatUnit);

        /// <summary>
        /// A measure described in MIDI ticks, with the time signature in force.
        /// </summary>
        public readonly record struct MeasureSpan(double StartTicks, double EndTicks, TimeSignature TimeSignature) {

            public double DurationTicks {
                get {
                    return EndTicks - StartTicks;
                }
            }

            public bool Contains(double tick) {
                return tick >= StartTicks && tick < EndTicks;
            }
        }

        /// <summary>
        /// A group of simultaneous notes after quantization. Segments never
        /// overlap: as the model is monophonic, a segment is truncated when the
        /// next attack starts.
        /// </summary>
        public sealed class QuantizedNote {

            public long StartUnit {
                get;
            }

            public long EndUnit {
                get;
            }

            public IReadOnlyList<int> Pitches {
                get;
            }

            internal QuantizedNote(long startUnit, long endUnit, IReadOnlyList<int> pitches) {
                StartUnit = startUnit;
                EndUnit = endUnit;
                Pitches = pitches;
            }
        }

        /// <summary>
        /// Quantized content of a single staff.
        /// </summary>
        public sealed class StaffQuantization {

            private readonly IReadOnlyList<QuantizedNote> _notes;
            private readonly IReadOnlyList<(long Units, RhythmRationalDuration Duration)> _durationTable;

            public int GridSubdivision {
                get;
            }

            public double GridTicks {
                get;
            }

            public IReadOnlyList<QuantizedNote> Notes {
                get {
                    return _notes;
                }
            }

            internal StaffQuantization(int gridSubdivision, double gridTicks, IReadOnlyList<QuantizedNote> notes) {
                GridSubdivision = gridSubdivision;
                GridTicks = gridTicks;
                _notes = notes;
                _durationTable = BuildDurationTable(gridSubdivision);
            }

            /// <summary>
            /// Builds the melody chords attacked in the given measure. Chords
            /// continuing from the previous measure are intentionally omitted:
            /// they are carried by the tie of the previous rhythm pattern.
            /// </summary>
            public List<MelodyChord> BuildMelody(MeasureSpan measure) {
                List<MelodyChord> chords = new();
                long measureStartUnit = ToUnit(measure.StartTicks);
                long measureEndUnit = ToUnit(measure.EndTicks);
                foreach (QuantizedNote note in _notes) {
                    if (note.StartUnit < measureStartUnit) {
                        continue;
                    }
                    if (note.StartUnit >= measureEndUnit) {
                        break;
                    }
                    List<Pitch> pitches = new();
                    foreach (int pitch in note.Pitches) {
                        pitches.Add(Pitch.FromMidi(Math.Clamp(pitch, 0, 127)));
                    }
                    chords.Add(new MelodyChord(pitches));
                }
                return chords;
            }

            /// <summary>
            /// Builds the rhythm pattern of the given measure. Each beat is
            /// filled with rests, notes and ties so that its total duration is
            /// exactly the expected beat duration.
            /// </summary>
            public MeasureRhythmPattern BuildPattern(int staffIndex, MeasureSpan measure) {
                MeasureRhythmPattern pattern = new(staffIndex, measure.TimeSignature);
                int beatCount = measure.TimeSignature.GetBeatCount();
                if (beatCount <= 0) {
                    return pattern;
                }

                long measureStartUnit = ToUnit(measure.StartTicks);
                long measureUnits = Math.Max(beatCount, ToUnit(measure.EndTicks) - measureStartUnit);
                long baseBeatUnits = measureUnits / beatCount;
                long extraUnits = measureUnits % beatCount;
                long beatStartUnit = measureStartUnit;

                for (int beat = 0; beat < beatCount; beat++) {
                    long beatUnits = baseBeatUnits + (beat == beatCount - 1 ? extraUnits : 0);
                    long beatEndUnit = beatStartUnit + beatUnits;
                    List<IRhythmElement> elements = new();
                    long cursorUnit = beatStartUnit;

                    foreach (QuantizedNote note in EnumerateNotesOverlapping(beatStartUnit, beatEndUnit)) {
                        long startUnit = Math.Max(note.StartUnit, beatStartUnit);
                        long endUnit = Math.Min(note.EndUnit, beatEndUnit);

                        if (startUnit > cursorUnit) {
                            AppendDuration(elements, startUnit - cursorUnit, false);
                        }

                        long noteStartUnit = Math.Max(startUnit, cursorUnit);
                        if (endUnit > noteStartUnit) {
                            AppendDuration(elements, endUnit - noteStartUnit, true);
                            cursorUnit = endUnit;
                        }

                        if (note.EndUnit > beatEndUnit) {
                            elements.Add(new RhythmTieFrom());
                        }
                    }

                    if (cursorUnit < beatEndUnit) {
                        AppendDuration(elements, beatEndUnit - cursorUnit, false);
                    }

                    pattern.SetBeat(beat, new BeatPattern(elements));
                    beatStartUnit = beatEndUnit;
                }

                return pattern;
            }

            private IEnumerable<QuantizedNote> EnumerateNotesOverlapping(long startUnit, long endUnit) {
                int index = FirstNoteStartingAtOrAfter(startUnit);
                if (index > 0 && _notes[index - 1].EndUnit > startUnit) {
                    index--;
                }

                for (int i = index; i < _notes.Count; i++) {
                    QuantizedNote note = _notes[i];
                    if (note.StartUnit >= endUnit) {
                        yield break;
                    }
                    if (note.EndUnit > startUnit) {
                        yield return note;
                    }
                }
            }

            private int FirstNoteStartingAtOrAfter(long unit) {
                int low = 0;
                int high = _notes.Count;
                while (low < high) {
                    int middle = low + (high - low) / 2;
                    if (_notes[middle].StartUnit < unit) {
                        low = middle + 1;
                    } else {
                        high = middle;
                    }
                }
                return low;
            }

            private long ToUnit(double ticks) {
                return (long)Math.Round(ticks / GridTicks, MidpointRounding.AwayFromZero);
            }

            private void AppendDuration(List<IRhythmElement> elements, long units, bool isNote) {
                long remaining = Math.Max(1, units);
                while (remaining > 0) {
                    RhythmRationalDuration duration = LongestRepresentable(remaining, out long usedUnits);
                    elements.Add(new RhythmSimpleElement(duration, !isNote));
                    remaining -= usedUnits;
                    // Extra parts of the same note are tied together: they do
                    // not create a new attack in the melody.
                    if (isNote && remaining > 0) {
                        elements.Add(new RhythmTieFrom());
                    }
                }
            }

            private RhythmRationalDuration LongestRepresentable(long units, out long usedUnits) {
                foreach ((long Units, RhythmRationalDuration Duration) candidate in _durationTable) {
                    if (candidate.Units <= units) {
                        usedUnits = candidate.Units;
                        return candidate.Duration;
                    }
                }

                // Should not happen: the table always contains the grid unit.
                usedUnits = 1;
                return new RhythmRationalDuration(1, 4 * GridSubdivision, 0);
            }

            private static IReadOnlyList<(long Units, RhythmRationalDuration Duration)> BuildDurationTable(int subdivision) {
                List<(long Units, RhythmRationalDuration Duration)> table = new();
                long unitsPerWhole = 4L * subdivision;
                int[] denominators = { 1, 2, 4, 8, 16, 32 };
                foreach (int denominator in denominators) {
                    if (unitsPerWhole % denominator != 0) {
                        continue;
                    }

                    long baseUnits = unitsPerWhole / denominator;
                    table.Add((baseUnits, new RhythmRationalDuration(1, denominator, 0)));
                    // Single dot only: dotted values are enough for import and
                    // keep the generated notation readable.
                    if (baseUnits % 2 == 0) {
                        table.Add((baseUnits * 3 / 2, new RhythmRationalDuration(1, denominator, 1)));
                    }
                }

                return table.OrderByDescending(entry => entry.Units).ToList();
            }
        }

        /// <summary>
        /// Chooses the coarsest grid that explains the note attacks. A coarse
        /// grid avoids turning human timing into visual noise, while files with
        /// eighth or sixteenth note motion automatically get a finer grid.
        /// </summary>
        public static int ChooseGridSubdivision(IEnumerable<Note> notes, long ticksPerQuarter,
            TimeSignature timeSignature) {
            return ChooseGridSubdivision(notes, ticksPerQuarter, new[] { timeSignature });
        }

        /// <summary>
        /// Chooses the coarsest grid that explains the note attacks and is
        /// compatible with every time signature of the file.
        /// </summary>
        public static int ChooseGridSubdivision(IEnumerable<Note> notes, long ticksPerQuarter,
            IEnumerable<TimeSignature> timeSignatures) {
            if (ticksPerQuarter <= 0) {
                throw new ArgumentOutOfRangeException(nameof(ticksPerQuarter));
            }

            List<TimeSignature> signatures = timeSignatures.ToList();
            List<long> onsets = notes.Select(note => note.Start).Distinct().ToList();
            List<int> candidates = GridCandidates
                .Where(subdivision => signatures.All(signature =>
                    IsCompatibleWithBeat(subdivision, ticksPerQuarter, signature)))
                .ToList();
            if (candidates.Count == 0) {
                candidates.Add(DefaultGridSubdivision);
            }

            if (onsets.Count > 0) {
                foreach (int subdivision in candidates) {
                    double gridTicks = (double)ticksPerQuarter / subdivision;
                    double tolerance = gridTicks * AlignmentTolerance;
                    int aligned = onsets.Count(onset => DistanceToGrid(onset, gridTicks) <= tolerance);
                    if (aligned >= onsets.Count * AlignmentRatio) {
                        return subdivision;
                    }
                }
            }

            return candidates[^1];
        }

        /// <summary>
        /// Quantizes a staff and merges simultaneous notes into chords. Note
        /// ends are snapped to the grid, with a minimum duration of one grid
        /// step. Segments are truncated at the next attack so that the result
        /// can be represented by a single melodic rhythm line.
        /// </summary>
        public static StaffQuantization Quantize(IEnumerable<Note> notes, long ticksPerQuarter, int gridSubdivision) {
            if (ticksPerQuarter <= 0) {
                throw new ArgumentOutOfRangeException(nameof(ticksPerQuarter));
            }
            if (!GridCandidates.Contains(gridSubdivision)) {
                throw new ArgumentOutOfRangeException(nameof(gridSubdivision));
            }

            double gridTicks = (double)ticksPerQuarter / gridSubdivision;
            Dictionary<long, NoteGroup> groups = new();
            foreach (Note note in notes) {
                long startUnit = Math.Max(0, ToUnit(note.Start, gridTicks));
                long endUnit = Math.Max(startUnit + 1, ToUnit(note.End, gridTicks));
                if (groups.TryGetValue(startUnit, out NoteGroup? group)) {
                    group.EndUnit = Math.Max(group.EndUnit, endUnit);
                    group.Pitches.Add(note.Pitch);
                } else {
                    group = new NoteGroup(startUnit, endUnit);
                    group.Pitches.Add(note.Pitch);
                    groups[startUnit] = group;
                }
            }

            List<NoteGroup> ordered = groups.OrderBy(entry => entry.Key).Select(entry => entry.Value).ToList();
            List<QuantizedNote> quantized = new(ordered.Count);
            for (int i = 0; i < ordered.Count; i++) {
                NoteGroup group = ordered[i];
                if (i + 1 < ordered.Count && group.EndUnit > ordered[i + 1].StartUnit) {
                    group.EndUnit = ordered[i + 1].StartUnit;
                }

                List<int> pitches = group.Pitches.OrderBy(pitch => pitch).ToList();
                quantized.Add(new QuantizedNote(group.StartUnit, group.EndUnit, pitches));
            }

            return new StaffQuantization(gridSubdivision, gridTicks, quantized);
        }

        /// <summary>
        /// Builds the measure map of the file, applying time signature changes
        /// at bar lines. Changes occurring inside a measure take effect at the
        /// next bar line so that every generated measure keeps a complete
        /// duration.
        /// </summary>
        public static List<MeasureSpan> BuildMeasureSpans(IEnumerable<TimeSignatureChange> changes, double lastTick,
            long ticksPerQuarter, TimeSignature defaultTimeSignature) {
            if (ticksPerQuarter <= 0) {
                throw new ArgumentOutOfRangeException(nameof(ticksPerQuarter));
            }

            List<TimeSignatureChange> ordered = changes
                .Where(change => change.Beats > 0 && change.BeatUnit > 0)
                .OrderBy(change => change.Time)
                .GroupBy(change => change.Time)
                .Select(group => group.First())
                .ToList();

            List<MeasureSpan> spans = new();
            TimeSignature current = defaultTimeSignature;
            double changeTolerance = Math.Max(1.0, ticksPerQuarter / 4.0);
            int changeIndex = 0;
            long cursor = 0;
            while (cursor < lastTick || spans.Count == 0) {
                while (changeIndex < ordered.Count && ordered[changeIndex].Time <= cursor + changeTolerance) {
                    TimeSignatureChange change = ordered[changeIndex];
                    current = new TimeSignature(change.Beats, change.BeatUnit);
                    changeIndex++;
                }

                double measureTicks = ticksPerQuarter * 4.0 * current.Beats / current.BeatUnit;
                if (measureTicks <= 0 || double.IsNaN(measureTicks) || double.IsInfinity(measureTicks)) {
                    break;
                }

                long end = (long)Math.Round(cursor + measureTicks, MidpointRounding.AwayFromZero);
                if (end <= cursor) {
                    end = cursor + 1;
                }

                spans.Add(new MeasureSpan(cursor, end, current));
                cursor = end;
            }

            return spans;
        }

        private static bool IsCompatibleWithBeat(int subdivision, long ticksPerQuarter, TimeSignature timeSignature) {
            RhythmRationalDuration beatDuration = timeSignature.GetBeatDuration().ApplyDots();
            double beatTicks = ticksPerQuarter * 4.0 * beatDuration.Numerator / beatDuration.Denominator;
            double gridTicks = (double)ticksPerQuarter / subdivision;
            double ratio = beatTicks / gridTicks;
            return ratio >= 1.0 - 1e-9 && Math.Abs(ratio - Math.Round(ratio)) < 1e-9;
        }

        private static double DistanceToGrid(double tick, double gridTicks) {
            double units = tick / gridTicks;
            return Math.Abs(units - Math.Round(units)) * gridTicks;
        }

        private static long ToUnit(double ticks, double gridTicks) {
            return (long)Math.Round(ticks / gridTicks, MidpointRounding.AwayFromZero);
        }

        private sealed class NoteGroup {

            public long StartUnit {
                get;
            }

            public long EndUnit {
                get;
                set;
            }

            public HashSet<int> Pitches {
                get;
            } = new();

            public NoteGroup(long startUnit, long endUnit) {
                StartUnit = startUnit;
                EndUnit = endUnit;
            }
        }
    }
}