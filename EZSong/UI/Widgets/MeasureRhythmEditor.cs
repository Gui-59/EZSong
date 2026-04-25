using Cairo;
using EZSong.Model;
using EZSong.UI.Widgets.Helpers;
using EZSong.UI.Widgets.WidgetsData;
using Gdk;
using Gtk;
using System;
using System.Runtime.CompilerServices;

namespace EZSong.UI.Widgets {
    public class MeasureRhythmEditor : DrawingArea {
        public MeasureRhythmPattern? Pattern {
            get; private set;
        }

        bool _durationOk = false;
        bool _noteOk = false;

        private string _musicalFontFamily = new Settings.UserSettings().MusicalFontFamily;

        private int _currentMelodyChordsCount = 0;
        public int CurrentMelodyChordsCount {
            get {
                return _currentMelodyChordsCount;
            }
            set {
                _currentMelodyChordsCount = value;

                if (Pattern == null) {
                    return;
                }
                _noteOk = Pattern.IsCompatibleWithNoteCount(CurrentMelodyChordsCount, _currentGraceNoteCount);
                QueueDraw();
            }
        }

        int _currentGraceNoteCount = 0; //TODO

        int _statusAreaHeight = 15;

        public MeasureRhythmEditor() {
            HeightRequest = 50; //Taille fixe pour éviter les problèmes de redimensionnement
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
        }

        protected override bool OnDrawn(Cairo.Context cr) {
            if (Pattern == null) {
                return true;
            }

            DrawBackground(cr);
            DrawStatus(cr);
            DrawBeats(cr);
            

            return true;
        }

        private void DrawBackground(Cairo.Context cr) {
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
            }
        }

        private Helpers.UICompositeGlyph BuildBeatElementCompositeGlyph(RhythmElement e) {

            Helpers.UICompositeGlyph beatElementCompositeGlyph = new();

            if (e.IsRest) {
                beatElementCompositeGlyph.AddGlyph(UIGlyph.FromDescriptor(e.Duration, true));
            } else {
                beatElementCompositeGlyph.AddGlyph(UIGlyph.FromDescriptor(e.Duration, false));
            }

            if (e.Duration.Dots > 0) {

                for (int i = 0; i < e.Duration.Dots; i++) {
                    beatElementCompositeGlyph.AddGlyph(UIGlyph.DotGlyph());
                }
            }

            if (e.TieToNext) {
                beatElementCompositeGlyph.AddGlyph(UIGlyph.TiefromGlyph());
            }

            return beatElementCompositeGlyph;
        }

        private Helpers.UICompositeGlyph BuildBeatCompositeGlyph(BeatPattern beat) {
            Helpers.UICompositeGlyph beatCompositeGlyph = new();

            foreach (RhythmElement e in beat.Elements) {
                beatCompositeGlyph.AddCompositeGlyph(BuildBeatElementCompositeGlyph(e));
            }

            return beatCompositeGlyph;
        }

        private void DrawBeat(Context cr, BeatPattern beat, double x, double width, double height) {

            Helpers.UICompositeGlyph compositeGlyph = BuildBeatCompositeGlyph(beat);

            cr.SetSourceRGB(0, 0, 0);

            //TODO : Faire une fois au départ du draw
            cr.SelectFontFace(_musicalFontFamily, FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(20);

            TextExtents ext = cr.TextExtents(compositeGlyph.ToString());

            double tx = x + (width - ext.Width) / 2;
            double ty = height / 2;
            ty += _statusAreaHeight;

            cr.MoveTo(tx, ty);
            cr.ShowText(compositeGlyph.ToString());
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

                if (ev.Button == 3) {
                    EditBeat(index);
                }

                if (ev.Button == 1 && ev.State == ModifierType.ShiftMask) { //TODO : permettre aussi le double click
                    ClearBeat(index);
                }                    
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

            

            CreateElementFromUserChoice(index);

            
        }

        void CreateElementFromUserChoice(int index) {

            if (Pattern == null) {
                return;
            }

            BeatPattern beat = Pattern.Beats[index];

            RhythmElementPickerPopover popover = new(this);

            double width = Allocation.Width;
            int beatCount = Pattern.Beats.Count;
            double beatWidth = width / beatCount;

            int beatX = (int)(index * beatWidth);

            popover.PointingTo = new Gdk.Rectangle(
                (int)beatX,
                0,
                (int)beatWidth,
                _statusAreaHeight
            );

            RhythmRationalDuration remaining = beat.GetRemainingDuration(Pattern.TimeSignature.GetBeatDuration());

            popover.ElementSelected += element =>
            {

                BeatPattern beat = Pattern.Beats[index];


                if (element == null) {
                    return;
                } else {
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
            };

            popover.Open(remaining);
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

            double width = Allocation.Width;
            int beatCount = Pattern.TimeSignature.GetBeatCount();

            double beatWidth = width / beatCount;

            for (int i = 0; i < beatCount; i++) {
                double x = i * beatWidth;

                // séparation visuelle
                cr.SetSourceRGB(1, 1, 1);
                cr.MoveTo(x, 0);
                cr.LineTo(x, _statusAreaHeight);
                cr.Stroke();
            }
        }





        // PUBLIC API: load external model into widget
        public void LoadFromModel(MeasureGlobalMelody measureGlobalMelody) {
            
            Pattern = measureGlobalMelody.Pattern;
                       
            _currentMelodyChordsCount = measureGlobalMelody.Melody.MelodyChords.Count;

            _durationOk = Pattern.IsDurationValid() && Pattern.AreBeatsValid();
            _noteOk = Pattern.IsCompatibleWithNoteCount(CurrentMelodyChordsCount, _currentGraceNoteCount);

            QueueDraw();

            //PatternChanged?.Invoke(Pattern);
        }
    }
}
