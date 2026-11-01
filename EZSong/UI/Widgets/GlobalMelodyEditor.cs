using EZSong.MIDI;
using EZSong.Model;
using EZSong.Settings;
using EZSong.UI.Widgets.WidgetsData;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets {

    /// <summary>
    /// A widget that allows for the editing of a melody measure (combination of rhythm and melody chords),
    /// </summary>
    public class GlobalMelodyEditor : Frame {

        private int _segmentIndex;
        private int _staffIndex;

        private UserSettings _userSettings;
        private EmbeddedMidiSynth _embeddedMidiSynth; //Pour echo MIDI

        public MelodyMeasureEditor MelodyMeasureEditor {
            get; 
            private set;
        }
        
        private MeasureRhythmEditor _rhythmEditor;

        public event Action<int, MeasureMelody>? MelodyChanged; //Premier paramètre : l'index de la portée
        public event Action<int, MeasureRhythmPattern>? PatternChanged; //Premier paramètre : l'index de la portée

        /// <summary>
        /// Creates a new instance of the GlobalMelodyEditor widget, 
        /// initializing it with the provided parameters and setting up its internal structure.
        /// </summary>
        /// <param name="segmentIndex">The index of the segment to be edited.</param>
        /// <param name="staffIndex">The index of the staff to be edited.</param>
        /// <param name="measureData">The measure data to be edited.</param>
        /// <param name="userSettings">The user settings to be applied.</param>
        /// <param name="embeddedMidiSynth">The embedded MIDI synthesizer for playback.</param>
        public GlobalMelodyEditor(
            int segmentIndex, 
            int staffIndex, 
            MeasureData measureData, 
            UserSettings userSettings, 
            EmbeddedMidiSynth embeddedMidiSynth
        ) {
            _segmentIndex = segmentIndex;
            _staffIndex = staffIndex;
            _userSettings = userSettings;
            _embeddedMidiSynth = embeddedMidiSynth;
            MelodyMeasureEditor = new(embeddedMidiSynth, userSettings);
            MelodyMeasureEditor.LoadFromModel(staffIndex, measureData);
            _rhythmEditor = new();
            _rhythmEditor.LoadFromModel(measureData);
            BuildUI();
            ShowAll();
        }

        /// <summary>
        /// Disposes of the editors, 
        /// stopping any timers or processes that may be running, such as the cursor timer in the MelodyMeasureEditor.
        /// </summary>
        public void DisposeEditors() {
            MelodyMeasureEditor.StopCursorTimer();
        }

        /// <summary>
        /// Builds the user interface of the GlobalMelodyEditor,
        /// </summary>
        private void BuildUI() {

            Box row = new(Orientation.Vertical, 0); //On met en superposé les elements qui définissent une mesure
            
            // Handler local : met à jour la measure associée (capture 'measure' et 'editor')
            MelodyMeasureEditor.ContentChanged += (s, e) => {              
                MeasureMelody newMeasureMelody = new(_staffIndex);
                newMeasureMelody.MelodyChords = new List<MelodyChord>();
                foreach (WidgetMelodyChord widgetChord in MelodyMeasureEditor.ExportToModel()) {
                    newMeasureMelody.MelodyChords.Add(widgetChord.ToMelodyChord());
                }
                MelodyChanged?.Invoke(_staffIndex, newMeasureMelody);
            };

            MelodyMeasureEditor.WidthRequest = 250; //TODO : calculer la largeur en fonction du nombre de notes et de la signature rythmique
            row.PackStart(MelodyMeasureEditor, true, false, 0);

            MelodyMeasureEditor.ShowAll();

            _rhythmEditor.PatternChanged += (staffIndex, pattern) => {
                PatternChanged?.Invoke(staffIndex, pattern);
                QueueDraw();
            };

            row.PackStart(_rhythmEditor, true, false, 0);

            Add(row);

            ShowAll();
        }

        /// <summary>
        /// Updates the time signature of the rhythm editor, 
        /// allowing for changes in the rhythmic structure of the measure being edited.
        /// </summary>
        /// <param name="timeSignature">The time signature to be applied to the rhythm editor.</param>
        internal void UpdateTimeSignature(TimeSignature timeSignature) {
            _rhythmEditor.UpdateTimeSignature(timeSignature);
        }

        /// <summary>
        /// Refreshes the displayed segment in both the melody and rhythm editors,
        /// </summary>
        /// <param name="displayedSegmentIndex">The index of the segment to be displayed.</param>
        /// <param name="measureData">The measure data to be used for refreshing the display.   </param>
        internal void RefreshDisplayedSegment(int displayedSegmentIndex, MeasureData measureData) {
            _segmentIndex = displayedSegmentIndex;
            MelodyMeasureEditor.RefreshDisplayedSegment(displayedSegmentIndex, measureData);
            _rhythmEditor.RefreshDisplayedSegment(displayedSegmentIndex);
        }

        /// <summary>
        /// Refreshes the displayed staff in both the melody and rhythm editors,
        /// </summary>
        /// <param name="displayedStaffIndex">The index of the staff to be displayed.</param>
        internal void RefreshDisplayedStaff(int displayedStaffIndex) {
            _staffIndex = displayedStaffIndex;
            MelodyMeasureEditor.RefreshDisplayedStaff(displayedStaffIndex);
            _rhythmEditor.RefreshDisplayedStaff(displayedStaffIndex);
        }

        /// <summary>
        /// Refreshes the editors with the provided measure data, 
        /// updating both the melody and rhythm editors to reflect any changes in the underlying model.
        /// </summary>
        /// <param name="measureData"></param>
        internal void RefreshFromModel(MeasureData measureData) {
            MelodyMeasureEditor.LoadFromModel(
                _staffIndex,
                measureData,
                false);

            _rhythmEditor.LoadFromModel(measureData);

            MelodyMeasureEditor.QueueDraw();
            _rhythmEditor.QueueDraw();
        }
    }
}