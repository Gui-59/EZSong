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
    /// A widget that displays and manages the measures of a song segment, 
    /// allowing for editing and interaction with individual measures.
    /// </summary>
    internal class MeasuresEditor : Box {

        private GlobalMeasuresEditor _globalMeasuresEditor;

        private UserSettings _userSettings;

        private EmbeddedMidiSynth _embeddedMidiSynth; //Pour echo MIDI

        private bool _isPlaceHolder;

        private Box _measuresWidgetsBox;

        private Song _song;

        private int _staffIndex;
        
        private int _segmentIndex;

        /// <summary>
        /// Determines whether this MeasuresEditor instance is a placeholder, which affects its behavior and display.
        /// </summary>
        public bool IsPlaceHolder {
            get {
                return _isPlaceHolder;
            }
            internal set {
                _isPlaceHolder = value;
            }
        }

        /// <summary>
        /// Creates a new instance of the MeasuresEditor widget, 
        /// initializing it with the provided parameters and setting up its internal structure.
        /// </summary>
        /// <param name="globalMeasuresEditor">The global measures editor that this widget is associated with.</param>
        /// <param name="userSettings">The user settings to be used by this widget.</param>
        /// <param name="embeddedMidiSynth">The embedded MIDI synthesizer for MIDI echo.</param>
        /// <param name="currentSong">The current song being edited.</param>
        /// <param name="isPlaceHolder">Indicates whether this widget is a placeholder.</param>
        public MeasuresEditor(GlobalMeasuresEditor globalMeasuresEditor, UserSettings userSettings, EmbeddedMidiSynth embeddedMidiSynth, Song currentSong, bool isPlaceHolder) {

            _globalMeasuresEditor = globalMeasuresEditor;
            _userSettings = userSettings;
            _embeddedMidiSynth = embeddedMidiSynth;
            _isPlaceHolder = isPlaceHolder;
            _song = currentSong;
            _measuresWidgetsBox = new Box(Orientation.Horizontal, 0);

            //Refresh();
        }

        /// <summary>
        /// Initializes the components of the MeasuresEditor widget, 
        /// setting up its internal structure and adding child widgets based on whether it is a placeholder or not.
        /// </summary>
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

        /// <summary>
        /// Refreshes the MeasuresEditor widget, 
        /// reinitializing its components and updating the displayed measures based on the current song segment and staff index.
        /// </summary>
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

        /// <summary>
        /// Refreshes the displayed segment in the MeasuresEditor widget,
        /// </summary>
        /// <param name="segmentIndex">The index of the segment to display.</param>
        internal void RefreshDisplayedSegment(int segmentIndex) {
            _segmentIndex = segmentIndex;
            Refresh();
        }

        /// <summary>
        /// Refreshes the displayed staff in the MeasuresEditor widget,
        /// </summary>
        /// <param name="staffIndex">The index of the staff to display.</param>
        internal void RefreshDisplayedStaff(int staffIndex) {
            _staffIndex = staffIndex;
            foreach (MeasureEditorWidget measureEditorWidget in _measuresWidgetsBox.Children) {
                measureEditorWidget.RefreshDisplayedStaff(staffIndex);
            }            
            ShowAll();
        }

        /// <summary>
        /// Clears the MeasuresEditor widget, 
        /// removing all child widgets and disposing of them, 
        /// while preserving the internal measures widgets box for potential reuse.
        /// </summary>
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

        /// <summary>
        /// Reindexes the measures in the current segment of the song,
        /// </summary>
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

        /// <summary>
        /// Gets the currently focused MelodyMeasureEditor, 
        /// if any, 
        /// from the measures displayed in this MeasuresEditor widget.
        /// </summary>
        /// <returns>The currently focused MelodyMeasureEditor, if any; otherwise, null.</returns>
        public MelodyMeasureEditor? GetFocusedMelodyMeasureEditor() {
            foreach (MeasureEditorWidget measureEditor in _measuresWidgetsBox.Children) {
                if (measureEditor.GlobalMelodyEditor.MelodyMeasureEditor.HasFocus) {
                    return measureEditor.GlobalMelodyEditor.MelodyMeasureEditor;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a MeasureEditorWidget to the MeasuresEditor widget,
        /// </summary>
        /// <param name="widget">The MeasureEditorWidget to add.</param>
        internal void AddMeasure(MeasureEditorWidget widget) {

            if (_isPlaceHolder) { //Ce garde-fou évite de construire des widgets dans un conteneur qui n'est pas affiché.
                widget.DisposeEditors();
                widget.Dispose();
                return;
            }

            _measuresWidgetsBox.PackStart(widget, true, false, 0);
        }

        /// <summary>
        /// Refreshes the specified measure in the MeasuresEditor widget,
        /// </summary>
        /// <param name="measure">The measure to refresh.</param>
        internal void RefreshMeasure(MeasureData measure) {
            foreach (MeasureEditorWidget widget in
                     _measuresWidgetsBox.Children) {
                if (ReferenceEquals(widget.Measure, measure)) {
                    widget.RefreshSharedFromModel();
                    return;
                }
            }
        }

        /// <summary>
        /// Refreshes the staff of the specified measure in the MeasuresEditor widget,
        /// </summary>
        /// <param name="measure">The measure whose staff to refresh.</param>
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
