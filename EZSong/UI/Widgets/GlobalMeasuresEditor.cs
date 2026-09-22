using EZSong.Helpers;
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
    internal class GlobalMeasuresEditor:Box {

        private GlobalSegmentEditor _globalSegmentEditor;

        private Song _currentSong;
        private UserSettings _userSettings;
        private EmbeddedMidiSynth _embeddedMidiSynth;
        private int _displayedSegmentIndex = 0;
        private bool _isSecondary;

        private MeasuresEditorHeader _measuresEditorHeader;

        private MeasuresEditor _measuresEditor;

        private int _displayedStaffIndex = 0;
        internal int DisplayedStaffIndex {
            get {
                return _displayedStaffIndex;
            }
        }

        public bool IsPlaceHolder {
            get {
                return _measuresEditorHeader.IsPlaceHolder || _measuresEditor.IsPlaceHolder;
            }
            internal set {
                bool changed = IsPlaceHolder != value;

                _measuresEditorHeader.IsPlaceHolder = value;
                _measuresEditor.IsPlaceHolder = value;

                if (changed && !value) {
                    // Le changement d'état doit reconstruire le contenu,
                    // sinon le placeholder reste affiché.
                    _measuresEditor.Refresh();

                    // Applique ensuite l'index de portée au header et aux widgets.
                    RefreshDisplayedStaff();
                }
            }
        }

        public GlobalMeasuresEditor(GlobalSegmentEditor globalSegmentEditor, Song currentSong, UserSettings userSettings, EmbeddedMidiSynth embeddedMidiSynth, int displayedSegmentIndex, bool isSecondary) {

            _globalSegmentEditor = globalSegmentEditor;

            _currentSong = currentSong;
            _userSettings = userSettings;
            _embeddedMidiSynth = embeddedMidiSynth;
            _displayedSegmentIndex = displayedSegmentIndex;
            _displayedStaffIndex =
                isSecondary
                    ? _currentSong.SongSettings.StaffsSettings.Staffs.Count - 1
                    : 0;
            _isSecondary = isSecondary;

            if (_currentSong.SongSettings.StaffsSettings.Staffs.Count() == 0) {
                //Ce cas ne devrait pas se produire, mais on le gère quand même
                throw new Exception("Aucune portée définie, on ne peut pas afficher l'éditeur de mesures");
            }
            if (isSecondary) {
                _measuresEditorHeader = new(this, _currentSong, _displayedStaffIndex, true);
                _measuresEditor = new MeasuresEditor(this, userSettings, embeddedMidiSynth, _currentSong, IsPlaceHolder);
            } else {
                _measuresEditorHeader = new(this, _currentSong, _displayedStaffIndex, false);
                _measuresEditor = new MeasuresEditor(this, userSettings, embeddedMidiSynth, _currentSong, false);
            }

            ScrolledWindow scrolled = new();
            scrolled.Add(_measuresEditor);
            PackStart(_measuresEditorHeader, false, false, 0);
            PackStart(scrolled, true, true, 0);

            ShowAll();
        }

        internal void GoToFirstStaff() {
            _displayedStaffIndex = 0;
            RefreshDisplayedStaff();
        }

        internal void GoToStaff(int staffNumber) {
            _displayedStaffIndex = staffNumber - 1;
            RefreshDisplayedStaff();
        }

        internal void GoToLastStaff() {
            _displayedStaffIndex = _currentSong.SongSettings.StaffsSettings.Staffs.Count() - 1;
            RefreshDisplayedStaff();
        }

        internal void GoToNextStaff() {
            _displayedStaffIndex = MathHelper.LoopIndex(_displayedStaffIndex, _currentSong.SongSettings.StaffsSettings.Staffs.Count(), 1);
            RefreshDisplayedStaff();
        }

        internal void GoToPreviousStaff() {
            _displayedStaffIndex = MathHelper.LoopIndex(_displayedStaffIndex, _currentSong.SongSettings.StaffsSettings.Staffs.Count(), -1);
            RefreshDisplayedStaff();
        }

        private void RefreshDisplayedStaff() {
            if (_measuresEditorHeader.IsPlaceHolder || _measuresEditor.IsPlaceHolder) {
                return;
            }
            _measuresEditorHeader.RefreshDisplayedStaff(_displayedStaffIndex);
            _measuresEditor.RefreshDisplayedStaff(_displayedStaffIndex);
        }
        
        internal void Clear() {
            foreach (Widget child in Children.ToArray()) {
                Remove(child);
                child.Dispose();
            }
        }

        internal void SetSong(Song currentSong) {
            _currentSong = currentSong;

            _displayedStaffIndex =
               _isSecondary
                   ? _currentSong.SongSettings.StaffsSettings.Staffs.Count - 1
                   : 0;

            Clear();

            if (_currentSong.SongSettings.StaffsSettings.Staffs.Count() == 0) {
                return; //Aucune portée définie, on ne peut pas afficher l'éditeur de mesures
            }
            if (_isSecondary) {

                if (_currentSong.SongSettings.StaffsSettings.Staffs.Count() < 2) {
                    _measuresEditorHeader = new MeasuresEditorHeader(this, _currentSong, _displayedStaffIndex, true); 
                    _measuresEditor = new MeasuresEditor(this, _userSettings, _embeddedMidiSynth, _currentSong, true);
                } else {
                    _measuresEditorHeader = new MeasuresEditorHeader(this, _currentSong, _displayedStaffIndex, false);
                    _measuresEditor = new MeasuresEditor(this, _userSettings, _embeddedMidiSynth, _currentSong, false);
                }
            } else {
                _measuresEditorHeader = new MeasuresEditorHeader(this, _currentSong, _displayedStaffIndex, false);
                _measuresEditor = new MeasuresEditor(this, _userSettings, _embeddedMidiSynth, _currentSong, false);
            }

            ScrolledWindow scrolled = new();
            scrolled.Add(_measuresEditor);
            PackStart(_measuresEditorHeader, false, false, 0);
            PackStart(scrolled, true, true, 0);

            ShowAll();
        }

        internal void Refresh() {
            if (!IsPlaceHolder) {
                _measuresEditorHeader.RefreshDisplayedStaff(_displayedStaffIndex);
            }
            _measuresEditor.Refresh();
        }

        internal void RefreshDisplayedSegment(int displayedSegmentIndex, bool isPlaceHolder) {
            _displayedSegmentIndex = displayedSegmentIndex;
            _measuresEditorHeader.IsPlaceHolder = isPlaceHolder;
            _measuresEditor.IsPlaceHolder = isPlaceHolder;
            _measuresEditor.RefreshDisplayedSegment(displayedSegmentIndex);
            if (!isPlaceHolder) {
                RefreshDisplayedStaff();
            }
        }

        internal MelodyMeasureEditor? GetFocusedMelodyMeasureEditor() {
            if (_measuresEditor.IsPlaceHolder) {
                return null;
            }
            return _measuresEditor.GetFocusedMelodyMeasureEditor();
        }

        internal void AddMeasure(MeasureData measure) {
            if (IsPlaceHolder) {
                return;
            }

            _measuresEditor.AddMeasure(CreateMeasureEditorWidget(measure));
        }

        internal MeasureEditorWidget CreateMeasureEditorWidget(
    MeasureData measure) {
            MeasureEditorWidget widget =
                new(measure, _userSettings, _embeddedMidiSynth, _displayedSegmentIndex, _displayedStaffIndex);

            widget.WidthRequest = 200;
            // TODO : Ajuster la largeur en fonction du nombre
            // de portées et de la signature rythmique.

            widget.MeasureChanged += (MeasureData changedMeasure) =>
            {
                _globalSegmentEditor.NotifyMeasureChanged(
                    changedMeasure,
                    this);
            };

            widget.StaffChanged += (MeasureData changedMeasure, int staffIndex) =>
            {
                _globalSegmentEditor.NotifyStaffChanged(
                    changedMeasure,
                    staffIndex,
                    this);
            };

            widget.InsertAfterRequested += (MeasureData requestedMeasure) =>
            {
                _globalSegmentEditor.InsertAfter(requestedMeasure);
            };

            widget.InsertBeforeRequested += (MeasureData requestedMeasure) =>
            {
                _globalSegmentEditor.InsertBefore(requestedMeasure);
            };

            widget.DeleteRequested += (MeasureData requestedMeasure) =>
            {
                _globalSegmentEditor.Delete(requestedMeasure);
            };

            return widget;
        }

        internal void Reindex() {
            _measuresEditor.Reindex();
        }

        internal void RefreshMeasure(MeasureData measure) {
            if (IsPlaceHolder) {
                return;
            }

            _measuresEditor.RefreshMeasure(measure);
        }

        internal void RefreshMeasureStaff(MeasureData measure, int staffIndex) {
            if (IsPlaceHolder) {
                return;
            }

            if (_displayedStaffIndex != staffIndex) {
                return;
            }

            _measuresEditor.RefreshMeasureStaff(measure);
        }
    }
}
