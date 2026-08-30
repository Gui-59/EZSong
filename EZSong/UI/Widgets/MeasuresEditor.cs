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

        private int _staffIndex;
        
        private int _segmentIndex;

        public MeasuresEditor() {
            _song = new Song();
            _container = new Box(Orientation.Horizontal, 4);
            Add(_container);
        }

        public void SetSong(Song song) {
            _song = song;
            Refresh();
        }

        public void Refresh() {

            Clear();

            Reindex();

            //On doit considérer ici uniquement le segment actif
            List<MeasureData> measures = _song.Segments[_segmentIndex].Measures;
            for (int i = 0; i < measures.Count; i++) {
                if (measures[i] is null) {
                    return;
                }
                AddMeasure(measures[i]);
            }

            ShowAll();
        }

        internal void RefreshDisplayedSegment(int segmentIndex) {
            _segmentIndex = segmentIndex;
            Refresh();
        }

        internal void RefreshDisplayedStaff(int staffIndex) {
            _staffIndex = staffIndex;
            foreach (MeasureEditorWidget measureEditorWidget in _container.Children) {
                measureEditorWidget.RefreshDisplayedStaff(staffIndex);
            }            
            ShowAll();
        }

        public void Clear() {
            foreach (Widget? child in _container.Children) {
                _container.Remove(child);
            }
        }

        public void AddMeasure(MeasureData measure) {

            MeasureEditorWidget widget = new(measure);

            widget.WidthRequest = 200; //TODO : Ajuster la largeur en fonction du nombre de portées et de la signature rythmique

            widget.MeasureChanged += (MeasureData measure) => {
                int index = _song.Segments[_segmentIndex].Measures.IndexOf(measure);
            };

            widget.InsertAfterRequested += (MeasureData measure) => {
                InsertAfter(measure);
            };

            widget.InsertBeforeRequested += (MeasureData measure) => {
                InsertBefore(measure);
            };

            widget.DeleteRequested += (MeasureData measure) => {
                Delete(measure);
            };

            _container.PackStart(widget, false, false, 2);
        }

        public void AppendBlankMeasures(int number) {
            for (int i = 0; i < number; i++) {
                MeasureData newMeasure = CreateEmptyMeasure(i, new TimeSignature());
                _song.Segments[_segmentIndex].Measures.Insert(i, newMeasure);
            }
            Reindex();
            Refresh();
        }

        private void InsertAfter(MeasureData measure) {
            int index = _song.Segments[_segmentIndex].Measures.IndexOf(measure);
            MeasureData newMeasure = CreateEmptyMeasure(index + 1, measure.TimeSignature);
            _song.Segments[_segmentIndex].Measures.Insert(index + 1, newMeasure);
            Reindex();
            Refresh();
        }

        private void InsertBefore(MeasureData measure) {
            int index = _song.Segments[_segmentIndex].Measures.IndexOf(measure);
            MeasureData newMeasure = CreateEmptyMeasure(index, measure.TimeSignature);
            _song.Segments[_segmentIndex].Measures.Insert(index, newMeasure);
            Reindex();
            Refresh();
        }

        private void Delete(MeasureData measure) {
            if (_song.Segments[_segmentIndex].Measures.Count <= 1) {
                return;
            }
            _ = _song.Segments[_segmentIndex].Measures.Remove(measure);
            Reindex();
            Refresh();
        }

        private void Reindex() {

            for (int i = 0; i < _song.Segments[_segmentIndex].Measures.Count; i++) {
                _song.Segments[_segmentIndex].Measures[i].Index = i + 1;

                if (i > 0) {
                    _song.Segments[_segmentIndex].Measures[i].PrecedingMeasure = _song.Segments[_segmentIndex].Measures[i - 1];
                } else {
                    _song.Segments[_segmentIndex].Measures[i].PrecedingMeasure = null;
                }

                if (i < _song.Segments[_segmentIndex].Measures.Count - 1) {
                    _song.Segments[_segmentIndex].Measures[i].FollowingMeasure = _song.Segments[_segmentIndex].Measures[i + 1];
                } else {
                    _song.Segments[_segmentIndex].Measures[i].FollowingMeasure = null;
                }
            }
        }

        private MeasureData CreateEmptyMeasure(int index, TimeSignature ts) {
            List<MeasureGlobalMelody> staffs = new();
            staffs.Add(new MeasureGlobalMelody(0)); //Toujours au moins une portée
            //TODO : S'assurer d'jouter le bon nombre de portées
            return new MeasureData(
                index,
                _song.SongSettings,
                ts,
                new KeySignature(NoteStep.C, Alteration.neutral, SongMode.major),
                new ChordSequence(),
                staffs,
                ""
            );
        }

        public MelodyMeasureEditor? GetFocusedGlobalMelodyEditor() {
            foreach (GlobalMelodyEditor globalMelodyEditorWidget in _container.Children) {
                if (globalMelodyEditorWidget.HasFocus) {
                    return globalMelodyEditorWidget.MelodyMeasureEditor; 
                }
            }
            return null;
        }
    }
}
