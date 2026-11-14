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
        /// sont réparties dans les mesures selon leur horodatage MIDI.
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
                MidiFile midi = MidiFile.Read(fileName);
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
            List<(long Start, long End, int Pitch, int Track)> allMidiNotes = new();
            List<(long Time, string Text)> lyrics = new();
            for (int trackIndex = 0; trackIndex < tracks.Count; trackIndex++) {
                TrackChunk track = tracks[trackIndex];
                int staffIndex = selectedTracks.IndexOf(trackIndex);
                foreach (Note note in track.GetNotes()) {
                    long start = Math.Max(0, note.Time);
                    long end = Math.Max(start + 1, note.Time + note.Length);
                    int pitch = (int)(byte)note.NoteNumber;
                    allMidiNotes.Add((start, end, pitch, trackIndex));
                    if (staffIndex >= 0) {
                        staffNotes[staffIndex].Add(new MidiQuantizer.Note(start, end, pitch));
                    }
                }
                // Les événements texte/lyrics sont horodatés en cumulant les DeltaTime de la piste.
                long absoluteTime = 0;
                foreach (MidiEvent ev in track.Events) {
                    absoluteTime += ev.DeltaTime;
                    if (ev is LyricEvent lyric && !string.IsNullOrWhiteSpace(lyric.Text)) {
                        lyrics.Add((absoluteTime, lyric.Text.Trim()));
                    }
                }
            }
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

            SegmentData segment = new(0, song.SongSettings, new List<MeasureData>());
            for (int measureIndex = 0; measureIndex < measureSpans.Count; measureIndex++) {
                MidiQuantizer.MeasureSpan span = measureSpans[measureIndex];
                List<MeasureGlobalMelody> staffs = new();
                for (int staffIndex = 0; staffIndex < selectedTracks.Count; staffIndex++) {
                    MeasureGlobalMelody global = new(staffIndex);
                    global.Melody = new MeasureMelody(staffIndex, staffQuantizations[staffIndex].BuildMelody(span));
                    global.Pattern = staffQuantizations[staffIndex].BuildPattern(staffIndex, span);
                    staffs.Add(global);
                }
                string measureLyrics = string.Join(" ", lyrics.Where(lyric => span.Contains(lyric.Time))
                    .OrderBy(lyric => lyric.Time).Select(lyric => lyric.Text));
                ChordSequence chordSequence = new();
                chordSequence.InitializeFromTimeSignature(span.TimeSignature);
                MeasureData measure = new(measureIndex, song.SongSettings, span.TimeSignature, new KeySignature(),
                    chordSequence, staffs, measureLyrics);
                // Calcul harmonique effectué à l'import uniquement : un accord par temps à partir
                // des classes de hauteur présentes dans la fenêtre du temps.
                CalculateMeasureChords(measure, allMidiNotes, span.StartTicks, span.DurationTicks, ticksPerQuarter);
                segment.Measures.Add(measure);
            }
            for (int i = 0; i < segment.Measures.Count; i++) {
                if (i > 0) {
                    segment.Measures[i].PrecedingMeasure = segment.Measures[i - 1];
                }

                if (i + 1 < segment.Measures.Count) {
                    segment.Measures[i].FollowingMeasure = segment.Measures[i + 1];
                }
            }
            song.Segments.Clear(); song.Segments.Add(segment);
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
        /// sélectionnées pour la transcription. Les pistes aiguës essentiellement
        /// monophoniques sont considérées comme probablement mélodiques ; les basses
        /// monophoniques sont conservées car elles apportent une information harmonique.
        /// </summary>
        private static void CalculateMeasureChords(MeasureData measure,
            List<(long Start, long End, int Pitch, int Track)> allNotes, double measureStart,
            double measureTicks, long ticksPerQuarter) {
            if (allNotes.Count == 0) {
                return;
            }

            List<(long Start, long End, int Pitch, int Track)> harmonicNotes =
                SelectHarmonicAnalysisNotes(allNotes);
            if (harmonicNotes.Count == 0) {
                harmonicNotes = allNotes;
            }

            RhythmRationalDuration beatDuration = measure.TimeSignature.GetBeatDuration().ApplyDots();
            double beatTicks = ticksPerQuarter * 4.0 * beatDuration.Numerator / beatDuration.Denominator;
            int beatCount = measure.TimeSignature.GetBeatCount();
            double measureEnd = measureStart + measureTicks;
            List<(long Start, long End, int Pitch, int Track)> notesInMeasure = harmonicNotes
                .Where(n => n.Start < measureEnd && n.End > measureStart).ToList();
            if (notesInMeasure.Count == 0) {
                return;
            }

            for (int beat = 0; beat < beatCount; beat++) {
                double beatStart = measureStart + beat * beatTicks;
                double beatEnd = beatStart + beatTicks;
                // L'accord est estimé à partir du temps courant et du contexte de la mesure.
                // Le temps courant pèse davantage ; le contexte aide pour les arpèges.
                Dictionary<int, double> weights = new();
                foreach ((long Start, long End, int Pitch, int Track) note in notesInMeasure) {
                    double overlapBeat = Math.Max(0, Math.Min(note.End, beatEnd) - Math.Max(note.Start, beatStart));
                    double overlapMeasure = Math.Max(0, Math.Min(note.End, measureEnd) - Math.Max(note.Start, measureStart));
                    if (overlapMeasure <= 0) {
                        continue;
                    }

                    int pc = ((note.Pitch % 12) + 12) % 12;
                    double weight = overlapBeat * 2.0 + overlapMeasure * 0.20;
                    weights[pc] = weights.GetValueOrDefault(pc) + weight;
                }

                if (weights.Count == 0) {
                    continue;
                }

                double maxWeight = weights.Values.Max();
                if (maxWeight <= 0) {
                    continue;
                }

                (int Root, ChordType Type, double Score) best = (-1, ChordType.NoneOrMajor, double.NegativeInfinity);
                for (int root = 0; root < 12; root++) {
                    EvaluateChord(root, ChordType.NoneOrMajor, new[] { root, (root + 4) % 12, (root + 7) % 12 });
                    EvaluateChord(root, ChordType.Minor, new[] { root, (root + 3) % 12, (root + 7) % 12 });
                }

                // Ne pas inventer un accord si moins de deux notes de l'accord sont
                // réellement présentes dans la matière musicale de la mesure.
                if (best.Root < 0) {
                    continue;
                }

                Pitch rootPitch = Pitch.FromMidi(60 + best.Root);
                measure.ChordSequence.ChordBeats[beat].Chords.Add(
                    new EZSong.Model.Chord(false, measure.TimeSignature.GetBeatDuration(),
                        rootPitch.Note, rootPitch.Alteration, best.Type));

                void EvaluateChord(int root, ChordType type, int[] tones) {
                    int matches = tones.Count(pc => weights.GetValueOrDefault(pc) > 0);
                    if (matches < 2) {
                        return;
                    }

                    double chordWeight = tones.Sum(pc => weights.GetValueOrDefault(pc));
                    double totalWeight = weights.Values.Sum();
                    double coverage = chordWeight / Math.Max(1.0, totalWeight);
                    double rootBonus = weights.GetValueOrDefault(root) / Math.Max(1.0, maxWeight);
                    // Favorise les accords qui expliquent plusieurs notes, avec un léger
                    // avantage à la fondamentale sans imposer sa présence.
                    double score = coverage * 100.0 + matches * 8.0 + rootBonus * 5.0;
                    if (score > best.Score) {
                        best = (root, type, score);
                    }
                }
            }
        }

        private static List<(long Start, long End, int Pitch, int Track)> SelectHarmonicAnalysisNotes(
            List<(long Start, long End, int Pitch, int Track)> allNotes) {
            List<(long Start, long End, int Pitch, int Track)> selected = new();
            foreach (IGrouping<int, (long Start, long End, int Pitch, int Track)> trackGroup in allNotes.GroupBy(n => n.Track)) {
                List<(long Start, long End, int Pitch, int Track)> trackNotes = trackGroup.OrderBy(n => n.Start).ToList();
                if (trackNotes.Count == 0) {
                    continue;
                }

                int medianPitch = trackNotes.Select(n => n.Pitch).OrderBy(p => p).ElementAt(trackNotes.Count / 2);
                int pitchRange = trackNotes.Max(n => n.Pitch) - trackNotes.Min(n => n.Pitch);
                int overlappingNotes = 0;
                for (int i = 0; i < trackNotes.Count; i++) {
                    bool overlapsAnother = false;
                    for (int j = 0; j < trackNotes.Count; j++) {
                        if (i != j && trackNotes[j].Start < trackNotes[i].End && trackNotes[j].End > trackNotes[i].Start) {
                            overlapsAnother = true;
                            break;
                        }
                    }
                    if (overlapsAnother) {
                        overlappingNotes++;
                    }
                }
                double polyphonyRatio = (double)overlappingNotes / trackNotes.Count;

                // Exclusion prudente : seulement les pistes aiguës, quasi monophoniques,
                // avec une tessiture étendue sont classées comme mélodies probables.
                // Les pistes graves restent incluses même si elles sont monophoniques.
                bool likelyMelody = medianPitch >= 60 && pitchRange >= 12 && polyphonyRatio < 0.08 && trackNotes.Count >= 5;
                if (!likelyMelody) {
                    selected.AddRange(trackNotes);
                }
            }
            return selected;
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
