using EZSong.Enums;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
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

            // Le numéro du segment affiché doit suivre tous les changements
            // internes (navigation, scission, ...), pas seulement les
            // commandes de navigation de la fenêtre principale.
            _globalSegmentEditor.DisplayedSegmentChanged += (int segmentIndex) => {
                RefreshDisplayedSegmentInfos();
            };

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
                    flow.Add(CreateIconButton("Importer MIDI/KAR", (s, e) => ImportMidiProject(), "file-load.svg"));
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
        /// Initialise un projet à partir d'un fichier MIDI/KAR. Les pistes mélodiques sont
        /// choisies graphiquement (une ou deux), puis les notes sont quantifiées sur une
        /// grille adaptée au contenu (1/4, 1/8 ou 1/16 de ronde).
        /// Les accords sont calculés une seule fois pendant l'import et les paroles KAR
        /// sont réparties dans les mesures selon leur horodatage MIDI. Les textes sont
        /// décodés en UTF-8 ou Windows-1252 afin de préserver les accents.
        /// </summary>
        private void ImportMidiProject() {
            string fileName;

            FileChooserDialog chooser = new(
                "Importer un fichier MIDI/KAR",
                this,
                FileChooserAction.Open,
                "Annuler", ResponseType.Cancel,
                "Importer", ResponseType.Accept);

            try {
                FileFilter filter = new() {
                    Name = "Fichiers MIDI / Karaoké"
                };
                filter.AddPattern("*.mid");
                filter.AddPattern("*.midi");
                filter.AddPattern("*.kar");
                chooser.AddFilter(filter);

                if (chooser.Run() != (int)ResponseType.Accept) {
                    return;
                }

                fileName = chooser.Filename;
            } finally {
                chooser.Destroy();
            }

            try {
                Console.WriteLine("MIDI 1 : lecture du fichier");
                // DryWetMidi décode par défaut les textes en US-ASCII, ce qui
                // remplace les accents par des '?'. On utilise donc un décodeur
                // qui essaie UTF-8 puis Windows-1252/Latin-1.
                MidiFile midi = MidiFile.Read(fileName, MidiTextDecoder.CreateReadingSettings());
                Console.WriteLine("MIDI 2 : lecture des pistes");
                List<TrackChunk> tracks = midi.GetTrackChunks().ToList();
                if (tracks.Count == 0) {
                    throw new InvalidDataException("Le fichier ne contient aucune piste MIDI.");
                }
                Console.WriteLine("MIDI 3 : affichage du choix des pistes");

                // Une liste de cases à cocher permet de sélectionner au plus deux pistes.
                Dialog trackDialog = new("Choix des pistes à importer", this, DialogFlags.Modal,
                    "Annuler", ResponseType.Cancel, "Continuer", ResponseType.Accept);
                trackDialog.SetDefaultSize(460, 360);
                Box content = (Box)trackDialog.ContentArea;
                content.Spacing = 6;
                content.PackStart(new Label("Sélectionnez une ou deux pistes à convertir en portée :") { Xalign = 0 }, false, false, 6);
                List<CheckButton> choices = new();
                for (int i = 0; i < tracks.Count; i++) {
                    string? trackName = tracks[i].Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
                    int noteCount = tracks[i].GetNotes().Count();
                    CheckButton check = new($"Piste {i + 1} — {(string.IsNullOrWhiteSpace(trackName) ? "Sans nom" : trackName)} ({noteCount} notes)");
                    check.Toggled += (_, _) => {
                        if (check.Active && choices.Count(c => c.Active) > 2) {
                            check.Active = false;
                        }
                    };
                    choices.Add(check);
                    content.PackStart(check, false, false, 2);
                }
                if (choices.Count > 0) {
                    choices[0].Active = true;
                }

                content.ShowAll();
                int response = trackDialog.Run();
                List<int> selected = choices.Select((c, i) => (c, i)).Where(x => x.c.Active).Select(x => x.i).ToList();
                trackDialog.Destroy();
                if (response != (int)ResponseType.Accept) {
                    return;
                }

                if (selected.Count is < 1 or > 2) {
                    throw new InvalidOperationException("Choisissez une ou deux pistes.");
                }

                Console.WriteLine("MIDI 4 : début de l'import des pistes");

                int gridSubdivision = ImportSelectedMidiTracks(midi, tracks, selected, System.IO.Path.GetFileNameWithoutExtension(fileName));

                Console.WriteLine("MIDI 5 : fin de l'import des pistes");

                Console.WriteLine("MIDI 6 : actualisation de l'interface");

                UpdateSongInfoUI();
                GoToFirstSegment();
                _globalSegmentEditor.ResetDisplayedStaffs();
                _globalSegmentEditor.Refresh();

                Console.WriteLine("MIDI 7 : actualisation terminée");

                _ = _statusBar.Push(_statusBarContextId,
                    $"Import MIDI terminé : {selected.Count} portée(s), grille 1/{gridSubdivision * 4}.");
            } catch (Exception ex) {
                MessageDialog error = new(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close,
                    "Impossible d'importer le fichier MIDI/KAR : " + ex.Message);
                _ = error.Run(); error.Destroy();
            }
        }

        private int ImportSelectedMidiTracks(MidiFile midi, List<TrackChunk> tracks, List<int> selectedTracks, string title) {
            if (midi.TimeDivision is not TicksPerQuarterNoteTimeDivision tpq) {
                throw new NotSupportedException("Les fichiers MIDI en division SMPTE ne sont pas pris en charge ; utilisez une division en ticks par noire.");
            }

            long ticksPerQuarter = tpq.TicksPerQuarterNote;
            List<MidiQuantizer.TimeSignatureChange> timeSignatureChanges = ReadTimeSignatureChanges(tracks);
            Model.TimeSignature defaultTimeSignature = GetInitialTimeSignature(timeSignatureChanges);

            // Deux jeux de notes distincts : les pistes choisies pour la transcription,
            // et toutes les pistes pour l'analyse harmonique.
            List<MidiQuantizer.Note>[] staffNotes = new List<MidiQuantizer.Note>[selectedTracks.Count];
            for (int i = 0; i < staffNotes.Length; i++) {
                staffNotes[i] = new List<MidiQuantizer.Note>();
            }
            List<(long Start, long End, int Pitch, int Track, int Channel)> allMidiNotes = new();
            for (int trackIndex = 0; trackIndex < tracks.Count; trackIndex++) {
                TrackChunk track = tracks[trackIndex];
                int staffIndex = selectedTracks.IndexOf(trackIndex);
                foreach (Note note in track.GetNotes()) {
                    long start = Math.Max(0, note.Time);
                    long end = Math.Max(start + 1, note.Time + note.Length);
                    int pitch = (int)(byte)note.NoteNumber;
                    // Le canal 10 (index 9) est réservé aux percussions : ses
                    // notes ne décrivent pas l'harmonie et fausseraient la
                    // détection des accords.
                    if (note.Channel != 9) {
                        allMidiNotes.Add((start, end, pitch, trackIndex, (int)note.Channel));
                    }
                    if (staffIndex >= 0) {
                        staffNotes[staffIndex].Add(new MidiQuantizer.Note(start, end, pitch));
                    }
                }
            }
            // Les paroles sont lues dans toutes les pistes, indépendamment des
            // pistes choisies pour la transcription.
            List<MidiLyricsExtractor.Lyric> lyrics = MidiLyricsExtractor.Extract(tracks, ticksPerQuarter / 8);
            // L'armure MIDI sert d'abord à choisir l'orthographe des accords
            // (bémols ou dièses). Si elle est vide (0 altération), on déduit
            // l'armure des notes réellement jouées : c'est le cas de nombreux
            // fichiers karaoké qui déclarent do majeur par défaut.
            sbyte? midiKeySignature = ReadInitialKeySignature(tracks);
            bool preferFlats = PreferFlatSpelling(allMidiNotes, midiKeySignature);
            // Tonalité calculée automatiquement à partir des notes jouées.
            KeySignature autoKeySignature = KeySignatureDetector.DetectFromNotes(
                allMidiNotes.Select(n => (n.Start, n.End, n.Pitch, n.Track))) ?? new KeySignature();
            // Les notes sont pondérées une seule fois : les voix mélodiques
            // (chant, flûte, lead…) comptent moins que l'accompagnement dans
            // le calcul des accords.
            List<MidiChordAnalyzer.MidiNote> harmonicNotes =
                SelectHarmonicAnalysisNotes(allMidiNotes, ReadVoicePrograms(tracks));
            // Les syllabes sont regroupées en mots avant la répartition dans
            // les mesures : un mot traversant une barre de mesure n'est ainsi
            // jamais coupé en deux.
            List<MidiLyricsExtractor.Lyric> lyricWords = MidiLyricsExtractor.GroupWords(lyrics);
            if (staffNotes.All(notes => notes.Count == 0)) {
                throw new InvalidDataException("Les pistes sélectionnées ne contiennent aucune note.");
            }

            List<MidiQuantizer.Note> importedNotes = staffNotes.SelectMany(notes => notes).ToList();
            IEnumerable<Model.TimeSignature> timeSignatures = timeSignatureChanges
                .Select(change => new Model.TimeSignature(change.Beats, change.BeatUnit))
                .Append(defaultTimeSignature)
                .DistinctBy(timeSignature => (timeSignature.Beats, timeSignature.BeatUnit));
            // La grille est choisie une seule fois pour toutes les portées afin que
            // les deux voix restent alignées.
            int gridSubdivision = MidiQuantizer.ChooseGridSubdivision(importedNotes, ticksPerQuarter, timeSignatures);
            MidiQuantizer.StaffQuantization[] staffQuantizations = staffNotes
                .Select(notes => MidiQuantizer.Quantize(notes, ticksPerQuarter, gridSubdivision))
                .ToArray();

            // Les mesures couvrent la dernière note quantifiée (qui peut dépasser
            // légèrement la dernière note brute) et le dernier événement de paroles.
            double lastNoteTick = staffQuantizations
                .SelectMany(quantization => quantization.Notes
                    .Select(note => note.EndUnit * quantization.GridTicks))
                .DefaultIfEmpty(0)
                .Max();
            double lastLyricTick = lyrics.Count == 0 ? 0 : lyrics.Max(lyric => lyric.Time) + 1;
            List<MidiQuantizer.MeasureSpan> measureSpans = MidiQuantizer.BuildMeasureSpans(
                timeSignatureChanges, Math.Max(lastNoteTick, lastLyricTick), ticksPerQuarter, defaultTimeSignature);

            Song song = new();
            song.Title = title;
            song.SongSettings.StaffsSettings.Staffs.Clear();
            foreach (int sourceIndex in selectedTracks) {
                
                string? name = tracks[sourceIndex].Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
                song.SongSettings.StaffsSettings.Staffs.Add(new StaffSettings(
                    string.IsNullOrWhiteSpace(name) ? $"Piste {sourceIndex + 1}" : name,
                    _userSettings.MidiInputDefaultVoice, Constants.MelodyBaseOctave));
            }

            List<MeasureData> allMeasures = new();
            for (int measureIndex = 0; measureIndex < measureSpans.Count; measureIndex++) {
                MidiQuantizer.MeasureSpan span = measureSpans[measureIndex];
                List<MeasureGlobalMelody> staffs = new();
                for (int staffIndex = 0; staffIndex < selectedTracks.Count; staffIndex++) {
                    MeasureGlobalMelody global = new(staffIndex);
                    global.Melody = new MeasureMelody(staffIndex, staffQuantizations[staffIndex].BuildMelody(span));
                    global.Pattern = staffQuantizations[staffIndex].BuildPattern(staffIndex, span);
                    staffs.Add(global);
                }
                string measureLyrics = MidiLyricsExtractor.Format(lyricWords
                    .Where(word => span.Contains(word.Time))
                    .OrderBy(word => word.Time));
                ChordSequence chordSequence = new();
                chordSequence.InitializeFromTimeSignature(span.TimeSignature);
                MeasureData measure = new(measureIndex, song.SongSettings, span.TimeSignature, new KeySignature(autoKeySignature.Note, autoKeySignature.Alteration, autoKeySignature.Mode),
                    chordSequence, staffs, measureLyrics);
                // Calcul harmonique effectué à l'import uniquement : un accord par temps à partir
                // des classes de hauteur présentes dans la fenêtre du temps.
                CalculateMeasureChords(measure, harmonicNotes, span.StartTicks, span.DurationTicks,
                    ticksPerQuarter, preferFlats);
                allMeasures.Add(measure);
            }

            // Tonalité initiale : on ignore les mesures vides en tête et on
            // détecte la tonalité à partir des premières mesures contenant des
            // notes, puis on applique cette tonalité à toutes les mesures.
            KeySignature initialKey = autoKeySignature;
            // Trouver la première mesure non vide.
            int firstNonEmpty = -1;
            for (int i = 0; i < allMeasures.Count; i++) {
                bool hasContent = allMeasures[i].ChordSequence.ChordBeats
                    .SelectMany(b => b.Chords).Any(c => !c.IsSilentChord)
                    || allMeasures[i].Staffs.SelectMany(g => g.Melody.MelodyChords).Any();
                if (hasContent) { firstNonEmpty = i; break; }
            }
            if (firstNonEmpty >= 0) {
                // Prendre jusqu'à 16 mesures à partir de la première non vide.
                int endIdx = Math.Min(allMeasures.Count, firstNonEmpty + 16);
                double secStart = measureSpans[firstNonEmpty].StartTicks;
                double secEnd = measureSpans[endIdx - 1].StartTicks + measureSpans[endIdx - 1].DurationTicks;
                List<(long Start, long End, int Pitch, int Track)> secNotes = harmonicNotes
                    .Where(n => n.Start < secEnd && n.End > secStart)
                    .Select(n => (n.Start, n.End, n.Pitch, n.Track))
                    .ToList();
                KeySignature? detected = secNotes.Count > 0
                    ? KeySignatureDetector.DetectFromNotes(secNotes)
                    : null;
                if (detected is not null) {
                    initialKey = detected;
                }
            }
            foreach (MeasureData m in allMeasures) {
                m.KeySignature = new KeySignature(initialKey.Note, initialKey.Alteration, initialKey.Mode);
            }
            // Un seul segment : on conserve toutes les mesures (y compris les
            // mesures vides en tête), afin de ne pas découper artificiellement
            // le morceau. La structure (intro, couplet, refrain, pont) est
            // détectée pour la tonalité, pas pour le découpage.
            SegmentData segment = new(0, song.SongSettings, allMeasures);
            for (int i = 0; i < segment.Measures.Count; i++) {
                segment.Measures[i].PrecedingMeasure = i > 0 ? segment.Measures[i - 1] : null;
                segment.Measures[i].FollowingMeasure = i + 1 < segment.Measures.Count ? segment.Measures[i + 1] : null;
            }
            song.Segments.Clear();
            song.Segments.Add(segment);
            _currentSong = song;
            _globalSegmentEditor.SetSong(song);
            return gridSubdivision;
        }

        /// <summary>
        /// Lit les changements de signature rythmique avec leur date absolue en ticks.
        /// </summary>
        private static List<MidiQuantizer.TimeSignatureChange> ReadTimeSignatureChanges(List<TrackChunk> tracks) {
            List<MidiQuantizer.TimeSignatureChange> changes = new();
            foreach (TrackChunk track in tracks) {
                long absoluteTime = 0;
                foreach (MidiEvent ev in track.Events) {
                    absoluteTime += ev.DeltaTime;
                    if (ev is TimeSignatureEvent timeSignature && timeSignature.Numerator > 0 && timeSignature.Denominator > 0) {
                        changes.Add(new MidiQuantizer.TimeSignatureChange(
                            absoluteTime, timeSignature.Numerator, timeSignature.Denominator));
                    }
                }
            }
            return changes;
        }

        /// <summary>
        /// Lit la première armure MIDI (nombre d'altérations : positif = dièses,
        /// négatif = bémols), ou null si le fichier n'en contient pas.
        /// </summary>
        private static sbyte? ReadInitialKeySignature(List<TrackChunk> tracks) {
            foreach (TrackChunk track in tracks) {
                foreach (MidiEvent midiEvent in track.Events) {
                    if (midiEvent is KeySignatureEvent keySignature) {
                        return keySignature.Key;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Détermine l'orthographe des altérations à utiliser pour les noms
        /// d'accords. L'armure MIDI est prioritaire ; lorsqu'elle est neutre
        /// (do majeur / la mineur déclaré par défaut), l'armure est déduite des
        /// notes jouées afin d'écrire par exemple si bémol plutôt que la dièse.
        /// </summary>
        private static bool PreferFlatSpelling(
            List<(long Start, long End, int Pitch, int Track, int Channel)> notes,
            sbyte? midiKeySignature) {
            if (midiKeySignature is sbyte key) {
                if (key < 0) {
                    return true;
                }
                if (key > 0) {
                    return false;
                }
            }

            double[] weights = new double[12];
            foreach ((long Start, long End, int Pitch, int Track, int Channel) note in notes) {
                weights[((note.Pitch % 12) + 12) % 12] += 1.0;
            }

            // Meilleure gamme majeure : on compare les notes qui appartiennent
            // à chaque gamme à celles qui en sortent. Les gammes relatives
            // (majeure / mineure) partagent les mêmes altérations, ce qui suffit
            // pour choisir entre bémols et dièses.
            int bestTonic = 0;
            double bestScore = double.NegativeInfinity;
            for (int tonic = 0; tonic < 12; tonic++) {
                double inside = 0;
                double outside = 0;
                for (int pitchClass = 0; pitchClass < 12; pitchClass++) {
                    if (IsInMajorScale(pitchClass, tonic)) {
                        inside += weights[pitchClass];
                    } else {
                        outside += weights[pitchClass];
                    }
                }

                double score = inside - outside * 0.5;
                if (score > bestScore) {
                    bestScore = score;
                    bestTonic = tonic;
                }
            }

            return bestTonic switch {
                // Tonalités à bémols les plus courantes (réb, mib, fa, lab, sib).
                1 or 3 or 5 or 8 or 10 => true,
                // Tonalités à dièses (ré, mi, fa#, sol, la, si).
                2 or 4 or 6 or 7 or 9 or 11 => false,
                // Do majeur / la mineur : on regarde les altérations
                // accidentelles réellement présentes.
                _ => weights[1] + weights[6] + weights[8]
                     <= weights[3] + weights[10]
            };
        }

        private static bool IsInMajorScale(int pitchClass, int tonic) {
            int degree = ((pitchClass - tonic) % 12 + 12) % 12;
            return degree is 0 or 2 or 4 or 5 or 7 or 9 or 11;
        }

        /// <summary>
        /// Convertit une classe de hauteur en nom de note, avec des bémols ou
        /// des dièses selon l'armure retenue.
        /// </summary>
        private static (NoteStep Note, Alteration Alteration) GetChordNoteSpelling(
            int pitchClass, bool preferFlats) {
            return (((pitchClass % 12) + 12) % 12) switch {
                0 => (NoteStep.C, Alteration.neutral),
                1 => preferFlats ? (NoteStep.D, Alteration.flat) : (NoteStep.C, Alteration.sharp),
                2 => (NoteStep.D, Alteration.neutral),
                3 => preferFlats ? (NoteStep.E, Alteration.flat) : (NoteStep.D, Alteration.sharp),
                4 => (NoteStep.E, Alteration.neutral),
                5 => (NoteStep.F, Alteration.neutral),
                6 => preferFlats ? (NoteStep.G, Alteration.flat) : (NoteStep.F, Alteration.sharp),
                7 => (NoteStep.G, Alteration.neutral),
                8 => preferFlats ? (NoteStep.A, Alteration.flat) : (NoteStep.G, Alteration.sharp),
                9 => (NoteStep.A, Alteration.neutral),
                10 => preferFlats ? (NoteStep.B, Alteration.flat) : (NoteStep.A, Alteration.sharp),
                _ => (NoteStep.B, Alteration.neutral)
            };
        }

        /// <summary>
        /// Orthographie la fondamentale d'un accord. En armure à bémols, un
        /// accord mineur ou diminué sur sol bémol n'existe pas en pratique
        /// (il faudrait des doubles bémols) : on écrit alors fa dièse.
        /// </summary>
        private static (NoteStep Note, Alteration Alteration) GetChordRootSpelling(
            int pitchClass, ChordType type, bool preferFlats) {
            if ((((pitchClass % 12) + 12) % 12) == 6
                && preferFlats
                && IsMinorFamily(type)) {
                return (NoteStep.F, Alteration.sharp);
            }

            return GetChordNoteSpelling(pitchClass, preferFlats);
        }

        private static bool IsMinorFamily(ChordType type) {
            return type is ChordType.Minor
                or ChordType.MinorSixth
                or ChordType.MinorSeventh
                or ChordType.MinorMajorSeventh
                or ChordType.MinorNinth
                or ChordType.MinorEleventh
                or ChordType.MinorThirteenth
                or ChordType.Diminished
                or ChordType.DiminishedSeventh
                or ChordType.MinorSeventhFlatFive;
        }

        /// <summary>
        /// Retourne la signature rythmique en vigueur au début du fichier (4/4 par défaut).
        /// </summary>
        private static Model.TimeSignature GetInitialTimeSignature(List<MidiQuantizer.TimeSignatureChange> changes) {
            MidiQuantizer.TimeSignatureChange initial = changes
                .Where(change => change.Time <= 0)
                .OrderBy(change => change.Time)
                .FirstOrDefault();
            return initial.Beats > 0 && initial.BeatUnit > 0
                ? new Model.TimeSignature(initial.Beats, initial.BeatUnit)
                : new Model.TimeSignature(4, 4);
        }

        /// <summary>
        /// Déduit les accords de toutes les pistes MIDI, indépendamment des pistes
        /// sélectionnées pour la transcription.
        /// </summary>
        private static void CalculateMeasureChords(MeasureData measure,
            List<MidiChordAnalyzer.MidiNote> harmonicNotes, double measureStart,
            double measureTicks, long ticksPerQuarter, bool preferFlats) {
            if (harmonicNotes.Count == 0) {
                return;
            }

            double measureEnd = measureStart + measureTicks;
            List<MidiChordAnalyzer.MidiNote> notesInMeasure = harmonicNotes
                .Where(n => n.Start < measureEnd && n.End > measureStart)
                .ToList();
            if (notesInMeasure.Count == 0) {
                return;
            }

            RhythmRationalDuration beatDuration = measure.TimeSignature.GetBeatDuration().ApplyDots();
            double beatTicks = ticksPerQuarter * 4.0 * beatDuration.Numerator / beatDuration.Denominator;
            int beatCount = measure.TimeSignature.GetBeatCount();
            MidiChordAnalyzer.DetectedChord? previousChord = null;

            for (int beat = 0; beat < beatCount; beat++) {
                double beatStart = measureStart + beat * beatTicks;
                double beatEnd = beatStart + beatTicks;

                // L'analyseur ne renvoie un accord que si les notes réellement
                // jouées pendant le temps (et, pour les arpèges, pendant la
                // mesure) le justifient. Un temps sans harmonie claire reste
                // vide plutôt que de recevoir un accord inventé.
                MidiChordAnalyzer.DetectedChord? detected = MidiChordAnalyzer.Analyze(
                    notesInMeasure, beatStart, beatEnd, measureStart, measureEnd, previousChord);
                if (detected is null) {
                    continue;
                }

                previousChord = detected;
                (NoteStep rootNote, Alteration rootAlteration) =
                    GetChordRootSpelling(detected.RootPitchClass, detected.Type, preferFlats);
                NoteStep? bassNote = null;
                Alteration bassAlteration = Alteration.neutral;
                if (detected.BassPitchClass is int bassPitchClass) {
                    (NoteStep detectedBassNote, Alteration detectedBassAlteration) =
                        GetChordNoteSpelling(bassPitchClass, preferFlats);
                    bassNote = detectedBassNote;
                    bassAlteration = detectedBassAlteration;
                }

                measure.ChordSequence.ChordBeats[beat].Chords.Add(
                    new EZSong.Model.Chord(false, measure.TimeSignature.GetBeatDuration(),
                        rootNote, rootAlteration, detected.Type,
                        bassNote, bassAlteration));
            }
        }

        private static List<MidiChordAnalyzer.MidiNote> SelectHarmonicAnalysisNotes(
            List<(long Start, long End, int Pitch, int Track, int Channel)> allNotes,
            List<int> voicePrograms) {
            List<MidiChordAnalyzer.MidiNote> selected = new();
            foreach (IGrouping<int, (long Start, long End, int Pitch, int Track, int Channel)> trackGroup
                     in allNotes.GroupBy(n => n.Track)) {
                int trackIndex = trackGroup.Key;
                List<(long Start, long End, int Pitch, int Track, int Channel)> trackNotes =
                    trackGroup.OrderBy(n => n.Start).ToList();
                if (trackNotes.Count == 0) {
                    continue;
                }

                int medianPitch = trackNotes.Select(n => n.Pitch).OrderBy(p => p).ElementAt(trackNotes.Count / 2);
                int pitchRange = trackNotes.Max(n => n.Pitch) - trackNotes.Min(n => n.Pitch);

                // Nombre maximal de notes simultanées dans la piste : une mélodie
                // est essentiellement monophonique, contrairement à l'accompagnement.
                int maxSimultaneous = MaxSimultaneousNotes(trackNotes);

                // Instrument déclaré pour cette piste (programme GM), s'il existe.
                int program = (voicePrograms is not null && trackIndex < voicePrograms.Count)
                    ? voicePrograms[trackIndex] : 0;

                // Une piste est considérée comme mélodique si son instrument est un
                // instrument de lead (bois, cuivres solo, synthés lead, voix…) ou
                // si son comportement est celui d'une mélodie : monophonique,
                // registre aigu ou médium-aigu, tessiture large. Les pistes graves
                // restent toujours incluses, même monophoniques (basses harmoniques).
                bool likelyMelody = false;
                if (IsMelodicProgram(program)) {
                    likelyMelody = true;
                } else if (medianPitch >= 55) {
                    if (maxSimultaneous <= 1 && trackNotes.Count >= 5 && pitchRange >= 7) {
                        likelyMelody = true;
                    } else if (maxSimultaneous <= 2 && medianPitch >= 66 && pitchRange >= 12
                               && trackNotes.Count >= 6) {
                        likelyMelody = true;
                    } else if (medianPitch >= 72 && pitchRange >= 5 && trackNotes.Count >= 4) {
                        likelyMelody = true;
                    }
                }

                if (likelyMelody) {
                    continue;
                }

                foreach ((long Start, long End, int Pitch, int Track, int Channel) note in trackNotes) {
                    selected.Add(new MidiChordAnalyzer.MidiNote(note.Start, note.End, note.Pitch, note.Track, 1.0));
                }
            }

            // Repli : si aucune piste n'a été retenue (tout a été classé comme
            // mélodie), on analyse toutes les notes avec un poids plein plutôt que
            // de ne produire aucun accord.
            if (selected.Count == 0) {
                foreach ((long Start, long End, int Pitch, int Track, int Channel) note in allNotes) {
                    selected.Add(new MidiChordAnalyzer.MidiNote(note.Start, note.End, note.Pitch, note.Track, 1.0));
                }
            }

            return selected;
        }

        private static int MaxSimultaneousNotes(
            List<(long Start, long End, int Pitch, int Track, int Channel)> notes) {
            List<(long Time, int Delta)> events = new();
            foreach ((long Start, long End, int Pitch, int Track, int Channel) note in notes) {
                events.Add((note.Start, 1));
                events.Add((note.End, -1));
            }
            // À temps égal, les fins (-1) précèdent les débuts (+1) : deux notes
            // qui se touchent ne sont pas simultanées.
            events.Sort((a, b) => a.Time == b.Time ? a.Delta.CompareTo(b.Delta) : a.Time.CompareTo(b.Time));
            int current = 0;
            int maximum = 0;
            foreach ((long Time, int Delta) ev in events) {
                current += ev.Delta;
                if (current > maximum) {
                    maximum = current;
                }
            }
            return maximum;
        }

        private static bool IsMelodicProgram(int program) {
            // Programmes General MIDI considérés comme des instruments de lead
            // (mélodie) plutôt que comme de l'accompagnement harmonique.
            return program is (>= 52 and <= 54)   // voix / chœurs
                or (>= 56 and <= 60)              // cuivres solo
                or (>= 64 and <= 79)              // anches / flûtes
                or (>= 80 and <= 87)              // synthés lead
                or (>= 104 and <= 119);           // effets / percussifs
        }

        private static List<int> ReadVoicePrograms(List<TrackChunk> tracks) {
            List<int> programs = new();
            foreach (TrackChunk track in tracks) {
                int program = 0;
                foreach (MidiEvent midiEvent in track.Events) {
                    if (midiEvent is ProgramChangeEvent programChange) {
                        program = (int)programChange.ProgramNumber;
                        break;
                    }
                }
                programs.Add(program);
            }
            return programs;
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
                string filename = dlg.Filename;
                if (!filename.ToLowerInvariant().EndsWith(".pdf")) {
                    filename += ".pdf";
                }
                try {
                    LilypondFileBuilder builder = new(_currentSong);
                    builder.GeneratePdfFile(filename);
                } catch (Exception ex) {
                    MessageDialog md = new(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, ex.Message) {
                        UseMarkup = false
                    };
                    md.Run();
                    md.Destroy();
                }
            }
            dlg.Destroy();
        }
    }
}
