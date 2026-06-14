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

        private MeasureData? _measureData;

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
                _noteOk = false;
                if (Pattern == null) {
                    return;
                }
                if (_measureData == null) {
                    return;
                }
                _noteOk = Pattern.IsCompatibleWithNoteCount(_measureData.GlobalMelody.Melody, _measureData.PrecedingMeasure);
                QueueDraw();
            }
        }

        int _statusAreaHeight = 15;

        public MeasureRhythmEditor() {
            HeightRequest = 50; //Taille fixe pour éviter les problèmes de redimensionnement
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _measureData = new();
        }

        public MeasureRhythmEditor(MeasureData measureData) {
            HeightRequest = 50; //Taille fixe pour éviter les problèmes de redimensionnement
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _measureData = measureData;
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

        private Helpers.UICompositeGlyph BuildBeatElementCompositeGlyph(IRhythmElement e) {

            Helpers.UICompositeGlyph beatElementCompositeGlyph = new();


            if (e.GetType() == typeof(RhythmTieFrom)) {
                //Cas des liaisons
                //RhythmTieFrom rhythmTieFrom = (RhythmTieFrom)e;
                beatElementCompositeGlyph.AddGlyph(UIGlyph.TiefromGlyph());
            } else if (e.GetType() == typeof(RhythmTuplet)) {

                //Cas du tuplet

                RhythmTuplet te = (RhythmTuplet)e;
                UICompositeGlyph uICompositeGlyph = UICompositeGlyph.FromTupletDescriptor(te);
                beatElementCompositeGlyph.AddCompositeGlyph(uICompositeGlyph);
            } else if (e.GetType() == typeof(RhythmSimpleElement)) {

                //Cas d'un element rythmique simples

                RhythmSimpleElement rhythmSimpleElement = (RhythmSimpleElement)e;

                beatElementCompositeGlyph.AddGlyph(UIGlyph.FromDescriptor((RhythmSimpleElement)e));


                if (rhythmSimpleElement.DotCount() > 0) {

                    for (int i = 0; i < rhythmSimpleElement.DotCount(); i++) {
                        beatElementCompositeGlyph.AddGlyph(UIGlyph.DotGlyph());
                    }
                }

            } else if (e.GetType() == typeof(RhythmTieFrom)) {
                beatElementCompositeGlyph.AddGlyph(UIGlyph.TiefromGlyph());
            }

            return beatElementCompositeGlyph;
        }

        private Helpers.UICompositeGlyph BuildBeatCompositeGlyph(BeatPattern beat) {
            Helpers.UICompositeGlyph beatCompositeGlyph = new();

            foreach (IRhythmElement e in beat.Elements) {
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

            _noteOk = false;
            if (_measureData == null) {
                return;
            }

            _noteOk = Pattern.IsCompatibleWithNoteCount(_measureData.GlobalMelody.Melody, _measureData.PrecedingMeasure);
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

                    if (element.GetType() == typeof(RhythmTieFrom)) {
                        //Cas des liaisons

                        RhythmTieFrom rhythmTieFrom = (RhythmTieFrom)element;

                        if (!beat.CanAddTieFrom()) {
                            return;
                        }

                        beat.Elements.Add(rhythmTieFrom);

                    } else if (element.GetType() == typeof(RhythmTuplet)) {

                        RhythmTuplet rhythmElement = (RhythmTuplet)element;

                        //if (element.Duration.Numerator == 0) {
                        //  return;
                        //}

                        if (!beat.CanAdd(rhythmElement, Pattern.TimeSignature.GetBeatDuration())) {
                            return;
                        }

                        beat.Elements.Add(rhythmElement);

                        
                    } else if (element.GetType() == typeof(RhythmSimpleElement)) {

                        RhythmSimpleElement rhythmElement = (RhythmSimpleElement)element;

                        //if (element.Duration.Numerator == 0) {
                        //  return;
                        //}

                        if (!beat.CanAdd(rhythmElement, Pattern.TimeSignature.GetBeatDuration())) {
                            return;
                        }

                        beat.Elements.Add(rhythmElement);

                        
                    }

                    _durationOk = Pattern.IsDurationValid() && Pattern.AreBeatsValid();

                    _noteOk = false;
                    if (_measureData == null) {
                        return;
                    }

                    _noteOk = Pattern.IsCompatibleWithNoteCount(_measureData.GlobalMelody.Melody, _measureData.PrecedingMeasure);

                    PatternChanged?.Invoke(Pattern);
                }
            };

            popover.Open(beat, remaining);
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
        public void LoadFromModel(MeasureData measureData) {

            _measureData = measureData;

            Pattern = measureData.GlobalMelody.Pattern;
                       
            _currentMelodyChordsCount = measureData.GlobalMelody.Melody.MelodyChords.Count;

            _durationOk = Pattern.IsDurationValid() && Pattern.AreBeatsValid();
            _noteOk = Pattern.IsCompatibleWithNoteCount(_measureData.GlobalMelody.Melody, _measureData.PrecedingMeasure);

            QueueDraw();

            //PatternChanged?.Invoke(Pattern);
        }

        internal void UpdateTimeSignature(TimeSignature timeSignature) {
            if (Pattern != null) {
                Pattern.TimeSignature = timeSignature;

                _durationOk = Pattern.IsDurationValid() && Pattern.AreBeatsValid();

                _noteOk = false;
                if (_measureData == null) {
                    return;
                }

                _noteOk = Pattern.IsCompatibleWithNoteCount(_measureData.GlobalMelody.Melody, _measureData.PrecedingMeasure);

                QueueDraw();
            }
        }
    }
}
