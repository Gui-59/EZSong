using Cairo;
using EZSong.Model;
using EZSong.UI.Widgets.WidgetsData;
using Gdk;
using Gtk;

namespace EZSong.UI.Widgets {
    public class MeasureRhythmEditor : DrawingArea {
        public MeasureRhythmPattern? Pattern {
            get; private set;
        }

        private static readonly String _musicalFontFamily = "Bravura";

        bool _durationOk = false;
        bool _noteOk = false;
        
        public int CurrentMelodyChordsCount {
            get; set;
        }

        int _currentGraceNoteCount = 0; //TODO

        private string _dotGlyph = string.Empty;

        int _statusAreaHeight = 15;

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
            ty += _statusAreaHeight;

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

            if (IsInStatusZone(ev.Y)) {
                ClearBeat(index);
            } else {
                EditBeat(index);
            }

            return true;
        }

        private bool IsInStatusZone(double y) {
            if (y <= _statusAreaHeight) {
                return true;
            }
            return false;
        }

        private void ClearBeat(int index) {
            if (Pattern == null) {
                return;
            }

            BeatPattern beat = Pattern.Beats[index];
            beat.Elements.Clear();

            _durationOk = Pattern.IsDurationValid() && Pattern.AreBeatsValid();

            _noteOk = Pattern.IsCompatibleWithNoteCount(CurrentMelodyChordsCount, _currentGraceNoteCount);
            PatternChanged?.Invoke(Pattern);
        }

        private void EditBeat(int index) {
            if (Pattern == null) {
                return;
            }

            BeatPattern beat = Pattern.Beats[index];

            RhythmRationalDuration remaining = beat.GetRemainingDuration(Pattern.TimeSignature.GetBeatDuration());

            RhythmElement element = CreateElementFromUserChoice(remaining);

            if (element == null) {
                return;
            }
            if (element.Duration.Numerator == 0) {
                return;
            }

            if (!beat.CanAdd(element, Pattern.TimeSignature.GetBeatDuration())) {
                return;
            }

            beat.Elements.Add(element);

            _durationOk = Pattern.IsDurationValid() && Pattern.AreBeatsValid();

            _noteOk = Pattern.IsCompatibleWithNoteCount(CurrentMelodyChordsCount, _currentGraceNoteCount);
            PatternChanged?.Invoke(Pattern);
        }

        RhythmElement CreateElementFromUserChoice(RhythmRationalDuration remaining) {
            // TEMPORAIRE : comportement simple pour tester

            //if (remaining.Equals(new RhythmRationalDuration(1, 4, 0)) || IsGreaterThan(remaining, new RhythmRationalDuration(1, 4, 0))) {
                //return new RhythmElement(new RhythmRationalDuration(1, 4, 0), false, new RhythmTuplet(1,1));
            //}

            if (remaining.Equals(new RhythmRationalDuration(1, 8, 0)) || IsGreaterThan(remaining, new RhythmRationalDuration(1, 8, 0))) {
                return new RhythmElement(new RhythmRationalDuration(1, 8, 0), false, new RhythmTuplet(1,1));
            }

            if (remaining.Equals(new RhythmRationalDuration(1, 16, 0)) || IsGreaterThan(remaining, new RhythmRationalDuration(1, 16, 0))) {
                return new RhythmElement(new RhythmRationalDuration(1, 16, 0), false, new RhythmTuplet(1,1));
            }

            return new RhythmElement(remaining, false, new RhythmTuplet(1, 1));
        }

        private static bool IsGreaterThan(RhythmRationalDuration a, RhythmRationalDuration b) {
            // Compare les durées en les ramenant à un dénominateur commun
            int left = a.Numerator * b.Denominator;
            int right = b.Numerator * a.Denominator;
            if (left > right) {
                return true;
            }
            if (left == right) {
                // Si les valeurs sont égales, comparer les points (dots)
                return a.Dots > b.Dots;
            }
            return false;
        }

        public event Action<MeasureRhythmPattern>? PatternChanged;
  

        private void DrawStatus(Context cr) {
            if (Pattern == null) {
                return;
            }

            if (!_durationOk) {
                cr.SetSourceRGB(0.8, 0.2, 0.2); // rouge
            } else if (!_noteOk) {
                cr.SetSourceRGB(0.8, 0.6, 1); 
            } else {
                cr.SetSourceRGB(0.2, 0.8, 0.2); // vert
            }

            cr.Rectangle(0, 0, Allocation.Width, _statusAreaHeight);
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
