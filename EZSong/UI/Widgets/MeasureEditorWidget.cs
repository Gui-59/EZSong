using EZSong.Model;
using EZSong.UI.Widgets.WidgetsData;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets {
    public class MeasureEditorWidget : Frame {

        private int _segmentIndex;
        private int _staffIndex;
        private MeasureData _measure;

        public GlobalMelodyEditor GlobalMelodyEditor { 
            get; 
            private set;
        }

        public event System.Action<MeasureData>? MeasureChanged;
        public event System.Action<MeasureData>? InsertBeforeRequested;
        public event System.Action<MeasureData>? InsertAfterRequested;
        public event System.Action<MeasureData>? DeleteRequested;

        private SelectableValues _selectableValues = new();

        public MeasureEditorWidget(MeasureData measure) {
            _measure = measure;
            _segmentIndex = 0;  
            _staffIndex = 0; 
            GlobalMelodyEditor = new(_segmentIndex, _staffIndex, _measure); 
            BuildUI();
        }

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

            // Signature temporelle : Upper (ComboBoxText)
            ComboBoxText upperTimeSigCombo = new();
            foreach (int upper in _selectableValues.UpperTimeSigs) {
                upperTimeSigCombo.Append(upper.ToString(), upper.ToString());
            }
            int tsuIndex = Array.IndexOf(_selectableValues.UpperTimeSigs, _measure.TimeSignature.Beats);
            upperTimeSigCombo.Active = tsuIndex >= 0 ? tsuIndex : Array.IndexOf(_selectableValues.UpperTimeSigs, _selectableValues.DefaultUpperTimeSig);

            upperTimeSigCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(upperTimeSigCombo.ActiveId)) {
                    _measure.TimeSignature.Beats = Int32.Parse(upperTimeSigCombo.ActiveId);

                    //Mise à jour de la signature temporelle de l'éditeur de cadence pour qu'il puisse recalculer la grille de temps
                    GlobalMelodyEditor.UpdateTimeSignature(_measure.TimeSignature);
                }
                MeasureChanged?.Invoke(_measure);
            };
            row.PackStart(new Label("Time Sig. (Upper) :") { Xalign = 0f }, false, false, 0);
            row.PackStart(upperTimeSigCombo, false, false, 0);

            // Signature temporelle : Lower (ComboBoxText)
            ComboBoxText lowerTimeSigCombo = new();
            foreach (int lower in _selectableValues.LowerTimeSigs) {
                lowerTimeSigCombo.Append(lower.ToString(), lower.ToString());
            }
            int tslIndex = Array.IndexOf(_selectableValues.LowerTimeSigs, _measure.TimeSignature.BeatUnit);
            lowerTimeSigCombo.Active = tslIndex >= 0 ? tslIndex : Array.IndexOf(_selectableValues.LowerTimeSigs, _selectableValues.DefaultLowerTimeSig);

            lowerTimeSigCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(lowerTimeSigCombo.ActiveId)) {
                    _measure.TimeSignature.BeatUnit = Int32.Parse(lowerTimeSigCombo.ActiveId);
                    //Mise à jour de la signature temporelle de l'éditeur de cadence pour qu'il puisse recalculer la grille de temps
                    GlobalMelodyEditor.UpdateTimeSignature(_measure.TimeSignature);
                }
                MeasureChanged?.Invoke(_measure);
            };
            row.PackStart(new Label("Time Sig. (Lower) :") { Xalign = 0f }, false, false, 0);
            row.PackStart(lowerTimeSigCombo, false, false, 0);

            // Tonalité (ComboBoxText)
            ComboBoxText keyCombo = new();
            foreach (string k in _selectableValues.Tonalities.Keys) {
                keyCombo.Append(k, _selectableValues.Tonalities[k]);
            }
            keyCombo.ActiveId = _measure.KeySignature.ToDropDownId() != "" ? _measure.KeySignature.ToDropDownId() : _selectableValues.DefaultKeySignature.ToDropDownId();
            keyCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(keyCombo.ActiveId) && _selectableValues.Tonalities.ContainsKey(keyCombo.ActiveId)) {
                    _measure.KeySignature = new(keyCombo.ActiveId);
                }
                MeasureChanged?.Invoke(_measure);
            };

            row.PackStart(new Label("Tonalité :") { Xalign = 0f }, false, false, 0);
            row.PackStart(keyCombo, false, false, 0);

            // Accords
            MeasureChordsEditor measureChordsEditor = new();
            measureChordsEditor.LoadFromModel(_measure);
            measureChordsEditor.ChordsChanged += (ChordSequence) => {
                _measure.ChordSequence = ChordSequence;
                MeasureChanged?.Invoke(_measure);
            };
            row.PackStart(new Label("Accords :") { Xalign = 0f }, false, false, 0);
            row.PackStart(measureChordsEditor, false, true, 0);

            //Mélodie (éditeur de mélodie global [notes + rythme])
            GlobalMelodyEditor.MelodyChanged += (staffIndex, melody) => {
                _measure.Staffs[staffIndex].Melody = melody;

                MeasureChanged?.Invoke(_measure);
            };
            GlobalMelodyEditor.PatternChanged += (staffIndex, pattern) => {
                _measure.Staffs[staffIndex].Pattern = pattern;
                MeasureChanged?.Invoke(_measure);
            };
            row.PackStart(GlobalMelodyEditor, true, true, 0);

            // Paroles (une saisie texte ; mots/syllabes séparés par espaces)
            Entry lyricsEntry = new() {
                Text = _measure.Lyrics ?? "",
                WidthChars = 24,
                PlaceholderText = "Paroles (séparées par espaces)"
            };
            lyricsEntry.Changed += (o, args) => {
               _measure.Lyrics = lyricsEntry.Text;
                MeasureChanged?.Invoke(_measure);
            };
            row.PackStart(new Label("Paroles :") { Xalign = 0f }, false, false, 0);
            row.PackStart(lyricsEntry, true, true, 0);

            Add(row);

            ShowAll();
        }

        internal void RefreshDisplayedSegment(int segmentIndex, MeasureData measureData) {
            _segmentIndex = segmentIndex;
            _measure = measureData;
            GlobalMelodyEditor.RefreshDisplayedSegment(segmentIndex, _measure);
        }

        internal void RefreshDisplayedStaff(int staffIndex) {
            _staffIndex = staffIndex;
            GlobalMelodyEditor.RefreshDisplayedStaff(staffIndex);
        }
    }
}
