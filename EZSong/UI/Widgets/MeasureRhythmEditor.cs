using Cairo;
using EZSong.Model;
using EZSong.UI.Widgets.WidgetsData;
using Gtk;

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

        private static readonly String _musicalFontFamily = "Bravura";

        private string _dotGlyph = string.Empty;

        public MeasureRhythmEditor() {

            if (_musicalFontFamily != "Bravura") {
                throw new NotSupportedException("Only Bravura font is supported for music glyphs");
            }
            _dotGlyph = "\uE1E7";

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
            
            //todo : éviter le Select (lent)
            string text = string.Join(" ",
                beat.Elements.Select(e => {
                    string glyph = e.IsRest
                        ? GetRestGlyph(e)
                        : GetMusicGlyph(e);

                    if (e.Duration.Dots > 0) {
                        glyph += new string('\uE1E7', e.Duration.Dots);
                    }

                    if (e.TieToNext) {
                        glyph += "\uE1FD"; // tie
                    }

                    return glyph;
                }));

            cr.SetSourceRGB(0, 0, 0);

            //TODO : Faire une fois au départ du draw
            cr.SelectFontFace(_musicalFontFamily, FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(20);

            TextExtents ext = cr.TextExtents(text);

            double tx = x + (width - ext.Width) / 2;
            double ty = height / 2;

            cr.MoveTo(tx, ty);
            cr.ShowText(text);
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

            BeatPattern newBeat = new();

            if (Pattern.Beats[index].Elements.Count == 1) {
                newBeat.Elements.Add(new RhythmElement(new RhythmRationalDuration(1, 8, 0), false, new RhythmTuplet(1, 1)));
                newBeat.Elements.Add(new RhythmElement(new RhythmRationalDuration(1, 8, 0), false, new RhythmTuplet(1, 1)));
            } else {
                newBeat.Elements.Add(new RhythmElement(new RhythmRationalDuration(1, 4, 0), false, new RhythmTuplet(1, 1)));
            }

            // 👉 remplacer réellement dans le pattern
            Pattern.SetBeat(index, newBeat);

            PatternChanged?.Invoke(Pattern);
        }

        public event Action<MeasureRhythmPattern>? PatternChanged;
  

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

        private string GetMusicGlyph(RhythmElement e) {

            if (_musicalFontFamily != "Bravura") {
                throw new NotSupportedException("Only Bravura font is supported for music glyphs");
            }

            return e.Duration.Denominator switch {
                1 => "\uE1D2", // ronde
                2 => "\uE1D3", // blanche
                4 => "\uE1D5", // noire
                8 => "\uE1D7", // croche
                16 => "\uE1D9", // double croche
                _ => "?"
            };
        }

        private string GetRestGlyph(RhythmElement e) {

            if (_musicalFontFamily != "Bravura") {
                throw new NotSupportedException("Only Bravura font is supported for music glyphs");
            }

            return e.Duration.Denominator switch {
                1 => "\uE4E3",
                2 => "\uE4E4",
                4 => "\uE4E5",
                8 => "\uE4E6",
                16 => "\uE4E7",
                _ => "?"
            };
        }

        // PUBLIC API: load external model into widget
        public void LoadFromModel(MeasureRhythmPattern rhythmPattern) {
            
            Pattern = rhythmPattern;

            QueueDraw();

            //PatternChanged?.Invoke(Pattern);

            SetPattern(Pattern);
        }
    }
}
