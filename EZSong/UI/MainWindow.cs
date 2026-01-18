using Cairo;
using Gdk;
using Gtk;
using Microsoft.VisualBasic;
using EZSong.Enums;
using EZSong.Exporting.Lilypond;
using EZSong.MIDI;
using EZSong.Serializable;
using EZSong.UI.Widgets;
using EZSong.UI.Widgets.WidgetsData;
using Pango;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EZSong.UI
{

    // Simple GTK UI
    public class MainWindow : Gtk.Window
    {
        private const int _initialWidth = 900;
        private const int _initialHeight = 600;
        private const int _compactThreshold = 800;

        private Song _currentSong = new();
        private ListStore _measureStore;

        private MidiInputManager _midiManager;

        //Pour gérer les onglets : Stack, StackSwitcher et FlowBox
        private Gtk.Stack _stack;
        private StackSwitcher _switcher;
        private FlowBox[] _flowTabs;

        private Entry _titleEntry;
        private Entry _artistEntry;
        private Entry _commentEntry;
        private Box _measuresBox;
        private List<MelodyMeasureEditor> _melodyMeasureEditorWidgets;
        private Statusbar _statusBar;
        private uint _statusBarContextId;

        private SelectableValues _selectableValues = new();            

        public MainWindow() : base("EZSong")
        {
            SetDefaultSize(_initialWidth, _initialHeight);
            DeleteEvent += (o, e) => Application.Quit();

            Box windowBox = new(Orientation.Vertical, 0);
            Add(windowBox);

            // Barre d’onglets
            _stack = new Gtk.Stack { 
                TransitionType = StackTransitionType.Crossfade, 
                TransitionDuration = 300 
            };
            _switcher = new StackSwitcher { 
                Stack = _stack 
            };
            _flowTabs = new FlowBox[3];
            PopulateMenuTabs();         

            Box mainBox = new(Orientation.Vertical, 0);

            // Header
            Box headerBox = new(Orientation.Horizontal, 0);
            _titleEntry = new Entry { 
                PlaceholderText = "Titre du morceau" 
            };
            headerBox.PackStart(_titleEntry, true, true, 0);
            _artistEntry = new Entry { 
                PlaceholderText = "Interprète" 
            };            
            headerBox.PackStart(_artistEntry, true, true, 0);
            _commentEntry = new Entry { 
                PlaceholderText = "Commentaires" 
            };
            headerBox.PackStart(_commentEntry, true, true, 0);
            mainBox.PackStart(headerBox, false, false, 0);

            // Table mesures
            _measureStore = new ListStore(
                typeof(string),  // Numéro
                typeof(string),  // Signature temporelle
                typeof(string),  // Tonalité
                typeof(string),  // Accords
                typeof(string)   // Paroles
            );
            _measuresBox = new Box(Orientation.Horizontal, 4); //Les mesures seront cotes à cotes

            ScrolledWindow scrolled = new();
            scrolled.Add(_measuresBox);
            mainBox.PackStart(scrolled, true, true, 0);

            // Barre de status
            _statusBar = new Statusbar();
            _statusBarContextId = _statusBar.GetContextId("main");
            _ = _statusBar.Push(_statusBarContextId, "Prêt.");
            mainBox.PackStart(_statusBar, false, false, 0);

            windowBox.PackStart(_switcher, false, false, 0);
            windowBox.PackStart(_stack, false, false, 0);
            windowBox.PackStart(mainBox, true, true, 0);

            // Signal de redimensionnement
            SizeAllocated += (o, args) => UpdateCompactMode(args.Allocation.Width);

            // Style moderne
            ApplyCss();

            DeleteEvent += (o, args) => Application.Quit();

            ShowAll();

            

            //Detection de la saisie via clavier MIDI
            _midiManager = new MidiInputManager();

            string? firstDevice = MidiInputManager.GetAvailableDevices().FirstOrDefault();
            if (firstDevice != null) {
                _ = _midiManager.Open(firstDevice);
                Console.WriteLine($"MIDI connecté : {firstDevice}");
            }

            // Quand des notes sont jouées
            _midiManager.NotesPlayed += notes =>
            {
                // Trouve l'éditeur de mesure actuellement focus
                MelodyMeasureEditor? focusedEditor = GetFocusedMelodyEditor();
                if (focusedEditor != null) {
                    Gtk.Application.Invoke((s, e) =>  // nécessaire car le callback MIDI n’est pas sur le thread GTK
                    {
                        focusedEditor.OnMidiNoteReceived(notes);
                    });
                }
            };

            _melodyMeasureEditorWidgets = new();

            AddMeasures(5); //5 mesures par défaut

            Maximize(); // Démarrer en mode maximisé
        }

        private void PopulateMenuTabs() {
            _flowTabs[0] = CreateTab("file");
            _stack.AddTitled(_flowTabs[0], "file", "Fichier");
            _flowTabs[1] = CreateTab("mesures");
            _stack.AddTitled(_flowTabs[1], "mesures", "Mesures");
            _flowTabs[2] = CreateTab("export");
            _stack.AddTitled(_flowTabs[2], "export", "Export");
        }

        private FlowBox CreateTab(string name) {
            FlowBox flow = new() {
                SelectionMode = SelectionMode.None,
                RowSpacing = 0,
                ColumnSpacing = 0,
                BorderWidth = 0,
                MaxChildrenPerLine = 8,
            };

            switch (name) {

                case "file":
                    flow.Add(CreateIconButton("Ouvrir", (s, e) => LoadProject(), "file-load.svg"));
                    flow.Add(CreateIconButton("Enregistrer", (s, e) => SaveProject(), "file-save.svg"));
                    break;

                case "mesures":
                    break;

                case "export":
                    flow.Add(CreateIconButton("Exporter au format LilyPond", (s, e) => ExportLilyPond(), "export-lilypond.svg"));
                    break;

            }

            return flow;
        }

        private Widget CreateIconButton(string label, EventHandler onClick, string iconName = "icon-placeholder.svg") {
            Image image = SvgHelper.LoadSvgAsGtkImage("EZSong.Ressources.SVG."+iconName, 32);
            Label lbl = new(label);
            lbl.Name = "btnLabel"; // pour le retrouver plus tard

            Box vbox = new(Orientation.Vertical, 0);
            vbox.PackStart(image, false, false, 0);
            vbox.PackStart(lbl, false, false, 0);

            Button btn = new() { Relief = ReliefStyle.None, Child = vbox };
            btn.Clicked += onClick;

            return btn;
        }

        private void UpdateCompactMode(int width) {
            bool compact = width < _compactThreshold;

            foreach (FlowBox flow in _flowTabs) {
                foreach (Widget w in flow.Children) {
                    if (w is Button btn && btn.Child is VBox box) {
                        foreach (Widget c in box.Children) {
                            if (c is Label lbl && lbl.Name == "btnLabel") {
                                lbl.Visible = !compact;
                            }
                        }
                    }
                }
            }
        }

        private void ApplyCss() {
            string css = @" 

                * {
                    padding : 0px;
                    margin : 0px;
                }

                button, entry {
                    padding : 4px;
                    margin : 1px;
                    border-radius : 4px;
                }

                .stack-switcher {
                    padding : 0px;
                    margin : 0px;
                }

                .stack-switcher button {
                    font-weight: bold;
                    background:transparent;
                    color:rgba(0, 0, 0, 1);
                    border : 0px;
                    border-bottom : 2px solid;
                    border-radius : 0px;
                    border-color : lightgray;
                    box-shadow: none;
                    background-image: none;
                    padding : 0px;
                    margin : 0px;
                    border-color : transparent;
                }

                .stack-switcher button:hover {
                    border-color : lightblue;
                }
                .stack-switcher button:checked {
                    border-color : blue;
                }

                .stack-switcher button label {
                    font-size: 9pt;
                    font-weight : normal;
                }
                .stack-switcher button:checked label {
                    font-weight : bold;
                }

                flowbox {
                    background-color : white;
                    margin-top : 6px;
                    margin-left: 7px;
                    margin-right: 7px;
                    margin-bottom: 10px;
                    padding : 2px;
                    border-radius : 5px;
                    box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
                }

                
                ";
            CssProvider cssProvider = new();
            _ = cssProvider.LoadFromData(css);
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, uint.MaxValue);
        }

        private void RefreshMeasuresView()  {

            if (_measureStore == null) {
                return;
            }

            _measureStore.Clear();

            foreach (MeasureData measure in _currentSong.Measures) {
                _ = _measureStore.AppendValues(
                    measure.Index.ToString(),
                    measure.TimeSignature,
                    measure.KeySignature,
                    measure.ChordSequence,
                    measure.Lyrics
                );

                AddMeasure(measure);
            }
        }

        private void AddMeasures(int number) {

            for (int i = 0; i < number; i++) {

                int num = _currentSong.Measures.Count + 1;
                MeasureData m = new() {
                    Index = num,
                    TimeSignature = new(4, 4),
                    KeySignature = new(NoteStep.C, Alteration.neutral, SongMode.major)
                };
                _currentSong.Measures.Add(m);
                _ = _measureStore.AppendValues(num.ToString(), m.TimeSignature, m.KeySignature, m.ChordSequence, m.Lyrics);

                AddMeasure(m);
            }
        }

        private void AddMeasure(MeasureData measure) {
            Box row = new(Orientation.Vertical, 0); //On met en superposé les elements qui définissent une mesure

            // Barre de boutons en haut : label + actions
            Box topBar = new(Orientation.Horizontal, 6) { Homogeneous = false };

            // Label mesure (aligné à gauche)
            Label label = new($"{measure.Index}") { Xalign = 0f, Yalign = 0.5f };
            topBar.PackStart(label, true, true, 6);

            // Conteneur pour les boutons (alignés à droite)
            Box buttonsBox = new(Orientation.Horizontal, 4) { Homogeneous = false };

            //Bouton de suppression de la mesure
            Button deleteSelf = new();
            deleteSelf.Label = "Supprimer";
            deleteSelf.Clicked += (o, args) => {
                DeleteMesure(measure.Index - 1);
            };
            buttonsBox.PackStart(deleteSelf, false, false, 0);

            //Bouton d'ajout de mesure avant
            Button addBefore = new();
            addBefore.Label = "Ajouter une mesure avant";
            addBefore.Clicked += (o, args) => {
                AddMesureBefore(measure.Index - 1);
            };
            buttonsBox.PackStart(addBefore, false, false, 0);

            //Bouton d'ajout de mesure après
            Button addAfter = new();
            addAfter.Label = "Ajouter une mesure après";
            addAfter.Clicked += (o, args) => {
                AddMesureAfter(measure.Index - 1);
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
            int tsuIndex = Array.IndexOf(_selectableValues.UpperTimeSigs, measure.TimeSignature.Upper);
            upperTimeSigCombo.Active = tsuIndex >= 0 ? tsuIndex : Array.IndexOf(_selectableValues.UpperTimeSigs, _selectableValues.DefaultUpperTimeSig);

            upperTimeSigCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(upperTimeSigCombo.ActiveId))
                {
                    measure.TimeSignature.Upper = Int32.Parse(upperTimeSigCombo.ActiveId);
                }
            };
            row.PackStart(new Label("Time Sig. (Upper) :") { Xalign = 0f }, false, false, 0);
            row.PackStart(upperTimeSigCombo, false, false, 0);

            // Signature temporelle : Lower (ComboBoxText)
            ComboBoxText lowerTimeSigCombo = new();
            foreach (int lower in _selectableValues.LowerTimeSigs) {
                lowerTimeSigCombo.Append(lower.ToString(), lower.ToString());
            }
            int tslIndex = Array.IndexOf(_selectableValues.LowerTimeSigs, measure.TimeSignature.Lower);
            lowerTimeSigCombo.Active = tslIndex >= 0 ? tslIndex : Array.IndexOf(_selectableValues.LowerTimeSigs, _selectableValues.DefaultLowerTimeSig);

            lowerTimeSigCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(lowerTimeSigCombo.ActiveId)) {
                    measure.TimeSignature.Lower = Int32.Parse(lowerTimeSigCombo.ActiveId);
                }
            };
            row.PackStart(new Label("Time Sig. (Lower) :") { Xalign = 0f }, false, false, 0);
            row.PackStart(lowerTimeSigCombo, false, false, 0);

            // Tonalité (ComboBoxText)
            ComboBoxText keyCombo = new();
            foreach (string k in _selectableValues.Tonalities.Keys) {
                keyCombo.Append(k, _selectableValues.Tonalities[k]);
            }
            keyCombo.ActiveId = measure.KeySignature.ToDropDownId() != "" ? measure.KeySignature.ToDropDownId() : _selectableValues.DefaultKeySignature.ToDropDownId();
            keyCombo.Changed += (o, args) => {
                if (!string.IsNullOrEmpty(keyCombo.ActiveId) && _selectableValues.Tonalities.ContainsKey(keyCombo.ActiveId))
                {
                    measure.KeySignature = new(keyCombo.ActiveId);
                }
            };

            row.PackStart(new Label("Tonalité :") { Xalign = 0f }, false, false, 0);
            row.PackStart(keyCombo, false, false, 0);

            // Accords (1 champ texte: accords séparés par espaces, 1 accord par temps)
            Entry chordEntry = new() { Text = measure.ChordSequence.ToLilyPondString() ?? new ChordSequence().ToString(), WidthChars = 24, PlaceholderText = "Accords (ex: C Am Dm G7)" };
            chordEntry.Changed += (o, args) => {
                string? text = chordEntry.Text;
                if (text is null) {
                    measure.ChordSequence = new();
                } else {
                    measure.ChordSequence = new(text);
                }
            };
            row.PackStart(new Label("Accords :") { Xalign = 0f }, false, false, 0);
            row.PackStart(chordEntry, true, true, 0);

            // Editeur de mélodie/cadence
            MelodyMeasureEditor editor = new();
            _melodyMeasureEditorWidgets.Add(editor);
            editor.LoadFromModel(measure.Melody.ToWidgetChords(), null, initialCursor: 0);

            // Handler local : met à jour la measure associée (capture 'measure' et 'editor')
            editor.ContentChanged += (s, e) =>
            {
                MeasureMelody newMeasureMelody = new();
                foreach (WidgetMelodyChord widgetChord in editor.ExportToModel()) {
                    newMeasureMelody.MelodyChords.Add(widgetChord.ToMelodyChord());
                }
                measure.Melody = newMeasureMelody;
            };

            editor.WidthRequest = 250;
            row.PackStart(editor, true, false, 0);
            editor.ShowAll();

            // Paroles (une saisie texte ; mots/syllabes séparés par espaces)
            Entry lyricsEntry = new() { Text = measure.Lyrics ?? "", WidthChars = 24, PlaceholderText = "Paroles (séparées par espaces)" };
            lyricsEntry.Changed += (o, args) => {
                measure.Lyrics = lyricsEntry.Text;
            };
            row.PackStart(new Label("Paroles :") { Xalign = 0f }, false, false, 0);
            row.PackStart(lyricsEntry, true, true, 0);

            // Ajout à la zone parent (assure-toi d'avoir un 'measuresBox' ou équivalent)
            _measuresBox.PackStart(row, false, false, 2);
            _measuresBox.ShowAll();
        }

        private void AddMesureAfter(int index) {
            if (_currentSong == null || _currentSong.Measures == null) {
                return;
            }

            int insertIndex = index + 1;
            if (insertIndex < 0) {
                insertIndex = 0;
            }

            if (insertIndex > _currentSong.Measures.Count) {
                insertIndex = _currentSong.Measures.Count;
            }

            MeasureData m = new() { Index = 0, TimeSignature = new(4, 4), KeySignature = new(NoteStep.C, Alteration.neutral, SongMode.major) };
            _currentSong.Measures.Insert(insertIndex, m);

            // Réindexation
            for (int i = 0; i < _currentSong.Measures.Count; i++) {
                _currentSong.Measures[i].Index = i + 1;
            }

            // Reconstruire la vue
            Widget[] children = _measuresBox.Children;
            for (int i = children.Length - 1; i >= 0; i--) {
                Widget child = children[i];
                _measuresBox.Remove(child);
                child.Destroy();
            }

            _melodyMeasureEditorWidgets = new List<MelodyMeasureEditor>();
            _measureStore.Clear();
            RefreshMeasuresView();

            _ = _statusBar.Push(_statusBarContextId, $"Mesure insérée après {index + 1}. Total mesures : {_currentSong.Measures.Count}.");
        }

        private void AddMesureBefore(int index) {
            if (_currentSong == null || _currentSong.Measures == null) {
                return;
            }

            int insertIndex = index;
            if (insertIndex < 0) {
                insertIndex = 0;
            }

            if (insertIndex > _currentSong.Measures.Count) {
                insertIndex = _currentSong.Measures.Count;
            }

            MeasureData m = new() { Index = 0, TimeSignature = new(4, 4), KeySignature = new(NoteStep.C, Alteration.neutral, SongMode.major) };
            _currentSong.Measures.Insert(insertIndex, m);

            // Réindexation
            for (int i = 0; i < _currentSong.Measures.Count; i++) {
                _currentSong.Measures[i].Index = i + 1;
            }

            // Reconstruire la vue
            Widget[] children = _measuresBox.Children;
            for (int i = children.Length - 1; i >= 0; i--) {
                Widget child = children[i];
                _measuresBox.Remove(child);
                child.Destroy();
            }

            _melodyMeasureEditorWidgets = new List<MelodyMeasureEditor>();
            _measureStore.Clear();
            RefreshMeasuresView();

            _ = _statusBar.Push(_statusBarContextId, $"Mesure insérée avant {index + 1}. Total mesures : {_currentSong.Measures.Count}.");
        }

        private void DeleteMesure(int index) {

            if (_currentSong.Measures.Count <= 1) {
                // Ne pas permettre la suppression si c'est la dernière mesure
                _ = _statusBar.Push(_statusBarContextId, "Impossible de supprimer la dernière mesure.");
                return;
            }

            // Vérification de l'index
            if (_currentSong == null || _currentSong.Measures == null) {
                return;
            }

            int measuresCount = _currentSong.Measures.Count;
            if (index < 0 || index >= measuresCount) {
                return;
            }

            // Suppression dans le modèle
            _currentSong.Measures.RemoveAt(index);

            // Réindexation des mesures restantes
            for (int i = 0; i < _currentSong.Measures.Count; i++) {
                _currentSong.Measures[i].Index = i + 1;
            }

            // Nettoyage des widgets de la vue pour éviter les widgets orphelins
            Widget[] children = _measuresBox.Children;
            for (int i = children.Length - 1; i >= 0; i--) {
                Widget child = children[i];
                _measuresBox.Remove(child);
                child.Destroy();
            }

            // Réinitialiser la liste des éditeurs (elles seront recréées par RefreshMeasuresView)
            _melodyMeasureEditorWidgets = new List<MelodyMeasureEditor>();

            // Reconstruire la ListStore et la vue des mesures
            RefreshMeasuresView();

            // Mettre à jour la barre de statut
            _ = _statusBar.Push(_statusBarContextId, $"Mesure {index + 1} supprimée. Total mesures : {_currentSong.Measures.Count}.");
        }

        private void EditorContentChanged(object? sender, EventArgs e) {
    
            if (sender is null) {
                return;
            }

            MelodyMeasureEditor editor = (MelodyMeasureEditor)sender;

            int measureIndex = _melodyMeasureEditorWidgets.IndexOf(editor);
            if (measureIndex < 0 || measureIndex >= _currentSong.Measures.Count) {
                return;
            }

            MeasureMelody newMeasureMelody = new();
            foreach (WidgetMelodyChord widgetChord in editor.ExportToModel()) {
                newMeasureMelody.MelodyChords.Add(widgetChord.ToMelodyChord());
            }

            _currentSong.Measures[measureIndex].Melody = newMeasureMelody;

            // (Pas de mise à jour du ListStore nécessaire pour la mélodie)
        }

        private void SaveProject() {
            FileChooserDialog dlg = new("Enregistrer projet", this, FileChooserAction.Save, "Annuler", ResponseType.Cancel, "Enregistrer", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                _currentSong.Title = _titleEntry.Text;
                _currentSong.Artist = _artistEntry.Text;
                _currentSong.Comment = _commentEntry.Buffer.Text;
                using FileStream fs = new(dlg.Filename, FileMode.Create);
                XmlSerializer ser = new(typeof(Song));
                ser.Serialize(fs, _currentSong);
            }
            dlg.Destroy();
        }

        private void LoadProject()
        {
            FileChooserDialog dlg = new("Ouvrir projet", this, FileChooserAction.Open, "Annuler", ResponseType.Cancel, "Ouvrir", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                _measureStore.Clear();

                using FileStream fs = new(dlg.Filename, FileMode.Open);
                XmlSerializer ser = new(typeof(Song));
                
                Object? obj = ser.Deserialize(fs);
                if (obj is not null) {
                    _currentSong = (Song)obj;
                    _titleEntry.Text = _currentSong.Title;
                    _artistEntry.Text = _currentSong.Artist;
                    _commentEntry.Buffer.Text = _currentSong.Comment;
                    
                    RefreshMeasuresView();
                }
                
            }
            dlg.Destroy();
        }

        private void ExportLilyPond() {
            FileChooserDialog dlg = new("Exporter LilyPond", this, FileChooserAction.Save, "Annuler", ResponseType.Cancel, "Exporter", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                LilypondFileBuilder builder = new(_currentSong);
                builder.GenerateLilypondFile(dlg.Filename);
            }
            dlg.Destroy();
        }               

        private MelodyMeasureEditor? GetFocusedMelodyEditor() {
            foreach (MelodyMeasureEditor melodyMeasureEditorWidget in _melodyMeasureEditorWidgets) {
                if (melodyMeasureEditorWidget.HasFocus) {
                    return melodyMeasureEditorWidget;
                }
            }

            return null;
        }

    }
}
