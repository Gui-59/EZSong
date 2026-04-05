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
        private MeasureData _measure;

        public MelodyMeasureEditor MelodyMeasureEditor;

        public event System.Action? MeasureChanged;
        public event System.Action? InsertBeforeRequested;
        public event System.Action? InsertAfterRequested;
        public event System.Action? DeleteRequested;

        private SelectableValues _selectableValues = new();

        public MeasureEditorWidget() {
            _measure = new MeasureData();
            MelodyMeasureEditor = new MelodyMeasureEditor();
        }

        public void SetMeasure(MeasureData measure) {
            _measure = measure;

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
            deleteSelf.Label = "Supprimer";
            deleteSelf.Clicked += (o, args) => {
                DeleteRequested?.Invoke();
            };
            buttonsBox.PackStart(deleteSelf, false, false, 0);

            //Bouton d'ajout de mesure avant
            Button addBefore = new();
            addBefore.Label = "Ajouter une mesure avant";
            addBefore.Clicked += (o, args) => {
                InsertBeforeRequested?.Invoke();
            };
            buttonsBox.PackStart(addBefore, false, false, 0);

            //Bouton d'ajout de mesure après
            Button addAfter = new();
            addAfter.Label = "Ajouter une mesure après";
            addAfter.Clicked += (o, args) => {
                InsertAfterRequested?.Invoke();
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

                    // TODO ? : Mettre à jour la signature temporelle de l'éditeur de cadence pour qu'il puisse recalculer la grille de temps
                }
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
                    //TODO ? : Mettre à jour la signature temporelle de l'éditeur de cadence pour qu'il puisse recalculer la grille de temps
                }
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
            };

            row.PackStart(new Label("Tonalité :") { Xalign = 0f }, false, false, 0);
            row.PackStart(keyCombo, false, false, 0);

            // Accords (1 champ texte: accords séparés par espaces, 1 accord par temps)
            Entry chordEntry = new() {
                Text = _measure.ChordSequence.ToLilyPondString() ?? new ChordSequence("").ToString(),
                WidthChars = 24,
                PlaceholderText = "Accords (ex: C Am Dm G7)"
            };
            chordEntry.Changed += (o, args) => {
                _measure.ChordSequence = new ChordSequence(chordEntry.Text ?? string.Empty);
                MeasureChanged?.Invoke();
            };
            row.PackStart(new Label("Accords :") { Xalign = 0f }, false, false, 0);
            row.PackStart(chordEntry, true, true, 0);

            // Editeur de mélodie/cadence
            MelodyMeasureEditor = new();
            MelodyMeasureEditor.LoadFromModel(_measure.Melody.ToWidgetChords(), null, initialCursor: 0);

            // Handler local : met à jour la measure associée (capture 'measure' et 'editor')
            MelodyMeasureEditor.ContentChanged += (s, e) => {
                MeasureMelody newMeasureMelody = _measure.Melody;
                newMeasureMelody.MelodyChords = new List<MelodyChord>();
                foreach (WidgetMelodyChord widgetChord in MelodyMeasureEditor.ExportToModel()) {
                    newMeasureMelody.MelodyChords.Add(widgetChord.ToMelodyChord());
                }
                _measure.Melody = newMeasureMelody;
            };

            MelodyMeasureEditor.WidthRequest = 250;
            row.PackStart(MelodyMeasureEditor, true, false, 0);

            MelodyMeasureEditor.ShowAll();

            MeasureRhythmEditor rhythmEditor = new();

            TimeSignature ts = new(4, 4); //TODO : faire en sorte que ce soit défini par la mesure et que ça puisse être modifié via l'interface 
            rhythmEditor.SetPattern(new MeasureRhythmPattern(ts));

            // exemple
            rhythmEditor.NoteCount = 4;
            rhythmEditor.GraceNoteCount = 0;

            row.PackStart(rhythmEditor, true, false, 0);

            // Paroles (une saisie texte ; mots/syllabes séparés par espaces)
            Entry lyricsEntry = new() {
                Text = _measure.Lyrics ?? "",
                WidthChars = 24,
                PlaceholderText = "Paroles (séparées par espaces)"
            };
            lyricsEntry.Changed += (o, args) => {
               _measure.Lyrics = lyricsEntry.Text;
            };
            row.PackStart(new Label("Paroles :") { Xalign = 0f }, false, false, 0);
            row.PackStart(lyricsEntry, true, true, 0);

            Add(row);

            ShowAll();
        }
    }
}
