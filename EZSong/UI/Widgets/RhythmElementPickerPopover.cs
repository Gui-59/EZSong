using EZSong.Model;
using EZSong.UI.Widgets.Helpers;
using Gtk;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;


namespace EZSong.UI.Widgets {
    public class RhythmElementPickerPopover : Popover {

        private RhythmRationalDuration _maxDuration;

        public event Action<IRhythmElement?>? ElementSelected;

        private Notebook _notebook;

        public RhythmElementPickerPopover(Widget relativeTo) : base(relativeTo) {
            BorderWidth = 6;
            WidthRequest = 300;

            _notebook = new Notebook {
                ShowTabs = true
            };

           


            BuildUI();

            Closed += (s, e) =>
            {
                ElementSelected?.Invoke(null);
            };
        }

        // =========================================================
        // PUBLIC
        // =========================================================

        public void Open(RhythmRationalDuration maxDuration) {
            _maxDuration = maxDuration;

            Rebuild();

            ShowAll();
            Popup();
        }

        // =========================================================
        // UI
        // =========================================================

        private void BuildUI() {


            Add(_notebook);
        }

        private void Rebuild() {
            // Clear
            while (_notebook.NPages > 0) {
                _notebook.RemovePage(0);
            }

            _ = _notebook.AppendPage(BuildNotesPage(), new Label("Notes"));
            _ = _notebook.AppendPage(BuildRestsPage(), new Label("Silences"));
            _ = _notebook.AppendPage(BuildTupletsPage(), new Label("Tuplets"));
        }

        private Widget BuildNotesPage() {
            return BuildGrid(GetAllowedDurations(), isRest: false);
        }

        private Widget BuildRestsPage() {
            return BuildGrid(GetAllowedDurations(), isRest: true);
        }

        private Widget BuildTupletsPage() {
            FlowBox flow = CreateFlow();

            foreach (Helpers.TupletOption t in GetAllowedTuplets()) {

                UICompositeGlyph compositeGlyph = UICompositeGlyph.FromTupletDescriptor(t.RhythmTuplet);

                if (compositeGlyph is null) {
                    continue; // Skip unsupported durations
                }


                string label = compositeGlyph.ToString();
                Button btn = CreateButton(label);

                btn.Clicked += (s, e) =>
                {
                    ElementSelected?.Invoke(t.RhythmTuplet);
                    Popdown();
                };

                flow.Add(btn);
            }

            return flow;
        }

        private Widget BuildGrid(List<RhythmRationalDuration> maxAllowedDuration, bool isRest) {
            FlowBox flow = CreateFlow();

            foreach (RhythmRationalDuration d in maxAllowedDuration) {
                UICompositeGlyph compositeGlyph = GetCompositeGlyph(new RhythmSimpleElement(d, isRest));

                if (compositeGlyph is null) {
                    continue; // Skip unsupported durations
                }

                string label = compositeGlyph.ToString(); 
                Button btn = CreateButton(label);
                
                btn.Clicked += (s, e) =>
                {
                    RhythmSimpleElement element = new(d, isRest);

                    ElementSelected?.Invoke(element);
                    Popdown();
                };

                flow.Add(btn);
            }

            return flow;
        }

        private FlowBox CreateFlow() {
            return new FlowBox {
                MaxChildrenPerLine = 4,
                SelectionMode = SelectionMode.None,
                RowSpacing = 4,
                ColumnSpacing = 4
            };
        }

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

        private List<RhythmRationalDuration> GetAllowedDurations() {
            List<RhythmRationalDuration> all = new() {
                new RhythmRationalDuration(1, 1, 0), // ronde
                new RhythmRationalDuration(1, 2, 0), // blanche
                new RhythmRationalDuration(1, 4, 0), // noire
                new RhythmRationalDuration(1, 8, 0), // croche
                new RhythmRationalDuration(1, 16, 0) // double croche
            };

            return all.FindAll(d => _maxDuration.IsGreaterOrEqual(d));
        }

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

        private Helpers.UICompositeGlyph GetCompositeGlyph(RhythmSimpleElement element) {

            UIGlyph glyph = UIGlyph.FromDescriptor(element);
            UICompositeGlyph compositeGlyph = new();
            compositeGlyph.AddGlyph(glyph);
            return compositeGlyph;
        }
    }
}
