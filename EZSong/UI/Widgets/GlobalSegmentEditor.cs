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

        internal MelodyMeasureEditor? GetFocusedMelodyMeasureEditor() {
            if (_globalMeasuresEditor1.IsFocus) {
                return _globalMeasuresEditor1.GetFocusedMelodyMeasureEditor();
            } else if (_globalMeasuresEditor2.IsFocus) {
                return _globalMeasuresEditor2.GetFocusedMelodyMeasureEditor();
            }
            return null;
        }

        internal void Refresh() {
            _globalMeasuresEditor1.Refresh();
            _globalMeasuresEditor2.Refresh();
        }

        internal void RefreshDisplayedSegment() {

            bool useSecondary = false;
            if (_currentSong.SongSettings.StaffsSettings.Staffs.Count > 1) {
                useSecondary = true;
            }

            _globalMeasuresEditor1.RefreshDisplayedSegment(_displayedSegmentIndex, false);
            _globalMeasuresEditor2.RefreshDisplayedSegment(_displayedSegmentIndex, !useSecondary);
        }

        internal void ResetDisplayedStaffs() {
            _globalMeasuresEditor1.GoToStaff(1);
            _globalMeasuresEditor2.GoToStaff(2);
        }

        internal void SetSong(Song currentSong) {
            _globalMeasuresEditor1.SetSong(currentSong);
            _globalMeasuresEditor2.SetSong(currentSong);    
        }

        internal void ShowLastStaff() {
            _globalMeasuresEditor2.GoToLastStaff();
        }

        internal void AddMeasure(MeasureData measure) {

            _globalMeasuresEditor1.AddMeasure(measure);
            _globalMeasuresEditor2.AddMeasure(measure);

            
        }

        internal void AppendBlankMeasures(int number) {
            for (int i = 0; i < number; i++) {
                MeasureData newMeasure = CreateEmptyMeasure(i, new TimeSignature());
                _currentSong.Segments[_displayedSegmentIndex].Measures.Insert(i, newMeasure);
            }
            Reindex();
            Refresh();
        }

        private void Reindex() {
            _globalMeasuresEditor1.Reindex();
            _globalMeasuresEditor2.Reindex();
        }

        internal void InsertAfter(MeasureData measure) {
            int index = _currentSong.Segments[_displayedSegmentIndex].Measures.IndexOf(measure);
            MeasureData newMeasure = CreateEmptyMeasure(index + 1, measure.TimeSignature);
            _currentSong.Segments[_displayedSegmentIndex].Measures.Insert(index + 1, newMeasure);
            Reindex();
            Refresh();
        }

        internal void InsertBefore(MeasureData measure) {
            int index = _currentSong.Segments[_displayedSegmentIndex].Measures.IndexOf(measure);
            MeasureData newMeasure = CreateEmptyMeasure(index, measure.TimeSignature);
            _currentSong.Segments[_displayedSegmentIndex].Measures.Insert(index, newMeasure);
            Reindex();
            Refresh();
        }

        internal void Delete(MeasureData measure) {
            if (_currentSong.Segments[_displayedSegmentIndex].Measures.Count <= 1) {
                return;
            }
            _ = _currentSong.Segments[_displayedSegmentIndex].Measures.Remove(measure);
            Reindex();
            Refresh();
        }

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

        internal void UseSecondaryStaff() {
            _globalMeasuresEditor2.IsPlaceHolder = false;
        }
    }
}
