using EZSong.Enums;
using EZSong.Exporting.Lilypond;
using EZSong.IO;
using EZSong.MIDI;
using EZSong.Model;
using EZSong.Settings;
using EZSong.UI.UX;
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

        private UserSettings _userSettings;

        private MidiInputManager _midiManager;
        private EmbeddedMidiSynth _embeddedMidiSynth; //Pour echo MIDI
        private SoundFontManager _soundFontManager;

        //Pour gérer les onglets : Stack, StackSwitcher et FlowBox
        private Gtk.Stack _stack;
        private StackSwitcher _switcher;
        private FlowBox[] _flowTabs;

        private Entry _titleEntry;
        private Entry _artistEntry;
        private Entry _commentEntry;
        private Label _displayedSegmentNumber;
        private Label _displayedSegmentName;
        private Label _displayedStaffNumber;
        private Label _displayedStaffName;
        private MeasuresEditor _measuresEditor;

        private Statusbar _statusBar;
        private uint _statusBarContextId;

        private int _displayedSegmentIndex = 0;
        private int _displayedStaffIndex = 0;

        public MainWindow() : base("EZSong") {
            _currentSong = new Song ();

            _userSettings = new Settings.UserSettings();

            //Synthé intégré pour l'echo MIDI
            _soundFontManager = new();
            _embeddedMidiSynth = new(_soundFontManager.GetCurrentSoundFontPath(), 0);

            //Detection de la saisie via clavier MIDI
            _midiManager = new MidiInputManager();

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
            _flowTabs = new FlowBox[5];
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

            //Barre d'informations (portée affichée, ...)
            Box infoBox = new(Orientation.Horizontal, 0);

            Label titleDisplayedSegment = new("Segment actuellement affiché :");
            titleDisplayedSegment.StyleContext.AddClass("titleLabel");
            _displayedSegmentNumber = new Label("?");
            _displayedSegmentNumber.StyleContext.AddClass("infoLabel");
            _displayedSegmentName = new Label("?");
            _displayedSegmentName.StyleContext.AddClass("infoLabel");
            infoBox.PackStart(titleDisplayedSegment, false, false, 0);
            infoBox.PackStart(_displayedSegmentNumber, false, false, 0);
            infoBox.PackStart(_displayedSegmentName, false, false, 0);

            //Bouton de suppression du segment
            Button deleteCurrentSegment = new();
            deleteCurrentSegment.Label = "Supprimer ce segment"; //TODO : icone à la place du texte
            deleteCurrentSegment.Clicked += (o, args) => {
                //TODO : demander confirmation avant de supprimer le segment
                throw new NotImplementedException(); //TODO : implémenter la suppression du segment
            };
            infoBox.PackStart(deleteCurrentSegment, false, false, 0);

            //Segment précédent
            Button gotToPreviousSegment = new();
            gotToPreviousSegment.Label = "⏪"; //TODO : icone à la place du texte
            gotToPreviousSegment.Clicked += (o, args) => {
                MainWindow mainWindow = this;
                mainWindow.GoToPreviousSegment();
            };
            infoBox.PackStart(gotToPreviousSegment, false, false, 0);

            //Segment suivant
            Button gotToNextSegment = new();
            gotToNextSegment.Label = "⏩"; //TODO : icone à la place du texte
            gotToNextSegment.Clicked += (o, args) => {
                MainWindow mainWindow = this;
                mainWindow.GoToNextSegment();
            };
            infoBox.PackStart(gotToNextSegment, false, false, 0);

            Label titleDisplayedStaff = new("Portée actuellement affichée :");
            titleDisplayedStaff.StyleContext.AddClass("titleLabel");
            _displayedStaffNumber = new Label("?");
            _displayedStaffNumber.StyleContext.AddClass("infoLabel");
            _displayedStaffName = new Label("?");
            _displayedStaffName.StyleContext.AddClass("infoLabel");
            infoBox.PackStart(titleDisplayedStaff, false, false, 0);
            infoBox.PackStart(_displayedStaffNumber, false, false, 0);
            infoBox.PackStart(_displayedStaffName, false, false, 0);

            //Bouton de suppression de la portée
            Button deleteCurrentStaff = new();
            deleteCurrentStaff.Label = "Supprimer cette portée"; //TODO : icone à la place du texte
            deleteCurrentStaff.Clicked += (o, args) => {
                //TODO : demander confirmation avant de supprimer la portée
                throw new NotImplementedException(); //TODO : implémenter la suppression de la portée
            };
            infoBox.PackStart(deleteCurrentStaff, false, false, 0);

            //Portée précédente
            Button gotToPreviousStaff = new();
            gotToPreviousStaff.Label = "🔺"; //TODO : icone à la place du texte
            gotToPreviousStaff.Clicked += (o, args) => {
                MainWindow mainWindow = this;
                mainWindow.GoToPreviousStaff();
            };
            infoBox.PackStart(gotToPreviousStaff, false, false, 0);

            //Portée suivante
            Button gotToNextStaff = new();
            gotToNextStaff.Label = "🔻"; //TODO : icone à la place du texte
            gotToNextStaff.Clicked += (o, args) => {
                MainWindow mainWindow = this;
                mainWindow.GoToNextStaff();
            };
            infoBox.PackStart(gotToNextStaff, false, false, 0);


            mainBox.PackStart(infoBox, false, false, 0);

            // Mesures
            _measuresEditor = new MeasuresEditor(_userSettings, _embeddedMidiSynth);
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

            GoToFirstSegment();

            GoToFirstStaff();

            ShowAll();




            string? firstDevice = MidiInputManager.GetAvailableDevices().FirstOrDefault();
            if (firstDevice != null) {
                _ = _midiManager.Open(firstDevice);
                Console.WriteLine($"MIDI connecté : {firstDevice}");
            }

            // Quand des notes sont jouées
            _midiManager.NotesPlayed += notes =>
            {
                Gtk.Application.Invoke((s, e) =>
                {
                    MelodyMeasureEditor? focusedEditor =
                        _measuresEditor.GetFocusedMelodyMeasureEditor();

                    focusedEditor?.OnMidiNoteReceived(notes);
                });
            };

            _measuresEditor.SetSong(_currentSong);
            
            Maximize(); // Démarrer en mode maximisé
        }

        private void PopulateMenuTabs() {
            _flowTabs[0] = CreateTab("file");
            _stack.AddTitled(_flowTabs[0], "file", "Fichier");
            _flowTabs[1] = CreateTab("segments");
            _stack.AddTitled(_flowTabs[1], "segments", "Segments");
            _flowTabs[2] = CreateTab("staffs");
            _stack.AddTitled(_flowTabs[2], "staffs", "Portées");
            _flowTabs[3] = CreateTab("mesures");
            _stack.AddTitled(_flowTabs[3], "mesures", "Mesures");
            _flowTabs[4] = CreateTab("export");
            _stack.AddTitled(_flowTabs[4], "export", "Export");
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
                    //TODO : ajouter un bouton "Nouveau projet"
                    flow.Add(CreateIconButton("Ouvrir", (s, e) => LoadProject(), "file-load.svg"));
                    flow.Add(CreateIconButton("Enregistrer", (s, e) => SaveProject(), "file-save.svg"));
                    break;

                case "segments":
                    flow.Add(CreateIconButton("Ajouter", (s, e) => AddSegment(), "icon-placeholder.svg"));
                    break ;

                case "staffs":
                    flow.Add(CreateIconButton("Ajouter", (s, e) => AddStaff(), "icon-placeholder.svg"));
                    
                    break ;

                case "mesures":
                    //TODO : supprimer cet onglet peu utile ?
                    break;

                case "export":
                    flow.Add(CreateIconButton("Exporter au format PDF", (s, e) => ExportPdf(), "export-pdf.svg"));
                    break;

            }

            return flow;
        }

        private void AddSegment() {
            _currentSong.AddSegment();
            GoToNextSegment();
        }

        private void AddStaff() {
            //TODO : faire saisir via une popup très simple
            String staffName = Settings.Constants.DefaultStaffName;
            bool isBass = false;
            _currentSong.AddStaff(staffName, isBass);
            GoToNextStaff();
        }

        private void GoToFirstSegment() {
            _displayedSegmentIndex = 0;
            RefreshDisplayedSegment();
        }

        private void GoToNextSegment() {
            _displayedSegmentIndex = LoopIndex(_displayedSegmentIndex, _currentSong.Segments.Count(), 1);
            RefreshDisplayedSegment();
        }

        private void GoToPreviousSegment() {
            _displayedSegmentIndex = LoopIndex(_displayedSegmentIndex, _currentSong.Segments.Count(), -1);
            RefreshDisplayedSegment ();
        }

        private void GoToFirstStaff() {
            _displayedStaffIndex = 0;
            RefreshDisplayedStaff();
        }

        private void GoToNextStaff() {
            _displayedStaffIndex = LoopIndex(_displayedStaffIndex, _currentSong.SongSettings.StaffsSettings.Staffs.Count(), 1);
            RefreshDisplayedStaff();
        }

        private void GoToPreviousStaff() {
            _displayedStaffIndex = LoopIndex(_displayedStaffIndex, _currentSong.SongSettings.StaffsSettings.Staffs.Count(), -1);
            RefreshDisplayedStaff();
        }

        private int LoopIndex(int index, int count, int offset) {
            if (count <= 0) {
                throw new ArgumentException("Count must be greater than zero.", nameof(count));
            }
            int result = (index + offset) % count;
            if (result < 0) {
                result += count;
            }
            return result;
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
            CSSManager cssManager = new();
            CssProvider cssProvider = new();
            _ = cssProvider.LoadFromData(cssManager.Css); 
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, uint.MaxValue);
        }

        private void SaveProject() {
            FileChooserDialog dlg = new("Enregistrer projet", this, FileChooserAction.Save, "Annuler", ResponseType.Cancel, "Enregistrer", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                //TODO : revoir la construction du chemin de sauvegarde pour ajouter l'extension .ezsong si nécessaire
                SongPersistancyManager.Save(dlg.Filename, _currentSong);
            }
            dlg.Destroy();
        }

        private void LoadProject() {
            FileChooserDialog dlg = new("Ouvrir projet", this, FileChooserAction.Open, "Annuler", ResponseType.Cancel, "Ouvrir", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                SetSong(SongPersistancyManager.Load(dlg.Filename));
                GoToFirstSegment();
                GoToFirstStaff();
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
            UpdateSongInfoUI();
            _measuresEditor.Refresh();
        }

        private void RefreshDisplayedSegment() {
            _displayedSegmentNumber.Text = (_displayedSegmentIndex + 1).ToString();
            _displayedSegmentName.Text = "Segment " + (_displayedSegmentIndex + 1).ToString();
            _measuresEditor.RefreshDisplayedSegment(_displayedSegmentIndex);
        }

        private void RefreshDisplayedStaff() {
            _displayedStaffNumber.Text = (_displayedStaffIndex + 1).ToString();
            _displayedStaffName.Text = _currentSong.SongSettings.StaffsSettings.Staffs[_displayedStaffIndex].Name;
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
                //TODO : revoir la construction du chemin de sauvegarde pour ajouter l'extension .pdf si nécessaire
                LilypondFileBuilder builder = new(_currentSong);
                builder.GeneratePdfFile(dlg.Filename);
            }
            dlg.Destroy();
        }
    }
}
