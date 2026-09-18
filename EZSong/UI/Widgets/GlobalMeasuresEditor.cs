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



        public GlobalMeasuresEditor(GlobalSegmentEditor globalSegmentEditor, Song currentSong, UserSettings userSettings, EmbeddedMidiSynth embeddedMidiSynth, int displayedSegmentIndex, bool isSecondary) {

            _globalSegmentEditor = globalSegmentEditor;

            _currentSong = currentSong;
            _userSettings = userSettings;
            _embeddedMidiSynth = embeddedMidiSynth;
            _displayedSegmentIndex = displayedSegmentIndex;
            _isSecondary = isSecondary;

            if (_currentSong.SongSettings.StaffsSettings.Staffs.Count() == 0) {
                //Ce cas ne devrait pas se produire, mais on le gère quand même
                throw new Exception("Aucune portée définie, on ne peut pas afficher l'éditeur de mesures");
            }
            if (isSecondary) {
                if (_currentSong.SongSettings.StaffsSettings.Staffs.Count() == 1) {
                    _measuresEditorHeader = new(this, _currentSong, true);
                    _measuresEditor = new MeasuresEditor(_globalSegmentEditor, userSettings, embeddedMidiSynth, _currentSong, true);
                } else {
                    _measuresEditorHeader = new(this, _currentSong, false);
                    _measuresEditor = new MeasuresEditor(_globalSegmentEditor, userSettings, embeddedMidiSynth, _currentSong, false);
                }
            } else {
                _measuresEditorHeader = new(this, _currentSong, false);
                _measuresEditor = new MeasuresEditor(_globalSegmentEditor, userSettings, embeddedMidiSynth, _currentSong, false);
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

            Clear();

            if (_currentSong.SongSettings.StaffsSettings.Staffs.Count() == 0) {
                return; //Aucune portée définie, on ne peut pas afficher l'éditeur de mesures
            }
            if (_isSecondary) {

                if (_currentSong.SongSettings.StaffsSettings.Staffs.Count() < 2) {
                    _measuresEditorHeader = new MeasuresEditorHeader(this, _currentSong, true); 
                    _measuresEditor = new MeasuresEditor(_globalSegmentEditor, _userSettings, _embeddedMidiSynth, _currentSong, true);
                } else {
                    _measuresEditorHeader = new MeasuresEditorHeader(this, _currentSong, false);
                    _measuresEditor = new MeasuresEditor(_globalSegmentEditor, _userSettings, _embeddedMidiSynth, _currentSong, false);
                }
            } else {
                _measuresEditorHeader = new MeasuresEditorHeader(this, _currentSong, false);
                _measuresEditor = new MeasuresEditor(_globalSegmentEditor, _userSettings, _embeddedMidiSynth, _currentSong, false);
            }

            ScrolledWindow scrolled = new();
            scrolled.Add(_measuresEditor);
            PackStart(_measuresEditorHeader, false, false, 0);
            PackStart(scrolled, true, true, 0);

            ShowAll();
        }

        internal void Refresh() {
            if (_measuresEditor.IsPlaceHolder) {
                return;
            }
            _measuresEditorHeader.RefreshDisplayedStaff(_displayedStaffIndex);
            _measuresEditor.Refresh();
        }

        internal void RefreshDisplayedSegment(int displayedSegmentIndex) {
            _displayedSegmentIndex = displayedSegmentIndex;
            if (_measuresEditor.IsPlaceHolder) {
                return;
            }
            _measuresEditor.RefreshDisplayedSegment(displayedSegmentIndex);
        }

        internal MelodyMeasureEditor? GetFocusedMelodyMeasureEditor() {
            if (_measuresEditor.IsPlaceHolder) {
                return null;
            }
            return _measuresEditor.GetFocusedMelodyMeasureEditor();
        }

        internal void AddMeasure(GlobalSegmentEditor globalSegmentEditor , MeasureData measure) {
            MeasureEditorWidget widget = new(measure, _userSettings, _embeddedMidiSynth);

            widget.WidthRequest = 200; //TODO : Ajuster la largeur en fonction du nombre de portées et de la signature rythmique

            widget.MeasureChanged += (MeasureData measure) => {
                int index = _currentSong.Segments[_displayedSegmentIndex].Measures.IndexOf(measure);
            };

            widget.InsertAfterRequested += (MeasureData measure) => {
                globalSegmentEditor.InsertAfter(measure);
            };

            widget.InsertBeforeRequested += (MeasureData measure) => {
                globalSegmentEditor.InsertBefore(measure);
            };

            widget.DeleteRequested += (MeasureData measure) => {
                globalSegmentEditor.Delete(measure);
            };

            _measuresEditor.AddMeasure(widget);
        }

        internal void Reindex() {
            _measuresEditor.Reindex();
        }
    }
}
