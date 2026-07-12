using Cairo;
using EZSong.Model;
using EZSong.UI.UX;
using EZSong.UI.Widgets.Helpers;
using EZSong.UI.Widgets.WidgetsData;
using Gdk;
using Gtk;
using System;
using System.Runtime.CompilerServices;

namespace EZSong.UI.Widgets {
    public class MeasureRhythmEditor : DrawingArea {

        int _statusAreaHeight = 15; //TODO : Const ?

        const int _height = 50; //Taille fixe pour éviter les problèmes de redimensionnement

        private int _staffIndex;

        private MeasureData? _measureData;

        private ColorPaletteManager _colorPaletteManager = new();

        private string _musicalFontFamily = new Settings.UserSettings().MusicalFontFamily;

        private bool NoteOK() {
            if (_measureData == null) {
                return false;
            }
            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return false;
            }
            return _measureData.Staffs[_staffIndex].Pattern.IsCompatibleWithNoteCount(_measureData.Staffs[_staffIndex].Melody, _measureData.PrecedingMeasure);
        }
 
        public MeasureRhythmEditor() {
            HeightRequest = _height; 
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _staffIndex = 0; //Par défaut
            _measureData = new();
        }

        public MeasureRhythmEditor(int staffIndex, MeasureData measureData) {
            HeightRequest = _height;
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _staffIndex = staffIndex;
            _measureData = measureData;
        }

        protected override bool OnDrawn(Cairo.Context cr) {

            if (_measureData == null) {
                return true;
            }
            
            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return true;
            }

            DrawBackground(cr);
            DrawStatus(cr);
            DrawBeats(cr);
            
            return true;
        }

        private void DrawBackground(Cairo.Context cr) {
            cr.SetSourceRGB(
                _colorPaletteManager.FrameBg.R, 
                _colorPaletteManager.FrameBg.G, 
                _colorPaletteManager.FrameBg.B
            );
            cr.Paint();
        }

        private void DrawBeats(Context cr) {

            if (_measureData == null) {
                return;
            }
            
            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return;
            }

            double width = Allocation.Width;
            double height = Allocation.Height;

            int beatCount = _measureData.Staffs[_staffIndex].Pattern.TimeSignature.GetBeatCount();

            if (beatCount == 0) {
                return;
            }

            double beatWidth = width / beatCount;

            for (int i = 0; i < beatCount; i++) {
                double x = i * beatWidth;
                DrawBeat(cr, _measureData.Staffs[_staffIndex].Pattern.Beats[i], x, beatWidth, height);
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

            cr.SetSourceRGB(
                _colorPaletteManager.FrameFore.R, 
                _colorPaletteManager.FrameFore.G,
                _colorPaletteManager.FrameFore.B
            );

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

            if (_measureData == null) {
                return false;
            }

            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return false;
            }

            double width = Allocation.Width;
            int beatCount = _measureData.Staffs[_staffIndex].Pattern.Beats.Count;

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

            if (_measureData == null) {
                return;
            }

            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return;
            }

            BeatPattern beat = _measureData.Staffs[_staffIndex].Pattern.Beats[index];
            beat.Elements.Clear();

            PatternChanged?.Invoke(_staffIndex, _measureData.Staffs[_staffIndex].Pattern);
        }

        private void EditBeat(int index) {

            if (_measureData == null) {
                return;
            }

            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return;
            }          

            CreateElementFromUserChoice(index);
        }

        void CreateElementFromUserChoice(int index) {

            if (_measureData == null) {
                return;
            }
            
            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return;
            }

            BeatPattern beat = _measureData.Staffs[_staffIndex].Pattern.Beats[index];

            RhythmElementPickerPopover popover = new(this);

            double width = Allocation.Width;
            int beatCount = _measureData.Staffs[_staffIndex].Pattern.Beats.Count;
            double beatWidth = width / beatCount;

            int beatX = (int)(index * beatWidth);

            popover.PointingTo = new Gdk.Rectangle(
                (int)beatX,
                0,
                (int)beatWidth,
                _statusAreaHeight
            );

            RhythmRationalDuration remaining = beat.GetRemainingDuration(_measureData.Staffs[_staffIndex].Pattern.TimeSignature.GetBeatDuration());

            popover.ElementSelected += element => {

                BeatPattern beat = _measureData.Staffs[_staffIndex].Pattern.Beats[index];

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

                        if (!beat.CanAdd(rhythmElement, _measureData.Staffs[_staffIndex].Pattern.TimeSignature.GetBeatDuration())) {
                            return;
                        }

                        beat.Elements.Add(rhythmElement);
                        
                    } else if (element.GetType() == typeof(RhythmSimpleElement)) {

                        RhythmSimpleElement rhythmElement = (RhythmSimpleElement)element;

                        if (!beat.CanAdd(rhythmElement, _measureData.Staffs[_staffIndex].Pattern.TimeSignature.GetBeatDuration())) {
                            return;
                        }

                        beat.Elements.Add(rhythmElement);
                    }

                    PatternChanged?.Invoke(_staffIndex, _measureData.Staffs[_staffIndex].Pattern);
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

        public event Action<int, MeasureRhythmPattern>? PatternChanged; //Premier paramètre : l'index de la portée
  
        private void DrawStatus(Context cr) {
            
            if (_measureData == null) {
                return;
            }

            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return;
            }

            if (!_measureData.Staffs[_staffIndex].Pattern.IsDurationValid() || !_measureData.Staffs[_staffIndex].Pattern.AreBeatsValid()) {
                cr.SetSourceRGB(
                    _colorPaletteManager.ErrorBg.R,
                    _colorPaletteManager.ErrorBg.G,
                    _colorPaletteManager.ErrorBg.B
                ); 
            } else if (!NoteOK()) {
                cr.SetSourceRGB(
                    _colorPaletteManager.WarningBg.R,
                    _colorPaletteManager.WarningBg.G,
                    _colorPaletteManager.WarningBg.B
                ); 
            } else {
                cr.SetSourceRGB(
                    _colorPaletteManager.SuccessBg.R,
                    _colorPaletteManager.SuccessBg.G,
                    _colorPaletteManager.SuccessBg.B                    
                );
            }

            cr.Rectangle(0, 0, Allocation.Width, _statusAreaHeight);
            cr.Fill();

            double width = Allocation.Width;
            int beatCount = _measureData.Staffs[_staffIndex].Pattern.TimeSignature.GetBeatCount();

            double beatWidth = width / beatCount;

            for (int i = 0; i < beatCount; i++) {
                double x = i * beatWidth;

                // séparation visuelle
                cr.SetSourceRGB(
                    _colorPaletteManager.FrameSubtleLine.R,
                    _colorPaletteManager.FrameSubtleLine.G,
                    _colorPaletteManager.FrameSubtleLine.B
                );
                cr.MoveTo(x, 0);
                cr.LineTo(x, _statusAreaHeight);
                cr.Stroke();
            }
        }

        // PUBLIC API: load external model into widget
        public void LoadFromModel(MeasureData measureData) {
            _measureData = measureData;
            QueueDraw();
        }

        internal void UpdateTimeSignature(TimeSignature timeSignature) {
            if (_measureData == null) {
                return;
            }
            for (int staffIndex = 0; staffIndex < _measureData.Staffs.Count(); staffIndex++) {
                if (_measureData.Staffs[staffIndex].Pattern != null) {
                    _measureData.Staffs[staffIndex].Pattern.TimeSignature = timeSignature;

                    QueueDraw();
                }
            }   
        }

        internal void RefreshDisplayedStaff(int displayedStaffIndex) {
            _staffIndex = displayedStaffIndex;
            QueueDraw();
        }
    }
}
