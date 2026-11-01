using EZSong.Enums;
using EZSong.Exporting.Lilypond;
using EZSong.Helpers;
using EZSong.IO;
using EZSong.MIDI;
using EZSong.Model;
using EZSong.Settings;
using EZSong.UI.UX;
using EZSong.UI.Widgets;
using EZSong.UI.Widgets.WidgetsData;
using Gtk;

namespace EZSong.UI {

    /// <summary>
    /// Main window of the EZSong application, responsible for managing the user interface and interactions with the song model.
    /// </summary>
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

        private GlobalSegmentEditor _globalSegmentEditor;
        private Label _displayedSegmentNumber;
        private Label _displayedSegmentName;




        private Statusbar _statusBar;
        private uint _statusBarContextId;



        /// <summary>
        /// Constructor for the MainWindow class. 
        /// Initializes the main window, 
        /// sets up the user interface, 
        /// and configures event handlers for user interactions.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
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

            Box globalBox = new(Orientation.Vertical, 0);

            //Bloc principal
            Box mainBox = new(Orientation.Horizontal, 0);

            //Barre d'informations (portée affichée, ...)
            Box infoBox = new(Orientation.Vertical, 0);

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

            


            mainBox.PackStart(infoBox, false, false, 0);

            Gtk.Notebook tabs = new() { ShowTabs = true }   ;

            //Infos du morceau
            Box songInfoBox = new(Orientation.Vertical, 0);
            _titleEntry = new Entry {
                PlaceholderText = "Titre du morceau"
            };
            _titleEntry.Changed += (o, e) => {
                _currentSong.Title = _titleEntry.Text;
            };
            songInfoBox.PackStart(_titleEntry, false, false, 0);
            _artistEntry = new Entry {
                PlaceholderText = "Interprète"
            };
            _artistEntry.Changed += (o, e) => {
                _currentSong.Artist = _artistEntry.Text;
            };
            songInfoBox.PackStart(_artistEntry, false, false, 0);
            _commentEntry = new Entry {
                PlaceholderText = "Commentaires"
            };
            _commentEntry.Changed += (o, e) => {
                _currentSong.Comment = _commentEntry.Buffer.Text;
            };
            songInfoBox.PackStart(_commentEntry, false, false, 0);

            // Ajout de la page au Notebook (Contenu, Label de l'onglet)

            _ = tabs.AppendPage(songInfoBox, new Label("Infos du morceau"));

            // Ajout de la page au Notebook (Contenu, Label de l'onglet)
            Box songSegmentsBox = new(Orientation.Vertical, 0);
            _ = tabs.AppendPage(songSegmentsBox, new Label("Découpage en segments"));

            // Ajout de la page au Notebook (Contenu, Label de l'onglet)
            Box segmentInfoBox = new(Orientation.Vertical, 0);
            _ = tabs.AppendPage(segmentInfoBox, new Label("Description du segment"));

            // Mesures
            _globalSegmentEditor = new(_currentSong, _userSettings, _embeddedMidiSynth);

            // Ajout de la page au Notebook (Contenu, Label de l'onglet)
            _ = tabs.AppendPage(_globalSegmentEditor, new Label("Transcription du segment (mesures)"));

            mainBox.PackStart(tabs, true, true, 0); 

            globalBox.PackStart(mainBox, true, true, 0);

            // Barre de status
            _statusBar = new Statusbar();
            _statusBarContextId = _statusBar.GetContextId("main");
            _ = _statusBar.Push(_statusBarContextId, "Prêt.");
            globalBox.PackStart(_statusBar, false, false, 0);

            windowBox.PackStart(_switcher, false, false, 0);
            windowBox.PackStart(_stack, false, false, 0);
            windowBox.PackStart(globalBox, true, true, 0);

            // Signal de redimensionnement
            SizeAllocated += (o, args) => UpdateCompactMode(args.Allocation.Width);

            // Style moderne
            ApplyCss();

            DeleteEvent += (o, args) => Application.Quit();

            GoToFirstSegment();

            _globalSegmentEditor.ResetDisplayedStaffs();

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
                        _globalSegmentEditor.GetFocusedMelodyMeasureEditor();

                    focusedEditor?.OnMidiNoteReceived(notes);
                });
            };
            
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

        /// <summary>
        /// Creates a FlowBox containing buttons for a specific tab in the main window. 
        /// Each button is associated with an action relevant to the tab's functionality.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds a new segment to the current song and navigates to it.
        /// </summary>
        private void AddSegment() {
            _currentSong.AddSegment();
            GoToNextSegment();
        }

        /// <summary>
        /// Adds a new staff to the current song. 
        /// If there is more than one staff, it switches to the secondary staff in the global segment editor.
        /// </summary>
        private void AddStaff() {
            //TODO : faire saisir via une popup très simple
            String staffName = Settings.Constants.DefaultStaffName;
            bool isBass = false;
            _currentSong.AddStaff(staffName, isBass);
            if (_currentSong.SongSettings.StaffsSettings.Staffs.Count > 1) {
                _globalSegmentEditor.UseSecondaryStaff();
            }
        }

        /// <summary>
        /// Navigates to the first segment of the current song and refreshes the displayed segment information in the UI.
        /// </summary>
        private void GoToFirstSegment() {
            _globalSegmentEditor.DisplayedSegmentIndex = 0;
            RefreshDisplayedSegmentInfos();
        }

        /// <summary>
        /// Navigates to the next segment of the current song, looping back to the first segment if currently at the last segment.
        /// </summary>
        private void GoToNextSegment() {
            _globalSegmentEditor.DisplayedSegmentIndex = MathHelper.LoopIndex(_globalSegmentEditor.DisplayedSegmentIndex, _currentSong.Segments.Count(), 1);
            RefreshDisplayedSegmentInfos();
        }

        /// <summary>
        /// Navigates to the previous segment of the current song, looping back to the last segment if currently at the first segment.
        /// </summary>
        private void GoToPreviousSegment() {
            _globalSegmentEditor.DisplayedSegmentIndex = MathHelper.LoopIndex(_globalSegmentEditor.DisplayedSegmentIndex, _currentSong.Segments.Count(), -1);
            RefreshDisplayedSegmentInfos ();
        }

        /// <summary>
        /// Creates a button with an icon and label for use in the FlowBox tabs.
        /// </summary>
        /// <param name="label">The text label for the button.</param>
        /// <param name="onClick">The event handler to invoke when the button is clicked.</param>
        /// <param name="iconName">The name of the SVG icon to display on the button.</param>
        /// <returns>A GTK Widget representing the button.</returns>
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
        
        /// <summary>
        /// Updates the visibility of button labels based on the current compact mode.
        /// </summary>
        /// <param name="width">The current width of the window.</param>
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

        /// <summary>
        /// Applies custom CSS styles to the application using the CSSManager class.
        /// </summary>
        private void ApplyCss() {            
            CSSManager cssManager = new();
            CssProvider cssProvider = new();
            _ = cssProvider.LoadFromData(cssManager.Css); 
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, uint.MaxValue);
        }

        /// <summary>
        /// Saves the current song project to a file chosen by the user through a file chooser dialog.
        /// </summary>
        private void SaveProject() {
            FileChooserDialog dlg = new("Enregistrer projet", this, FileChooserAction.Save, "Annuler", ResponseType.Cancel, "Enregistrer", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                //TODO : revoir la construction du chemin de sauvegarde pour ajouter l'extension .ezsong si nécessaire
                SongPersistancyManager.Save(dlg.Filename, _currentSong);
            }
            dlg.Destroy();
        }

        /// <summary>
        /// Loads a song project from a file chosen by the user through a file chooser dialog and updates the UI accordingly.
        /// </summary>
        private void LoadProject() {
            FileChooserDialog dlg = new("Ouvrir projet", this, FileChooserAction.Open, "Annuler", ResponseType.Cancel, "Ouvrir", ResponseType.Accept);
            if (dlg.Run() == (int)ResponseType.Accept) {
                SetSong(SongPersistancyManager.Load(dlg.Filename));
                UpdateSongInfoUI();
                GoToFirstSegment();                
            }
            dlg.Destroy();
        }

        /// <summary>
        /// Sets the current song in the main window and updates the global segment editor with the new song. 
        /// </summary>
        /// <param name="song">The song to set as the current song.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided song is null.</exception>
        public void SetSong(Song song) {
            _currentSong = song ?? throw new ArgumentNullException(nameof(song));
            _globalSegmentEditor.SetSong(song);
        }

        /// <summary>
        /// Refreshes the displayed segment information in the UI, 
        /// including the segment number and name, 
        /// based on the currently displayed segment index in the global segment editor.
        /// </summary>
        private void RefreshDisplayedSegmentInfos() {
            _displayedSegmentNumber.Text = (_globalSegmentEditor.DisplayedSegmentIndex + 1).ToString();
            _displayedSegmentName.Text = "Segment " + (_globalSegmentEditor.DisplayedSegmentIndex + 1).ToString();
        }

        /// <summary>
        /// Updates the song information UI fields (title, artist, comment) based on the current song's properties.
        /// </summary>
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

        /// <summary>
        /// Exports the current song to a PDF file using a file chooser dialog to select the save location.
        /// </summary>
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
