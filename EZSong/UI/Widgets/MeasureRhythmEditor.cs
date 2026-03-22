using Gtk;
using Cairo;

namespace EZSong.UI.Widgets {
    public class MeasureRhythmEditor : DrawingArea {
        public MeasureRhythmPattern? Pattern {
            get; private set;
        }

        public int NoteCount {
            get; set;
        }
        public int GraceNoteCount {
            get; set;
        }

        public event System.Action? PatternChanged;

        public MeasureRhythmEditor() {
            HeightRequest = 50; //Taille fixe pour éviter les problèmes de redimensionnement
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
        }

        public void SetPattern(MeasureRhythmPattern pattern) {
            Pattern = pattern;
            QueueDraw();
        }

        protected override bool OnDrawn(Context cr) {
            if (Pattern == null) {
                return true;
            }

            DrawBackground(cr);
            DrawBeats(cr);
            DrawStatus(cr);

            return true;
        }

        private void DrawBackground(Context cr) {
            cr.SetSourceRGB(1, 1, 1);
            cr.Paint();
        }

        private void DrawBeats(Context cr) {
            if (Pattern == null) {
                return;
            }

            double width = Allocation.Width;
            double height = Allocation.Height;

            int beatCount = Pattern.TimeSignature.GetBeatCount();

            if (beatCount == 0) {
                return;
            }

            double beatWidth = width / beatCount;

            for (int i = 0; i < beatCount; i++) {
                double x = i * beatWidth;

                DrawBeat(cr, Pattern.Beats[i], x, beatWidth, height);

                // séparation visuelle
                cr.SetSourceRGB(0.3, 0.3, 0.3);
                cr.MoveTo(x, 0);
                cr.LineTo(x, height);
                cr.Stroke();
            }
        }

        private void DrawBeat(Context cr, BeatPattern beat, double x, double width, double height) {
            string text = string.Join(" ",
                beat.Elements.Select(e => {
                    string s = DurationToString(e.Duration);

                    if (e.IsRest) {
                        s = "r" + s;
                    }

                    if (e.TieToNext) {
                        s += "~";
                    }

                    return s;
                }));

            cr.SetSourceRGB(0, 0, 0);

            cr.SelectFontFace("Consolas", FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(14);

            TextExtents ext = cr.TextExtents(text);

            double tx = x + (width - ext.Width) / 2;
            double ty = height / 2;

            cr.MoveTo(tx, ty);
            cr.ShowText(text);
        }

        private string DurationToString(RhythmRationalDuration d) {
            string s = d.Denominator switch {
                1 => "w",
                2 => "h",
                4 => "q",
                8 => "e",
                16 => "s",
                _ => "?"
            };

            if (d.Dots > 0) {
                s += new string('.', d.Dots);
            }

            return s;
        }

        protected override bool OnButtonPressEvent(Gdk.EventButton ev) {
            if (Pattern == null) {
                return false;
            }

            double width = Allocation.Width;
            int beatCount = Pattern.Beats.Count;

            double beatWidth = width / beatCount;

            int index = (int)(ev.X / beatWidth);

            if (index < 0 || index >= beatCount) {
                return false;
            }

            EditBeat(index);

            return true;
        }

        private void EditBeat(int index) {
            if (Pattern == null) {
                return;
            }

            BeatPattern beat = new();

            // Exemple simple : toggle entre ♩ et ♪♪
            if (Pattern.Beats[index].Elements.Count == 1) {
                beat.Elements.Add(new RhythmElement(new RhythmRationalDuration(1, 8)));
                beat.Elements.Add(new RhythmElement(new RhythmRationalDuration(1, 8)));
            } else {
                beat.Elements.Add(new RhythmElement(new RhythmRationalDuration(1, 4)));
            }

            Pattern.SetBeat(index, beat);

            PatternChanged?.Invoke();

            QueueDraw();
        }

        private void DrawStatus(Context cr) {
            if (Pattern == null) {
                return;
            }

            bool durationOk = Pattern.IsDurationValid() && Pattern.AreBeatsValid();
            bool noteOk = Pattern.IsCompatibleWithNoteCount(NoteCount, GraceNoteCount);

            if (!durationOk) {
                cr.SetSourceRGB(0.8, 0.2, 0.2); // rouge
            } else if (!noteOk) {
                cr.SetSourceRGB(0.8, 0.6, 0.2); // orange
            } else {
                cr.SetSourceRGB(0.2, 0.8, 0.2); // vert
            }

            cr.Rectangle(0, 0, Allocation.Width, 5);
            cr.Fill();
        }
    }
}
