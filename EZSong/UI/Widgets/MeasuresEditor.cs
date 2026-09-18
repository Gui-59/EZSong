using EZSong.Enums;
using EZSong.MIDI;
using EZSong.Model;
using EZSong.Settings;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets {
    internal class MeasuresEditor : Box {

        private GlobalSegmentEditor _globalSegmentEditor;

        private UserSettings _userSettings;

        private EmbeddedMidiSynth _embeddedMidiSynth; //Pour echo MIDI

        private bool _isPlaceHolder;

        private Box _measuresWidgetsBox;

        private Song _song;

        private int _staffIndex;
        
        private int _segmentIndex;

        public bool IsPlaceHolder {
            get {
                return _isPlaceHolder;
            }
            internal set {
                _isPlaceHolder = value;
            }
        }

        public MeasuresEditor(GlobalSegmentEditor globalSegmentEditor, UserSettings userSettings, EmbeddedMidiSynth embeddedMidiSynth, Song currentSong, bool isPlaceHolder) {

            _globalSegmentEditor = globalSegmentEditor;

            _userSettings = userSettings;
            _embeddedMidiSynth = embeddedMidiSynth;
            _isPlaceHolder = isPlaceHolder;

            _song = currentSong;

            _measuresWidgetsBox = new Box(Orientation.Horizontal, 0);

            Refresh();
        }

        private void ResetContent() {
            Clear();


            if (_isPlaceHolder) {
                Label placeholderLabel = new("Créez une deuxième portée pour la voir ici");
                placeholderLabel.StyleContext.AddClass("titleLabel");
                Add(placeholderLabel);
            } else {

                _measuresWidgetsBox = new Box(Orientation.Horizontal, 0);
                Add(_measuresWidgetsBox);
            }

        }

        public void Refresh() {

            ResetContent(); 

            Reindex();

            //On doit considérer ici uniquement le segment actif
            List<MeasureData> measures = _song.Segments[_segmentIndex].Measures;
            for (int i = 0; i < measures.Count; i++) {
                if (measures[i] is null) {
                    return;
                }
                _globalSegmentEditor.AddMeasure(measures[i]);
            }

            ShowAll();
        }

        internal void RefreshDisplayedSegment(int segmentIndex) {
            _segmentIndex = segmentIndex;
            Refresh();
        }

        internal void RefreshDisplayedStaff(int staffIndex) {
            _staffIndex = staffIndex;
            foreach (MeasureEditorWidget measureEditorWidget in _measuresWidgetsBox.Children) {
                measureEditorWidget.RefreshDisplayedStaff(staffIndex);
            }            
            ShowAll();
        }

        public void Clear() {
            foreach (Widget child in _measuresWidgetsBox.Children.ToArray()) {
                if (child is MeasureEditorWidget measureEditor) {
                    measureEditor.DisposeEditors();
                    _measuresWidgetsBox.Remove(measureEditor);
                    measureEditor.Dispose();
                }
            }
        }

        internal void Reindex() {

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

        public MelodyMeasureEditor? GetFocusedMelodyMeasureEditor() {
            foreach (MeasureEditorWidget measureEditor in _measuresWidgetsBox.Children) {
                if (measureEditor.GlobalMelodyEditor.MelodyMeasureEditor.HasFocus) {
                    return measureEditor.GlobalMelodyEditor.MelodyMeasureEditor;
                }
            }

            return null;
        }

        internal void AddMeasure(MeasureEditorWidget widget) {
            _measuresWidgetsBox.PackStart(widget, true, false, 0);
        }
    }
}
