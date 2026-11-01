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

    /// <summary>
    /// A widget that displays and manages the measures of a song segment, 
    /// allowing for editing and interaction with individual measures.
    /// </summary>
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

        /// <summary>
        /// Determines whether this GlobalMeasuresEditor instance is a placeholder, which affects its behavior and display.
        /// </summary>
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

        /// <summary>
        /// Creates a new instance of the GlobalMeasuresEditor widget, 
        /// initializing it with the provided parameters and setting up its internal structure.
        /// </summary>
        /// <param name="globalSegmentEditor">The parent GlobalSegmentEditor instance.</param>
        /// <param name="currentSong">The current song being edited.</param>
        /// <param name="userSettings">The user settings to be applied.</param>
        /// <param name="embeddedMidiSynth">The embedded MIDI synthesizer for playback.</param>
        /// <param name="displayedSegmentIndex">The index of the segment to be displayed.</param>
        /// <param name="isSecondary">Indicates whether this editor is for a secondary staff.</param>
        /// <exception cref="Exception"></exception>
        public GlobalMeasuresEditor(
            GlobalSegmentEditor globalSegmentEditor, 
            Song currentSong, 
            UserSettings userSettings, 
            EmbeddedMidiSynth embeddedMidiSynth, 
            int displayedSegmentIndex, 
            bool isSecondary
        ) {

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

        /// <summary>
        /// Goes to the first staff in the current song, updating the displayed staff index and refreshing the display accordingly.
        /// </summary>
        internal void GoToFirstStaff() {
            _displayedStaffIndex = 0;
            RefreshDisplayedStaff();
        }

        /// <summary>
        /// Goes to the specified staff number in the current song, updating the displayed staff index and refreshing the display accordingly.
        /// </summary>
        /// <param name="staffNumber">The number of the staff to go to.</param>
        internal void GoToStaff(int staffNumber) {
            _displayedStaffIndex = staffNumber - 1;
            RefreshDisplayedStaff();
        }

        /// <summary>
        /// Goes to the last staff in the current song, updating the displayed staff index and refreshing the display accordingly.
        /// </summary>
        internal void GoToLastStaff() {
            _displayedStaffIndex = _currentSong.SongSettings.StaffsSettings.Staffs.Count() - 1;
            RefreshDisplayedStaff();
        }

        /// <summary>
        /// Goes to the next staff in the current song, updating the displayed staff index and refreshing the display accordingly.
        /// </summary>
        internal void GoToNextStaff() {
            _displayedStaffIndex = MathHelper.LoopIndex(_displayedStaffIndex, _currentSong.SongSettings.StaffsSettings.Staffs.Count(), 1);
            RefreshDisplayedStaff();
        }

        /// <summary>
        /// Goes to the previous staff in the current song, updating the displayed staff index and refreshing the display accordingly.
        /// </summary>
        internal void GoToPreviousStaff() {
            _displayedStaffIndex = MathHelper.LoopIndex(_displayedStaffIndex, _currentSong.SongSettings.StaffsSettings.Staffs.Count(), -1);
            RefreshDisplayedStaff();
        }

        /// <summary>
        /// Refreshes the displayed staff in both the MeasuresEditorHeader and MeasuresEditor widgets,
        /// </summary>
        private void RefreshDisplayedStaff() {
            if (_measuresEditorHeader.IsPlaceHolder || _measuresEditor.IsPlaceHolder) {
                return;
            }
            _measuresEditorHeader.RefreshDisplayedStaff(_displayedStaffIndex);
            _measuresEditor.RefreshDisplayedStaff(_displayedStaffIndex);
        }

        /// <summary>
        /// Clears all child widgets from the GlobalMeasuresEditor, 
        /// removing them from the container and disposing of them to free resources.
        /// </summary>
        internal void Clear() {
            foreach (Widget child in Children.ToArray()) {
                Remove(child);
                child.Dispose();
            }
        }

        /// <summary>
        /// Sets the current song for the GlobalMeasuresEditor, 
        /// initializing the MeasuresEditorHeader and MeasuresEditor widgets based on the song's settings and the specified staff index.
        /// </summary>
        /// <param name="currentSong"></param>
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

        /// <summary>
        /// Refreshes the MeasuresEditor and MeasuresEditorHeader widgets,
        /// </summary>
        internal void Refresh() {
            if (!IsPlaceHolder) {
                _measuresEditorHeader.RefreshDisplayedStaff(_displayedStaffIndex);
            }
            _measuresEditor.Refresh();
        }

        /// <summary>
        /// Refreshes the displayed segment in both the MeasuresEditorHeader and MeasuresEditor widgets,
        /// </summary>
        /// <param name="displayedSegmentIndex">The index of the segment to be displayed.</param>
        /// <param name="isPlaceHolder">Indicates whether the segment is a placeholder.</param>
        internal void RefreshDisplayedSegment(int displayedSegmentIndex, bool isPlaceHolder) {
            _displayedSegmentIndex = displayedSegmentIndex;
            _measuresEditorHeader.IsPlaceHolder = isPlaceHolder;
            _measuresEditor.IsPlaceHolder = isPlaceHolder;
            _measuresEditor.RefreshDisplayedSegment(displayedSegmentIndex);
            if (!isPlaceHolder) {
                RefreshDisplayedStaff();
            }
        }

        /// <summary>
        /// Gets the currently focused MelodyMeasureEditor, if any, from the MeasuresEditor.
        /// </summary>
        /// <returns>The currently focused MelodyMeasureEditor, or null if there is none.</returns>
        internal MelodyMeasureEditor? GetFocusedMelodyMeasureEditor() {
            if (_measuresEditor.IsPlaceHolder) {
                return null;
            }
            return _measuresEditor.GetFocusedMelodyMeasureEditor();
        }

        /// <summary>
        /// Adds a new measure to the MeasuresEditor, 
        /// creating a corresponding MeasureEditorWidget and adding it to the display.
        /// </summary>
        /// <param name="measure">The measure to be added.</param>
        internal void AddMeasure(MeasureData measure) {
            if (IsPlaceHolder) {
                return;
            }

            _measuresEditor.AddMeasure(CreateMeasureEditorWidget(measure));
        }

        /// <summary>
        /// Creates a new MeasureEditorWidget for the specified measure,
        /// </summary>
        /// <param name="measure">The measure for which to create the widget.</param>
        /// <returns>The created MeasureEditorWidget.</returns>
        internal MeasureEditorWidget CreateMeasureEditorWidget(MeasureData measure) {
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

        /// <summary>
        /// Reindexes the measures in the MeasuresEditor, updating their order and display based on the current state of the song segment.
        /// </summary>
        internal void Reindex() {
            _measuresEditor.Reindex();
        }

        /// <summary>
        /// Refreshes the specified measure in the MeasuresEditor, updating its display and state based on any changes made to the measure data.
        /// </summary>
        /// <param name="measure"></param>
        internal void RefreshMeasure(MeasureData measure) {
            if (IsPlaceHolder) {
                return;
            }

            _measuresEditor.RefreshMeasure(measure);
        }

        /// <summary>
        /// Refreshes the specified measure for the given staff index in the MeasuresEditor, 
        /// updating its display and state based on any changes made to the measure data.
        /// </summary>
        /// <param name="measure">The measure to be refreshed.</param>
        /// <param name="staffIndex">The index of the staff to be refreshed.</param>
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
