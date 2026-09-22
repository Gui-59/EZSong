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

        private GlobalMeasuresEditor _globalMeasuresEditor;

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

        public MeasuresEditor(GlobalMeasuresEditor globalMeasuresEditor, UserSettings userSettings, EmbeddedMidiSynth embeddedMidiSynth, Song currentSong, bool isPlaceHolder) {

            _globalMeasuresEditor = globalMeasuresEditor;
            _userSettings = userSettings;
            _embeddedMidiSynth = embeddedMidiSynth;
            _isPlaceHolder = isPlaceHolder;
            _song = currentSong;
            _measuresWidgetsBox = new Box(Orientation.Horizontal, 0);

            //Refresh();
        }

        private void InitializeComponent() {
            Clear();


            if (_isPlaceHolder) {
                Label placeholderLabel = new("Créez une deuxième portée pour la voir ici");
                placeholderLabel.StyleContext.AddClass("titleLabel");
                Add(placeholderLabel);
            } else {
                Add(_measuresWidgetsBox);
            }

            ShowAll();

        }

        public void Refresh() {
            InitializeComponent();

            if (_isPlaceHolder) {
                ShowAll();
                return;
            }

            Reindex();

            // On doit considérer ici uniquement le segment actif.
            List<MeasureData> measures =
                _song.Segments[_segmentIndex].Measures;

            for (int i = 0; i < measures.Count; i++) {
                if (measures[i] is null) {
                    continue;
                }

                MeasureEditorWidget widget =
                    _globalMeasuresEditor.CreateMeasureEditorWidget(measures[i]);

                AddMeasure(widget);
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
            // Supprime les widgets de mesures contenus dans le box,
            // mais conserve le box lui-même pour le réutiliser.
            foreach (Widget child in _measuresWidgetsBox.Children.ToArray()) {
                if (child is MeasureEditorWidget measureEditor) {
                    measureEditor.DisposeEditors();
                }

                _measuresWidgetsBox.Remove(child);
                child.Dispose();
            }

            // Supprime tous les enfants directs de MeasuresEditor.
            // Le _measuresWidgetsBox est conservé en mémoire, mais retiré
            // du parent comme les autres widgets.
            foreach (Widget child in Children.ToArray()) {
                Remove(child);

                if (child != _measuresWidgetsBox) {
                    child.Dispose();
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

            if (_isPlaceHolder) { //Ce garde-fou évite de construire des widgets dans un conteneur qui n'est pas affiché.
                widget.DisposeEditors();
                widget.Dispose();
                return;
            }

            _measuresWidgetsBox.PackStart(widget, true, false, 0);
        }

        internal void RefreshMeasure(MeasureData measure) {
            foreach (MeasureEditorWidget widget in
                     _measuresWidgetsBox.Children) {
                if (ReferenceEquals(widget.Measure, measure)) {
                    widget.RefreshSharedFromModel();
                    return;
                }
            }
        }

        internal void RefreshMeasureStaff(MeasureData measure) {
            foreach (MeasureEditorWidget widget in
                     _measuresWidgetsBox.Children) {
                if (ReferenceEquals(widget.Measure, measure)) {
                    widget.RefreshStaffFromModel();
                    return;
                }
            }
        }
    }
}
