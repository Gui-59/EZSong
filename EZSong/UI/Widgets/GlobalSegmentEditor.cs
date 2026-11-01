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

    /// <summary>
    /// A widget that manages and displays the measures of a song segment across multiple staffs,
    /// allowing for editing and interaction with the measures and their associated chords.
    /// </summary>
    internal class GlobalSegmentEditor:Box {

        private int _displayedSegmentIndex = 0;
        public int DisplayedSegmentIndex {
            get {
                return _displayedSegmentIndex;
            }
            set {
                _displayedSegmentIndex = value;
                RefreshDisplayedSegment();
            }
        }

        private Song _currentSong;
        private UserSettings _userSettings;
        private EmbeddedMidiSynth _embeddedMidiSynth;

        private GlobalMeasuresEditor _globalMeasuresEditor1;
        private GlobalMeasuresEditor _globalMeasuresEditor2;


        /// <summary>
        /// Creates a new instance of the GlobalSegmentEditor widget, 
        /// initializing it with the provided parameters 
        /// and setting up its internal structure to manage and display measures across multiple staffs.
        /// </summary>
        /// <param name="currentSong">The song to be edited and displayed.</param>
        /// <param name="userSettings">The user settings to be applied.</param>
        /// <param name="embeddedMidiSynth">The embedded MIDI synthesizer for playback.</param>
        public GlobalSegmentEditor(Song currentSong, UserSettings userSettings, EmbeddedMidiSynth embeddedMidiSynth) : base(Orientation.Vertical, 0) {

            _currentSong = currentSong;
            _userSettings = userSettings;
            _embeddedMidiSynth = embeddedMidiSynth;

            _globalMeasuresEditor1 = new(this, currentSong, userSettings, embeddedMidiSynth, _displayedSegmentIndex, false);
            PackStart(_globalMeasuresEditor1, true, true, 0);

            _globalMeasuresEditor2 = new(this, currentSong, userSettings, embeddedMidiSynth, _displayedSegmentIndex, true);
            PackStart(_globalMeasuresEditor2, true, true, 0);

            _globalMeasuresEditor1.Refresh(); // Appel différé, après que le champ du parent soit assigné
            _globalMeasuresEditor2.Refresh(); // Appel différé, après que le champ du parent soit assigné

            ShowAll();
        }

        /// <summary>
        /// Gets the currently focused MelodyMeasureEditor, 
        /// if any, 
        /// from the two GlobalMeasuresEditors.
        /// </summary>
        /// <returns>The currently focused MelodyMeasureEditor, if any; otherwise, null.</returns>
        internal MelodyMeasureEditor? GetFocusedMelodyMeasureEditor() {
            if (_globalMeasuresEditor1.IsFocus) {
                return _globalMeasuresEditor1.GetFocusedMelodyMeasureEditor();
            } else if (_globalMeasuresEditor2.IsFocus) {
                return _globalMeasuresEditor2.GetFocusedMelodyMeasureEditor();
            }
            return null;
        }

        /// <summary>
        /// Refreshes the display of both GlobalMeasuresEditors,
        /// </summary>
        internal void Refresh() {
            _globalMeasuresEditor1.Refresh();
            _globalMeasuresEditor2.Refresh();
        }

        /// <summary>
        /// Refreshes the displayed segment in both GlobalMeasuresEditors,
        /// </summary>
        internal void RefreshDisplayedSegment() {

            EnsureDisplayedSegmentStaffs();

            bool useSecondary = false;
            if (_currentSong.SongSettings.StaffsSettings.Staffs.Count > 1) {
                useSecondary = true;
            }

            _globalMeasuresEditor1.RefreshDisplayedSegment(_displayedSegmentIndex, false);
            _globalMeasuresEditor2.RefreshDisplayedSegment(_displayedSegmentIndex, !useSecondary);
        }

        /// <summary>
        /// Resets the displayed staffs in both GlobalMeasuresEditors to their initial state,
        /// </summary>
        internal void ResetDisplayedStaffs() {
            _globalMeasuresEditor1.GoToStaff(1);
            _globalMeasuresEditor2.GoToStaff(2);
        }

        /// <summary>
        /// Sets the current song for both GlobalMeasuresEditors, 
        /// allowing them to display and edit the measures of the specified song.
        /// </summary>
        /// <param name="currentSong"></param>
        internal void SetSong(Song currentSong) {
            _globalMeasuresEditor1.SetSong(currentSong);
            _globalMeasuresEditor2.SetSong(currentSong);    
        }

        /// <summary>
        /// Shows the last staff in the second GlobalMeasuresEditor,
        /// </summary>
        internal void ShowLastStaff() {
            _globalMeasuresEditor2.GoToLastStaff();
        }

        /// <summary>
        /// Adds a new measure to both GlobalMeasuresEditors,
        /// </summary>
        /// <param name="measure">The measure to add.</param>
        internal void AddMeasure(MeasureData measure) {
            _globalMeasuresEditor1.AddMeasure(measure);
            _globalMeasuresEditor2.AddMeasure(measure);
        }

        /// <summary>
        /// Appends a specified number of blank measures to the beginning of the currently displayed segment in both GlobalMeasuresEditors,
        /// </summary>
        /// <param name="number">The number of blank measures to append.</param>
        internal void AppendBlankMeasures(int number) {
            for (int i = 0; i < number; i++) {
                MeasureData newMeasure = CreateEmptyMeasure(i, new TimeSignature());
                _currentSong.Segments[_displayedSegmentIndex].Measures.Insert(i, newMeasure);
            }
            Reindex();
            Refresh();
        }

        /// <summary>
        /// Reindexes the measures in the currently displayed segment of the song in both GlobalMeasuresEditors, 
        /// ensuring that their indices are consistent and correctly ordered.
        /// </summary>
        private void Reindex() {
            _globalMeasuresEditor1.Reindex();
            _globalMeasuresEditor2.Reindex();
        }

        /// <summary>
        /// Inserts a new empty measure after the specified measure in the currently displayed segment of the song,
        /// </summary>
        /// <param name="measure">The measure after which to insert the new empty measure.</param>
        internal void InsertAfter(MeasureData measure) {
            int index = _currentSong.Segments[_displayedSegmentIndex].Measures.IndexOf(measure);
            MeasureData newMeasure = CreateEmptyMeasure(index + 1, measure.TimeSignature);
            _currentSong.Segments[_displayedSegmentIndex].Measures.Insert(index + 1, newMeasure);
            Reindex();
            Refresh();
        }

        /// <summary>
        /// Inserts a new empty measure before the specified measure in the currently displayed segment of the song,
        /// </summary>
        /// <param name="measure">The measure before which to insert the new empty measure.</param>
        internal void InsertBefore(MeasureData measure) {
            int index = _currentSong.Segments[_displayedSegmentIndex].Measures.IndexOf(measure);
            MeasureData newMeasure = CreateEmptyMeasure(index, measure.TimeSignature);
            _currentSong.Segments[_displayedSegmentIndex].Measures.Insert(index, newMeasure);
            Reindex();
            Refresh();
        }

        /// <summary>
        /// Deletes the specified measure from the currently displayed segment of the song,
        /// </summary>
        /// <param name="measure">The measure to delete.</param>
        internal void Delete(MeasureData measure) {
            if (_currentSong.Segments[_displayedSegmentIndex].Measures.Count <= 1) {
                return;
            }
            _ = _currentSong.Segments[_displayedSegmentIndex].Measures.Remove(measure);
            Reindex();
            Refresh();
        }

        /// <summary>
        /// Creates a new empty measure with the specified index and time signature,        
        /// </summary>
        /// <param name="index">The index of the new measure.</param>
        /// <param name="ts">The time signature of the new measure.</param>
        /// <returns>The newly created empty measure.</returns>
        internal MeasureData CreateEmptyMeasure(int index, TimeSignature ts) {
            List<MeasureGlobalMelody> staffs = new();

            int staffCount =
                _currentSong.SongSettings.StaffsSettings.Staffs.Count;

            for (int staffIndex = 0;
                 staffIndex < staffCount;
                 staffIndex++) {
                staffs.Add(new MeasureGlobalMelody(staffIndex));
            }

            return new MeasureData(
                index,
                _currentSong.SongSettings,
                ts,
                new KeySignature(
                    NoteStep.C,
                    Alteration.neutral,
                    SongMode.major),
                new ChordSequence(),
                staffs,
                ""
            );
        }

        /// <summary>
        /// Specifies that the second GlobalMeasuresEditor should be used for editing,
        /// and sets it to display the last staff,
        /// </summary>
        internal void UseSecondaryStaff() {
            _globalMeasuresEditor2.GoToLastStaff();
            _globalMeasuresEditor2.IsPlaceHolder = false;
        }

        /// <summary>
        /// Notifies both GlobalMeasuresEditors that a measure has changed,
        /// </summary>
        /// <param name="measure">The measure that has changed.</param>
        /// <param name="source">The GlobalMeasuresEditor that originated the change.</param>
        internal void NotifyMeasureChanged(MeasureData measure, GlobalMeasuresEditor source) {
            if (source != _globalMeasuresEditor1) {
                _globalMeasuresEditor1.RefreshMeasure(measure);
            }
            if (source != _globalMeasuresEditor2) {
                _globalMeasuresEditor2.RefreshMeasure(measure);
            }
        }

        /// <summary>
        /// Notifies both GlobalMeasuresEditors that a specific staff of a measure has changed,
        /// </summary>
        /// <param name="measure">The measure that has changed.</param>
        /// <param name="staffIndex">The index of the staff that has changed.</param>
        /// <param name="source">The GlobalMeasuresEditor that originated the change.</param>
        internal void NotifyStaffChanged(MeasureData measure, int staffIndex, GlobalMeasuresEditor source) {
            if (source != _globalMeasuresEditor1) {
                _globalMeasuresEditor1.RefreshMeasureStaff(
                    measure,
                    staffIndex);
            }

            if (source != _globalMeasuresEditor2) {
                _globalMeasuresEditor2.RefreshMeasureStaff(
                    measure,
                    staffIndex);
            }
        }

        /// <summary>
        /// Ensures that the number of staffs in the currently displayed segment matches the expected count based on the song's settings,
        /// </summary>
        private void EnsureDisplayedSegmentStaffs() {
            int expectedStaffCount =
                _currentSong.SongSettings.StaffsSettings.Staffs.Count;

            SegmentData segment =
                _currentSong.Segments[_displayedSegmentIndex];

            segment.AddOrRemoveMesuresStaffs(expectedStaffCount);
        }
    }
}
