using EZSong.Model;
using EZSong.UI.Widgets.Helpers;
using Gtk;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;


namespace EZSong.UI.Widgets {

    /// <summary>
    /// Popover widget that allows users to select rhythm elements (notes, rests, tuplets, etc.) 
    /// based on a given beat pattern and maximum duration.
    /// </summary>
    public class RhythmElementPickerPopover : Popover {

        private BeatPattern? _beatPattern;
        private RhythmRationalDuration _maxDuration;

        public event Action<Object?>? ElementSelected;

        private Notebook _notebook;

        /// <summary>
        /// Constructs a new RhythmElementPickerPopover that is relative to the specified widget.
        /// </summary>
        /// <param name="relativeTo"></param>
        public RhythmElementPickerPopover(Widget relativeTo) : base(relativeTo) {
            BorderWidth = 6;
            WidthRequest = 300;

            _notebook = new Notebook {
                ShowTabs = true
            };

            BuildUI();

            Closed += (s, e) => {
                ElementSelected?.Invoke(null);
            };
        }

        // =========================================================
        // PUBLIC
        // =========================================================

        /// <summary>
        /// Opens the popover with the specified beat pattern and maximum duration, allowing users to select rhythm elements.
        /// </summary>
        /// <param name="beatPattern">The beat pattern to use for determining allowed rhythm elements.</param>
        /// <param name="maxDuration">The maximum duration of rhythm elements that can be selected.</param>
        public void Open(BeatPattern beatPattern, RhythmRationalDuration maxDuration) {
            _maxDuration = maxDuration;
            _beatPattern = beatPattern;
            Rebuild();
            ShowAll();
            Popup();
        }

        // =========================================================
        // UI
        // =========================================================

        /// <summary>
        /// Builds the user interface of the popover, adding the notebook that contains tabs for different rhythm elements.
        /// </summary>
        private void BuildUI() {
            Add(_notebook);
        }

        /// <summary>
        /// Rebuilds the content of the popover based on the current beat pattern and maximum duration, clearing any existing pages and adding new ones for notes, rests, tuplets, and other elements.
        /// </summary>
        private void Rebuild() {
            // Clear
            while (_notebook.NPages > 0) {
                _notebook.RemovePage(0);
            }

            _ = _notebook.AppendPage(BuildNotesPage(), new Label("Notes"));
            _ = _notebook.AppendPage(BuildRestsPage(), new Label("Silences"));
            _ = _notebook.AppendPage(BuildTupletsPage(), new Label("Tuplets"));
            _ = _notebook.AppendPage(BuildOtherElementsPage(), new Label("Autres"));
        }

        /// <summary>
        /// Builds the page for selecting notes, creating a grid of buttons for each allowed note duration based on the current beat pattern and maximum duration.
        /// </summary>
        /// <returns>The widget representing the notes page.</returns>
        private Widget BuildNotesPage() {
            return BuildGrid(isRest: false);
        }

        /// <summary>
        /// Builds the page for selecting rests, creating a grid of buttons for each allowed rest duration based on the current beat pattern and maximum duration.
        /// </summary>
        /// <returns>The widget representing the rests page.</returns>
        private Widget BuildRestsPage() {
            return BuildGrid(isRest: true);
        }

        /// <summary>
        /// Builds the page for selecting tuplets, creating a flow of buttons for each allowed tuplet based on the current beat pattern and maximum duration.
        /// </summary>
        /// <returns>The widget representing the tuplets page.</returns>
        private Widget BuildTupletsPage() {
            FlowBox flow = CreateFlow();

            foreach (Helpers.TupletOption t in GetAllowedTuplets()) {

                UICompositeGlyph compositeGlyph = UICompositeGlyph.FromTupletDescriptor(t.RhythmTuplet);

                if (compositeGlyph is null) {
                    continue; // Skip unsupported durations
                }

                string label = compositeGlyph.ToString();
                Button btn = CreateButton(label);

                btn.Clicked += (s, e) => {
                    ElementSelected?.Invoke(t.RhythmTuplet);
                    Popdown();
                };

                flow.Add(btn);
            }

            return flow;
        }

        /// <summary>
        /// Builds the page for selecting other rhythm elements, 
        /// such as ties, creating a flow of buttons for each allowed element based on the current beat pattern and maximum duration.
        /// </summary>
        /// <returns>The widget representing the other elements page.</returns>
        private Widget BuildOtherElementsPage() {

            FlowBox flow = CreateFlow();

            //Symbole de liaison avec la note/mesure suivante
            if (_beatPattern != null && _beatPattern.CanAddTieFrom()) {
                UICompositeGlyph compositeGlyph = new();
                UIGlyph tieGlyph = new(Enums.Glyph.TieFrom);
                compositeGlyph.AddGlyph(tieGlyph);
                string label = compositeGlyph.ToString();
                Button btn = CreateButton(label);
                btn.Clicked += (s, e) => {
                    //Ajout de la liaison avec la note/mesure suivante
                    ElementSelected?.Invoke(new RhythmTieFrom());
                    Popdown();
                };
                flow.Add(btn);
            }

            //TODO : Ajouter symboles de début/fin de phrase

            return flow;
        }

        /// <summary>
        /// Builds a grid of buttons for selecting rhythm elements (notes or rests) based on the specified duration type (isRest). Each button represents an allowed duration, and clicking a button will trigger the ElementSelected event with the corresponding RhythmSimpleElement.
        /// </summary>
        /// <param name="isRest">Indicates whether the grid is for rests (true) or notes (false).</param>
        /// <returns>The widget representing the grid of rhythm elements.</returns>
        private Widget BuildGrid(bool isRest) {
            FlowBox flow = CreateFlow();

            foreach (RhythmRationalDuration d in GetAllowedDurations()) {
                UICompositeGlyph compositeGlyph = GetCompositeGlyph(new RhythmSimpleElement(d, isRest));

                if (compositeGlyph is null) {
                    continue; // Skip unsupported durations
                }

                string label = compositeGlyph.ToString(); 
                Button btn = CreateButton(label);
                
                btn.Clicked += (s, e) => {
                    RhythmSimpleElement element = new(d, isRest);

                    ElementSelected?.Invoke(element);
                    Popdown();
                };

                flow.Add(btn);
            }

            return flow;
        }

        /// <summary>
        /// Creates a FlowBox widget with specific properties for arranging child widgets in a grid-like layout. 
        /// The FlowBox is configured to have a maximum of 4 children per line, no selection mode, and specified row and column spacing.
        /// </summary>
        /// <returns>The FlowBox widget.</returns>
        private FlowBox CreateFlow() {
            return new FlowBox {
                MaxChildrenPerLine = 4,
                SelectionMode = SelectionMode.None,
                RowSpacing = 4,
                ColumnSpacing = 4
            };
        }

        /// <summary>
        /// Creates a button with the specified label and sets its width, height, and style class. 
        /// The button is intended to represent a rhythm element (note, rest, or tuplet) in the popover.
        /// </summary>
        /// <param name="label">The text label to display on the button.</param>
        /// <returns>The created Button widget.</returns>
        private Button CreateButton(string label) {
            Button btn = new(label) {
                WidthRequest = 60,
                HeightRequest = 40,
            };

            btn.StyleContext.AddClass("glyph");

            return btn;
        }

        // =========================================================
        // DATA
        // =========================================================

        /// <summary>
        /// Gets a list of allowed rhythm durations based on the current maximum duration. The method returns a list of RhythmRationalDuration objects that are less than or equal to the specified maximum duration, including various note and rest durations (e.g., whole notes, half notes, quarter notes, eighth notes, sixteenth notes) with different dot counts.
        /// </summary>
        /// <returns>A list of allowed RhythmRationalDuration objects.</returns>
        private List<RhythmRationalDuration> GetAllowedDurations() {
            List<RhythmRationalDuration> all = new() {
                new RhythmRationalDuration(1, 1, 0), // ronde
                new RhythmRationalDuration(1, 1, 1), // ronde
                new RhythmRationalDuration(1, 1, 2), // ronde
                new RhythmRationalDuration(1, 2, 0), // blanche
                new RhythmRationalDuration(1, 2, 1), // blanche
                new RhythmRationalDuration(1, 2, 2), // blanche
                new RhythmRationalDuration(1, 4, 0), // noire
                new RhythmRationalDuration(1, 4, 1), // noire
                new RhythmRationalDuration(1, 4, 2), // noire
                new RhythmRationalDuration(1, 8, 0), // croche
                new RhythmRationalDuration(1, 8, 1), // croche
                new RhythmRationalDuration(1, 8, 2), // croche
                new RhythmRationalDuration(1, 16, 0), // double croche
                new RhythmRationalDuration(1, 16, 1), // double croche
                new RhythmRationalDuration(1, 16, 2) // double croche
            };

            return all.FindAll(d => _maxDuration.IsGreaterOrEqual(d));
        }


        /// <summary>
        /// Gets a list of allowed tuplet options based on the current maximum duration. 
        /// </summary>
        /// <returns>A list of Helpers.TupletOption objects that represent various tuplet configurations (e.g., triplets, quintuplets) that can fit within the specified maximum duration</returns>
        private List<Helpers.TupletOption> GetAllowedTuplets() {
            List<Helpers.TupletOption> list = new();

            // Correction : utiliser la méthode CompareTo ou une méthode utilitaire pour comparer les durées
            if (_maxDuration.IsGreaterOrEqual(new RhythmRationalDuration(1, 4, 0))) {

                //3 croches dans la durée d'une noire
                List<RhythmSimpleElement> subdivisions = new() {
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false),
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false),
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false)
                };
                RhythmTuplet rhythmTuplet = new(subdivisions, new RhythmRationalDuration(1, 4, 0));
                list.Add(new Helpers.TupletOption(rhythmTuplet));

                //Un soupir + 2 croches dans la durée d'une noire
                subdivisions = new() {
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), true),
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false),
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false)
                };
                rhythmTuplet = new(subdivisions, new RhythmRationalDuration(1, 4, 0));
                list.Add(new Helpers.TupletOption(rhythmTuplet));

                //1 croche + 1 noire + 1 croche dans la durée d'une noire
                subdivisions = new() {
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false),
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 4, 0), false),
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false)
                };
                rhythmTuplet = new(subdivisions, new RhythmRationalDuration(1, 4, 0));
                list.Add(new Helpers.TupletOption(rhythmTuplet));
            }

            if (_maxDuration.IsGreaterOrEqual(new RhythmRationalDuration(1, 8, 0))) {

                //3 croches dans la durée d'une croche
                List<RhythmSimpleElement> subdivisions = new() {
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false),
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false),
                    new RhythmSimpleElement(new RhythmRationalDuration(1, 8, 0), false)
                };
                RhythmTuplet rhythmTuplet = new(subdivisions, new RhythmRationalDuration(1, 8, 0));
                list.Add(new Helpers.TupletOption(rhythmTuplet));
            }

            //TODO : ajouter d'autres tuplets (ex: quintuplet, septuplet, etc.) en fonction de la durée maximale autorisée

            return list;
        }


        // =========================================================
        // SYMBOLS
        // =========================================================

        /// <summary>
        /// Gets a composite glyph representation of a RhythmSimpleElement, including its base glyph and any additional dot glyphs based on the element's dot count. This method is used to create a visual representation of the rhythm element for display in the popover.
        /// </summary>
        /// <param name="element">The RhythmSimpleElement to create a composite glyph for.</param>
        /// <returns>A UICompositeGlyph representing the visual appearance of the RhythmSimpleElement, including its base glyph and any dot glyphs.</returns>
        private Helpers.UICompositeGlyph GetCompositeGlyph(RhythmSimpleElement element) {

            UIGlyph glyph = UIGlyph.FromDescriptor(element);
            UICompositeGlyph compositeGlyph = new();
            compositeGlyph.AddGlyph(glyph);
            for (int i = 0; i < element.DotCount(); i++) {
                compositeGlyph.AddGlyph(UIGlyph.DotGlyph());
            }
            return compositeGlyph;
        }
    }
}
