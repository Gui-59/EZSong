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

    /// <summary>
    /// A widget that allows users to edit the rhythm of a measure in a musical score.
    /// </summary>
    public class MeasureRhythmEditor : DrawingArea {

        int _statusAreaHeight = 15; //TODO : Const ?

        const int _height = 50; //Taille fixe pour éviter les problèmes de redimensionnement

        private int _segmentIndex;
        private int _staffIndex;

        private MeasureData? _measureData;

        private ColorPaletteManager _colorPaletteManager = new();

        private string _musicalFontFamily = new Settings.UserSettings().MusicalFontFamily;

        /// <summary>
        /// Determines whether the current note pattern is compatible with the measure's rhythm pattern,
        /// </summary>
        /// <returns>True if the note pattern is compatible; otherwise, false.</returns>
        private bool NoteOK() {
            if (_measureData == null) {
                return false;
            }
            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return false;
            }
            return _measureData.Staffs[_staffIndex].Pattern.IsCompatibleWithNoteCount(_measureData.Staffs[_staffIndex].Melody, _measureData.PrecedingMeasure);
        }

        /// <summary>
        /// Creates a new instance of the MeasureRhythmEditor widget, 
        /// initializing it with default values and setting up event handling for user interactions.
        /// </summary>
        public MeasureRhythmEditor() {
            HeightRequest = _height; 
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _staffIndex = 0; //Par défaut
            _measureData = new();
        }

        /// <summary>
        /// Creates a new instance of the MeasureRhythmEditor widget with specified segment and staff indices,
        /// initializing it with the provided measure data and setting up event handling for user interactions.
        /// </summary>
        /// <param name="segmentIndex">The index of the segment to display.</param>
        /// <param name="staffIndex">The index of the staff to display.</param>
        /// <param name="measureData">The measure data to be edited.</param>
        public MeasureRhythmEditor(int segmentIndex, int staffIndex, MeasureData measureData) {
            HeightRequest = _height;
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _segmentIndex = segmentIndex;
            _staffIndex = staffIndex;
            _measureData = measureData;
        }

        /// <summary>
        /// Renders the MeasureRhythmEditor widget, 
        /// drawing the background, status area, and beats based on the current measure data and staff index.
        /// </summary>
        /// <param name="cr">The Cairo context to draw on.</param>
        /// <returns>True if the drawing was successful; otherwise, false.</returns>
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

        /// <summary>
        /// Draws the background of the MeasureRhythmEditor widget using the specified Cairo context,
        /// </summary>
        /// <param name="cr">The Cairo context to draw on.</param>
        private void DrawBackground(Cairo.Context cr) {
            cr.SetSourceRGB(
                _colorPaletteManager.FrameBg.R, 
                _colorPaletteManager.FrameBg.G, 
                _colorPaletteManager.FrameBg.B
            );
            cr.Paint();
        }

        /// <summary>
        /// Draws the beats of the measure based on the current measure data and staff index,
        /// </summary>
        /// <param name="cr">The Cairo context to draw on.</param>
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

        /// <summary>
        /// Builds a composite glyph representing a single beat element, based on its type (tie, tuplet, or simple rhythm element),
        /// </summary>
        /// <param name="e">The rhythm element to build a composite glyph for.</param>
        /// <returns>A UICompositeGlyph representing the rhythm element.</returns>
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

        /// <summary>
        /// Builds a composite glyph representing an entire beat pattern,
        /// </summary>
        /// <param name="beat">The beat pattern to build a composite glyph for.</param>
        /// <returns>A UICompositeGlyph representing the beat pattern.</returns>
        private Helpers.UICompositeGlyph BuildBeatCompositeGlyph(BeatPattern beat) {
            Helpers.UICompositeGlyph beatCompositeGlyph = new();

            foreach (IRhythmElement e in beat.Elements) {
                beatCompositeGlyph.AddCompositeGlyph(BuildBeatElementCompositeGlyph(e));
            }

            return beatCompositeGlyph;
        }


        /// <summary>
        /// Draws a single beat within the MeasureRhythmEditor widget, using the specified Cairo context and positioning parameters,
        /// </summary>
        /// <param name="cr">The Cairo context to draw on.</param>
        /// <param name="beat">The beat pattern to draw.</param>
        /// <param name="x">The x-coordinate to start drawing the beat.</param>
        /// <param name="width">The width allocated for the beat.</param>
        /// <param name="height">The height allocated for the beat.</param>
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

        /// <summary>
        /// Event handler for mouse button press events within the MeasureRhythmEditor widget,
        /// </summary>
        /// <param name="ev">The mouse button press event.</param>
        /// <returns>True if the event was handled; otherwise, false.</returns>
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


        /// <summary>
        /// Determines whether the specified y-coordinate is within the status area of the MeasureRhythmEditor widget,
        /// </summary>
        /// <param name="y">The y-coordinate to check.</param>
        /// <returns>True if the y-coordinate is within the status area; otherwise, false.</returns>
        private bool IsInStatusZone(double y) {
            if (y <= _statusAreaHeight) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clears the rhythm elements of a specific beat in the measure, based on the provided index,
        /// </summary>
        /// <param name="index">The index of the beat to clear.</param>
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


        /// <summary>
        /// Edits the rhythm elements of a specific beat in the measure, based on the provided index,
        /// </summary>
        /// <param name="index">The index of the beat to edit.</param>
        private void EditBeat(int index) {

            if (_measureData == null) {
                return;
            }

            if (_measureData.Staffs[_staffIndex].Pattern == null) {
                return;
            }          

            CreateElementFromUserChoice(index);
        }

        /// <summary>
        /// Creates a new rhythm element for a specific beat in the measure, based on the user's choice from a popover dialog,
        /// </summary>
        /// <param name="index">The index of the beat for which to create a new rhythm element.</param>
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
        
        /// <summary>
        /// Determines whether the first rhythm duration is greater than the second rhythm duration.
        /// </summary>
        /// <param name="a">The first rhythm duration.</param>
        /// <param name="b">The second rhythm duration.</param>
        /// <returns>True if the first rhythm duration is greater than the second; otherwise, false.</returns>
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

        /// <summary>
        /// Draws the status area of the MeasureRhythmEditor widget, 
        /// indicating whether the current measure's rhythm pattern is valid, 
        /// has warnings, 
        /// or is successful, 
        /// using the specified Cairo context.
        /// </summary>
        /// <param name="cr">The Cairo context used for drawing.</param>
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

        /// <summary>
        /// Loads the measure data model into the MeasureRhythmEditor widget, 
        /// updating its internal state and triggering a redraw of the widget to reflect the new data.
        /// </summary>
        /// <param name="measureData">The measure data model to load into the widget.</param>
        public void LoadFromModel(MeasureData measureData) {
            _measureData = measureData;
            QueueDraw();
        }

        /// <summary>
        /// Updates the time signature of the measure's rhythm pattern for all staffs in the MeasureRhythmEditor widget,
        /// </summary>
        /// <param name="timeSignature">The new time signature to apply to the measure's rhythm pattern.</param>
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

        /// <summary>
        /// Refreshes the displayed segment in the MeasureRhythmEditor widget,
        /// </summary>
        /// <param name="displayedSegmentIndex">The index of the segment to display.</param>
        internal void RefreshDisplayedSegment(int displayedSegmentIndex) {
            _segmentIndex = displayedSegmentIndex;
            RefreshDisplayedStaff(_staffIndex);
        }

        /// <summary>
        /// Refreshes the displayed staff in the MeasureRhythmEditor widget,
        /// </summary>
        /// <param name="displayedStaffIndex">The index of the staff to display.</param>
        internal void RefreshDisplayedStaff(int displayedStaffIndex) {
            _staffIndex = displayedStaffIndex;
            QueueDraw();
        }
    }
}
