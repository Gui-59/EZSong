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
    /// A widget that allows for the editing of a single measure within a song segment,
    /// </summary>
    public class MeasureEditorWidget : Frame {

        private ComboBoxText _keyCombo;
        private ComboBoxText _upperTimeSigCombo;
        private ComboBoxText _lowerTimeSigCombo;
        private MeasureChordsEditor _measureChordsEditor;
        private Entry _lyricsEntry;

        private int _segmentIndex;
        private int _staffIndex;
        private MeasureData _measure;
        internal MeasureData Measure {
            get {
                return _measure;
            }
        }


        public GlobalMelodyEditor GlobalMelodyEditor { 
            get; 
            private set;
        }

        public event System.Action<MeasureData>? MeasureChanged;

        public event System.Action<MeasureData, int>? StaffChanged;

        public event System.Action<MeasureData>? InsertBeforeRequested;
        public event System.Action<MeasureData>? InsertAfterRequested;
        public event System.Action<MeasureData>? DeleteRequested;

        private SelectableValues _selectableValues = new();

        /// <summary>
        /// Creates a new instance of the MeasureEditorWidget, 
        /// initializing it with the provided measure data, user settings, and MIDI synthesizer.
        /// </summary>
        /// <param name="measure">The measure data to be edited.</param>
        /// <param name="userSettings">The user settings to apply.</param>
        /// <param name="embeddedMidiSynth">The embedded MIDI synthesizer to use for playback.</param>
        /// <param name="segmentIndex">The index of the segment containing the measure.</param>
        /// <param name="staffIndex">The index of the staff containing the measure.</param>
        public MeasureEditorWidget(
            MeasureData measure, 
            UserSettings userSettings, 
            EmbeddedMidiSynth embeddedMidiSynth,
            int segmentIndex,
            int staffIndex
        ) {
            _measure = measure;
            _segmentIndex = segmentIndex;
            _staffIndex = staffIndex;
            GlobalMelodyEditor = new(_segmentIndex, _staffIndex, _measure, userSettings, embeddedMidiSynth);
            _keyCombo = new();
            _upperTimeSigCombo = new();
            _lowerTimeSigCombo = new();
            _measureChordsEditor = new();
            _lyricsEntry = new() {
                Text = _measure.Lyrics ?? "",
                WidthChars = 24,
                PlaceholderText = "Paroles (séparées par espaces)"
            };
            BuildUI();
        }

        /// <summary>
        /// Disposes of the editors within the MeasureEditorWidget, releasing any resources they may be holding.
        /// </summary>
        public void DisposeEditors() {
            GlobalMelodyEditor.DisposeEditors();
        }

        /// <summary>
        /// Builds the user interface for the MeasureEditorWidget, arranging the various controls and editors in a vertical layout.
        /// </summary>
        private void BuildUI() {

            Box row = new(Orientation.Vertical, 0); //On met en superposé les elements qui définissent une mesure

            // Barre de boutons en haut : label + actions
            Box topBar = new(Orientation.Horizontal, 6) {
                Homogeneous = false
            };

            // Label mesure (aligné à gauche)
            Label label = new($"{_measure.Index}") {
                Xalign = 0f,
                Yalign = 0.5f
            };
            topBar.PackStart(label, true, true, 6);

            // Conteneur pour les boutons (alignés à droite)
            Box buttonsBox = new(Orientation.Horizontal, 4) {
                Homogeneous = false
            };

            //Bouton de suppression de la mesure
            Button deleteSelf = new();
            deleteSelf.Label = "Supprimer"; //TODO : icone à la place du texte
            deleteSelf.Clicked += (o, args) => {
                DeleteRequested?.Invoke(_measure);
            };
            buttonsBox.PackStart(deleteSelf, false, false, 0);

            //Bouton d'ajout de mesure avant
            Button addBefore = new();
            addBefore.Label = "Ajouter une mesure avant"; //TODO : icone à la place du texte
            addBefore.Clicked += (o, args) => {
                InsertBeforeRequested?.Invoke(_measure);
            };
            buttonsBox.PackStart(addBefore, false, false, 0);

            //Bouton d'ajout de mesure après
            Button addAfter = new();
            addAfter.Label = "Ajouter une mesure après"; //TODO : icone à la place du texte
            addAfter.Clicked += (o, args) => {
                InsertAfterRequested?.Invoke(_measure);
            };
            buttonsBox.PackStart(addAfter, false, false, 0);

            topBar.PackStart(buttonsBox, false, false, 0);

            // Ajouter la barre de boutons en haut de la mesure
            row.PackStart(topBar, false, false, 6);

            Box mesureSetupBar = new(Orientation.Horizontal, 6) {
                Homogeneous = false
            };

            // Tonalité (ComboBoxText)
            
            foreach (string k in _selectableValues.Tonalities.Keys) {
                _keyCombo.Append(k, _selectableValues.Tonalities[k]);
            }
            _keyCombo.ActiveId = _measure.KeySignature.ToDropDownId() != "" ? _measure.KeySignature.ToDropDownId() : _selectableValues.DefaultKeySignature.ToDropDownId();
            _keyCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(_keyCombo.ActiveId) && _selectableValues.Tonalities.ContainsKey(_keyCombo.ActiveId)) {
                    _measure.KeySignature = new(_keyCombo.ActiveId);
                }
                MeasureChanged?.Invoke(_measure);
            };
            mesureSetupBar.PackStart(_keyCombo, false, false, 0);

            // Signature temporelle : Upper (ComboBoxText)
            
            foreach (int upper in _selectableValues.UpperTimeSigs) {
                _upperTimeSigCombo.Append(upper.ToString(), upper.ToString());
            }
            int tsuIndex = Array.IndexOf(_selectableValues.UpperTimeSigs, _measure.TimeSignature.Beats);
            _upperTimeSigCombo.Active = tsuIndex >= 0 ? tsuIndex : Array.IndexOf(_selectableValues.UpperTimeSigs, _selectableValues.DefaultUpperTimeSig);

            _upperTimeSigCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(_upperTimeSigCombo.ActiveId)) {
                    _measure.TimeSignature.Beats = Int32.Parse(_upperTimeSigCombo.ActiveId);

                    //Mise à jour de la signature temporelle de l'éditeur de cadence pour qu'il puisse recalculer la grille de temps
                    GlobalMelodyEditor.UpdateTimeSignature(_measure.TimeSignature);
                }
                MeasureChanged?.Invoke(_measure);
            };
            mesureSetupBar.PackStart(_upperTimeSigCombo, false, false, 0);

            mesureSetupBar.PackStart(new Label("|") { Xalign = 0f }, false, false, 0);

            

            // Signature temporelle : Lower (ComboBoxText)
            
            foreach (int lower in _selectableValues.LowerTimeSigs) {
                _lowerTimeSigCombo.Append(lower.ToString(), lower.ToString());
            }
            int tslIndex = Array.IndexOf(_selectableValues.LowerTimeSigs, _measure.TimeSignature.BeatUnit);
            _lowerTimeSigCombo.Active = tslIndex >= 0 ? tslIndex : Array.IndexOf(_selectableValues.LowerTimeSigs, _selectableValues.DefaultLowerTimeSig);

            _lowerTimeSigCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(_lowerTimeSigCombo.ActiveId)) {
                    _measure.TimeSignature.BeatUnit = Int32.Parse(_lowerTimeSigCombo.ActiveId);
                    //Mise à jour de la signature temporelle de l'éditeur de cadence pour qu'il puisse recalculer la grille de temps
                    GlobalMelodyEditor.UpdateTimeSignature(_measure.TimeSignature);
                }
                MeasureChanged?.Invoke(_measure);
            };
            mesureSetupBar.PackStart(_lowerTimeSigCombo, false, false, 0);

            mesureSetupBar.PackStart(new Label("000 bpm") { Xalign = 0f }, false, false, 0); //TODO : indiquer la vraie valeur de BPM

            row.PackStart(mesureSetupBar, false, false, 0);

            // Accords
            
            _measureChordsEditor.LoadFromModel(_measure);
            _measureChordsEditor.ChordsChanged += (ChordSequence) => {
                _measure.ChordSequence = ChordSequence;
                MeasureChanged?.Invoke(_measure);
            };
            row.PackStart(_measureChordsEditor, false, false, 0);

            // Paroles (une saisie texte ; mots/syllabes séparés par espaces)
            
            _lyricsEntry.Changed += (o, args) => {
                _measure.Lyrics = _lyricsEntry.Text;
                MeasureChanged?.Invoke(_measure);
            };
            row.PackStart(_lyricsEntry, false, false, 0);


            //Mélodie (éditeur de mélodie global [notes + rythme])
            GlobalMelodyEditor.MelodyChanged += (staffIndex, melody) => {
                _measure.Staffs[staffIndex].Melody = melody;

                StaffChanged?.Invoke(_measure, staffIndex);
            };
            GlobalMelodyEditor.PatternChanged += (staffIndex, pattern) => {
                _measure.Staffs[staffIndex].Pattern = pattern;

                StaffChanged?.Invoke(_measure, staffIndex);
            };
            row.PackStart(GlobalMelodyEditor, false, false, 0);



            Add(row);

            ShowAll();
        }


        /// <summary>
        /// Refreshes the displayed segment in the MeasureEditorWidget, 
        /// updating the internal measure data and notifying the GlobalMelodyEditor to refresh its display accordingly.
        /// </summary>
        /// <param name="segmentIndex">The index of the segment to refresh.</param>
        /// <param name="measureData">The measure data to refresh with.</param>
        internal void RefreshDisplayedSegment(int segmentIndex, MeasureData measureData) {
            _segmentIndex = segmentIndex;
            _measure = measureData;
            GlobalMelodyEditor.RefreshDisplayedSegment(segmentIndex, _measure);
        }

        /// <summary>
        /// Refreshes the displayed staff in the MeasureEditorWidget, 
        /// updating the internal staff index and notifying the GlobalMelodyEditor to refresh its display accordingly.
        /// </summary>
        /// <param name="staffIndex">The index of the staff to refresh.</param>
        internal void RefreshDisplayedStaff(int staffIndex) {
            _staffIndex = staffIndex;
            GlobalMelodyEditor.RefreshDisplayedStaff(staffIndex);
        }

        /// <summary>
        /// Refreshes the shared properties of the measure (key signature, time signature, chords, lyrics) from the model,
        /// </summary>
        internal void RefreshSharedFromModel() {
            _keyCombo.ActiveId =
                _measure.KeySignature.ToDropDownId();

            int upperIndex =
                Array.IndexOf(
                    _selectableValues.UpperTimeSigs,
                    _measure.TimeSignature.Beats);

            _upperTimeSigCombo.Active =
                upperIndex >= 0
                    ? upperIndex
                    : Array.IndexOf(
                        _selectableValues.UpperTimeSigs,
                        _selectableValues.DefaultUpperTimeSig);

            int lowerIndex =
                Array.IndexOf(
                    _selectableValues.LowerTimeSigs,
                    _measure.TimeSignature.BeatUnit);

            _lowerTimeSigCombo.Active =
                lowerIndex >= 0
                    ? lowerIndex
                    : Array.IndexOf(
                        _selectableValues.LowerTimeSigs,
                        _selectableValues.DefaultLowerTimeSig);

            _measureChordsEditor.LoadFromModel(_measure);

            _lyricsEntry.Text = _measure.Lyrics ?? "";

            GlobalMelodyEditor.UpdateTimeSignature(
                _measure.TimeSignature);
        }

        /// <summary>
        /// Refreshes the staff information in the GlobalMelodyEditor from the model,
        /// </summary>
        internal void RefreshStaffFromModel() {
            GlobalMelodyEditor.RefreshFromModel(_measure);
        }
    }
}
