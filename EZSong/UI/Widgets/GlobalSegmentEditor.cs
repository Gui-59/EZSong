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

        

        private GlobalMeasuresEditor _globalMeasuresEditor1;
        private GlobalMeasuresEditor _globalMeasuresEditor2;

        public GlobalSegmentEditor(Song _currentSong, UserSettings _userSettings, EmbeddedMidiSynth _embeddedMidiSynth) : base(Orientation.Vertical, 0) {

            _globalMeasuresEditor1 = new(_currentSong, _userSettings, _embeddedMidiSynth, false);
            PackStart(_globalMeasuresEditor1, true, true, 0);

            _globalMeasuresEditor2 = new(_currentSong, _userSettings, _embeddedMidiSynth, true);
            PackStart(_globalMeasuresEditor2, true, true, 0);

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

        internal void RefreshDisplayedSegment(int displayedSegmentIndex) {
            _globalMeasuresEditor1.RefreshDisplayedSegment(displayedSegmentIndex);
            _globalMeasuresEditor2.RefreshDisplayedSegment(displayedSegmentIndex);
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
    }
}
