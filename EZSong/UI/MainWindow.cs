using EZSong.Enums;
using EZSong.Exporting.Lilypond;
using EZSong.IO;
using EZSong.MIDI;
using EZSong.Model;
using EZSong.UI.Widgets;
using EZSong.UI.Widgets.WidgetsData;
using Gtk;

namespace EZSong.UI {

    // Simple GTK UI
    public class MainWindow : Gtk.Window {
        private const int _initialWidth = 900;
        private const int _initialHeight = 600;
        private const int _compactThreshold = 800;

        private Song _currentSong;

        private MidiInputManager _midiManager;

        //Pour gérer les onglets : Stack, StackSwitcher et FlowBox
        private Gtk.Stack _stack;
        private StackSwitcher _switcher;
        private FlowBox[] _flowTabs;

        private Entry _titleEntry;
        private Entry _artistEntry;
        private Entry _commentEntry;
        private MeasuresEditor _measuresEditor;

        private Statusbar _statusBar;
        private uint _statusBarContextId;

        private int _displayedStaffIndex = 0;

        public MainWindow() : base("EZSong") {
            _currentSong = new Song ();

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
            _titleEntry.Changed += (o, e) => {
                _currentSong.Title = _titleEntry.Text;
            };
            headerBox.PackStart(_titleEntry, true, true, 0);
            _artistEntry = new Entry { 
                PlaceholderText = "Interprète" 
            };            
            _artistEntry.Changed += (o, e) => {
                _currentSong.Artist = _artistEntry.Text;
            };
            headerBox.PackStart(_artistEntry, true, true, 0);
            _commentEntry = new Entry { 
                PlaceholderText = "Commentaires" 
            };
            _commentEntry.Changed += (o, e) => {
                _currentSong.Comment = _commentEntry.Buffer.Text;
            };
            headerBox.PackStart(_commentEntry, true, true, 0);
            mainBox.PackStart(headerBox, false, false, 0);

            // Mesures
            _measuresEditor = new MeasuresEditor();
            ScrolledWindow scrolled = new();
            scrolled.Add(_measuresEditor);
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
                MelodyMeasureEditor? focusedEditor = _measuresEditor.GetFocusedGlobalMelodyEditor();
                if (focusedEditor != null) {
                    Gtk.Application.Invoke((s, e) =>  // nécessaire car le callback MIDI n’est pas sur le thread GTK
                    {
                        focusedEditor.OnMidiNoteReceived(notes);
                    });
                }
            };

            _measuresEditor.SetSong(_currentSong);
            _measuresEditor.AppendBlankMeasures(1); // mesure par défaut
            
            Maximize(); // Démarrer en mode maximisé
        }

        private void PopulateMenuTabs() {
            _flowTabs[0] = CreateTab("file");
            _stack.AddTitled(_flowTabs[0], "file", "Fichier");
            _flowTabs[0] = CreateTab("staffs");
            _stack.AddTitled(_flowTabs[0], "staffs", "Portées");
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

                case "staffs":
                    flow.Add(CreateIconButton("Ajouter", (s, e) => AddStaff(), "icon-placeholder.svg"));
                    flow.Add(CreateIconButton("Aller à la suivante", (s, e) => GoToNextStaff(), "icon-placeholder.svg"));
                    flow.Add(CreateIconButton("Aller à la précédente", (s, e) => GoToPreviousStaff(), "icon-placeholder.svg"));
                    break ;

                case "mesures":
                    break;

                case "export":
                    flow.Add(CreateIconButton("Exporter au format PDF", (s, e) => ExportPdf(), "export-pdf.svg"));
                    break;

            }

            return flow;
        }

        private void AddStaff() {
            _currentSong.AddStaff();
            GoToNextStaff();
        }

        private void GoToNextStaff() {
            _displayedStaffIndex = (_displayedStaffIndex + 1) % _currentSong.SongSettings.StaffsSettings.Staffs.Count();
            RefreshDisplayedStaff();
        }

        private void GoToPreviousStaff() {
            _displayedStaffIndex = (_displayedStaffIndex + 1) % _currentSong.SongSettings.StaffsSettings.Staffs.Count();
            RefreshDisplayedStaff();
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

                button.glyph {
                    font-family: 'Bravura'; 
                    font-size: 20px;
                }

                
                "; //TODO : rendre la police de glyph dynamique en fonction des paramètres utilisateur
            CssProvider cssProvider = new();
            _ = cssProvider.LoadFromData(css);
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, uint.MaxValue);
        }

        

        private void SaveProject() {
            FileChooserDialog dlg = new("Enregistrer projet", this, FileChooserAction.Save, "Annuler", ResponseType.Cancel, "Enregistrer", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                SongPersistancyManager.Save(dlg.Filename, _currentSong);
            }
            dlg.Destroy();
        }

        private void LoadProject() {
            FileChooserDialog dlg = new("Ouvrir projet", this, FileChooserAction.Open, "Annuler", ResponseType.Cancel, "Ouvrir", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {

                SetSong(SongPersistancyManager.Load(dlg.Filename));

                RefreshUI();

            }
            dlg.Destroy();
        }

        public void SetSong(Song song) {
            _currentSong = song ?? throw new ArgumentNullException(nameof(song));
            _measuresEditor.SetSong(_currentSong);
            RefreshUI();
        }

        private void RefreshUI() {

            //_measuresEditor.Refresh();

            UpdateSongInfoUI();
            _measuresEditor.Refresh();
        }

        private void RefreshDisplayedStaff() {
            _measuresEditor.RefreshDisplayedStaff(_displayedStaffIndex);
        }

        private void UpdateSongInfoUI() {
            if (_currentSong != null) {
                _titleEntry.Text = _currentSong.Title ?? string.Empty;
                _artistEntry.Text = _currentSong.Artist ?? string.Empty;
                _commentEntry.Text = _currentSong.Comment ?? string.Empty;
            } else {
                _titleEntry.Text = string.Empty;
                _artistEntry.Text = string.Empty;
                _commentEntry.Text = string.Empty;
            }
        }

        private void ExportPdf() {
            FileChooserDialog dlg = new("Exporter en PDF", this, FileChooserAction.Save, "Annuler", ResponseType.Cancel, "Exporter", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                LilypondFileBuilder builder = new(_currentSong);
                builder.GeneratePdfFile(dlg.Filename);
            }
            dlg.Destroy();
        }

    }
}
