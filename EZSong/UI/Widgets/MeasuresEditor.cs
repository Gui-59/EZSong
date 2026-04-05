using EZSong.Enums;
using EZSong.Model;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets {
    public class MeasuresEditor : ScrolledWindow {
        private Box _container;
        private Song _song;

        public event System.Action? MeasuresChanged;

        private List<MelodyMeasureEditor> _melodyMeasureEditorWidgets;

        public MeasuresEditor() {
            _song = new Song();
            _melodyMeasureEditorWidgets = new();
            _container = new Box(Orientation.Horizontal, 4);
            Add(_container);
        }

        public void SetSong(Song song) {
            _song = song;
            Refresh();
        }

        public void Refresh() {
            Clear();

            foreach (MeasureData measure in _song.Measures) {
                AddMeasure(measure);
            }

            ShowAll();
        }

        public void Clear() {
            foreach (Widget? child in _container.Children) {
                _container.Remove(child);
            }
        }

        public void AddMeasure(MeasureData measure) {
            MeasureEditorWidget widget = new();

            widget.SetMeasure(measure);
            _melodyMeasureEditorWidgets.Add(widget.MelodyMeasureEditor);

            widget.MeasureChanged += () => {
                MeasuresChanged?.Invoke();
            };

            widget.InsertAfterRequested += () => {
                InsertAfter(measure);
            };

            widget.InsertBeforeRequested += () => {
                InsertBefore(measure);
            };

            widget.DeleteRequested += () => {
                Delete(measure);
            };

            

            _container.PackStart(widget, false, false, 2);
        }

        private void InsertAfter(MeasureData measure) {
            int index = _song.Measures.IndexOf(measure);

            MeasureData newMeasure = CreateEmptyMeasure(index + 1, measure.TimeSignature);

            _song.Measures.Insert(index + 1, newMeasure);
            Reindex();

            Refresh();
        }

        private void InsertBefore(MeasureData measure) {
            int index = _song.Measures.IndexOf(measure);

            MeasureData newMeasure = CreateEmptyMeasure(index, measure.TimeSignature);

            _song.Measures.Insert(index, newMeasure);
            Reindex();

            Refresh();
        }

        private void Delete(MeasureData measure) {
            if (_song.Measures.Count <= 1) {
                return;
            }

            _ = _song.Measures.Remove(measure);
            Reindex();

            Refresh();
        }

        private void Reindex() {
            for (int i = 0; i < _song.Measures.Count; i++) {
                _song.Measures[i].Index = i + 1;
            }
        }

        private MeasureData CreateEmptyMeasure(int index, TimeSignature ts) {
            return new MeasureData(
                index,
                ts,
                new KeySignature(NoteStep.C, Alteration.neutral, SongMode.major),
                new ChordSequence(""),
                new MeasureMelody(new List<MelodyChord>(), new MeasureRhythmPattern(ts)),
                "",
                new MeasureRhythmPattern(ts)
            );
        }

        public MelodyMeasureEditor? GetFocusedMelodyEditor() {
            foreach (MelodyMeasureEditor melodyMeasureEditorWidget in _melodyMeasureEditorWidgets) {
                if (melodyMeasureEditorWidget.HasFocus) {
                    return melodyMeasureEditorWidget;
                }
            }

            return null;
        }
    }
}
