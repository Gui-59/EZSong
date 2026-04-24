using EZSong.Model;
using Gtk;
using System;
using System.Collections.Generic;


namespace EZSong.UI.Widgets {
    public class RhythmElementPickerPopover : Popover {

        private RhythmRationalDuration _maxDuration;

        public event Action<RhythmElement?>? ElementSelected;

        private Notebook _notebook;

        public RhythmElementPickerPopover(Widget relativeTo) : base(relativeTo) {
            BorderWidth = 6;

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
                Button btn = CreateButton(t.Label);

                btn.Clicked += (s, e) =>
                {
                    ElementSelected?.Invoke(t.Create());
                    Popdown();
                };

                flow.Add(btn);
            }

            return flow;
        }

        private Widget BuildGrid(List<RhythmRationalDuration> durations, bool isRest) {
            FlowBox flow = CreateFlow();

            foreach (RhythmRationalDuration d in durations) {
                string label = GetSymbol(d, isRest);

                Button btn = CreateButton(label);

                btn.Clicked += (s, e) =>
                {
                    RhythmElement element = new() {
                        Duration = d,
                        IsRest = isRest
                    };

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
            return new Button(label) {
                WidthRequest = 60,
                HeightRequest = 40
            };
        }

        // =========================================================
        // DATA
        // =========================================================

        private List<RhythmRationalDuration> GetAllowedDurations() {
            List<RhythmRationalDuration> all = new() {
                new RhythmRationalDuration(1, 4, 0),
                new RhythmRationalDuration(1, 8, 0),
                new RhythmRationalDuration(1, 16, 0),
                new RhythmRationalDuration(1, 32, 0)
            };

            return all.FindAll(d => _maxDuration.IsGreaterOrEqual(d));
        }

        private List<Helpers.TupletOption> GetAllowedTuplets() {
            List<Helpers.TupletOption> list = new();

            // Correction : utiliser la méthode CompareTo ou une méthode utilitaire pour comparer les durées
            if (_maxDuration.IsGreaterOrEqual(new RhythmRationalDuration(1, 1, 0))) {
                list.Add(new Helpers.TupletOption("Triolet ♩", 3, 1));
            }

            if (_maxDuration.IsGreaterOrEqual(new RhythmRationalDuration(1, 1, 0))) {
                list.Add(new Helpers.TupletOption("Triolet ♪", 3, 1));
            }

            return list;
        }


        // =========================================================
        // SYMBOLS
        // =========================================================

        private string GetSymbol(RhythmRationalDuration duration, bool isRest) {

            if (duration.Equals(new RhythmRationalDuration(1, 4, 0))) {
                return isRest ? "𝄽" : "♩";
            }

            if (duration.Equals(new RhythmRationalDuration(1, 8, 0))) {
                return isRest ? "𝄾" : "♪";
            }

            if (duration.Equals(new RhythmRationalDuration(1, 16, 0))) {
                return isRest ? "𝄿" : "♬";
            }

            if (duration.Equals(new RhythmRationalDuration(1, 32, 0))) {
                return isRest ? "rest" : "♬♬";
            }

            return "?";
        }
    }
}
