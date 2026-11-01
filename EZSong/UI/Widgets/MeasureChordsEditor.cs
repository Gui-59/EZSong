using Cairo;
using EZSong.Model;
using EZSong.UI.UX;
using EZSong.UI.Widgets.Helpers;
using EZSong.UI.Widgets.WidgetsData;
using Gdk;
using Gtk;
using Pango;
using System;
using System.Runtime.CompilerServices;

namespace EZSong.UI.Widgets {

    /// <summary>
    /// A widget that allows users to edit the chords of a measure in a song, 
    /// providing a visual representation of the beats and their associated chords.
    /// </summary>
    public class MeasureChordsEditor : DrawingArea {

        const int _height = 20; //Taille fixe pour éviter les problèmes de redimensionnement. TODO : calculer la taille en fonction de la police

        private MeasureData? _measureData;

        private ColorPaletteManager _colorPaletteManager = new();

        /// <summary>
        /// Creates a new instance of the MeasureChordsEditor widget, 
        /// initializing it with default settings and preparing it for user interaction.
        /// </summary>
        public MeasureChordsEditor() {
            HeightRequest = _height; 
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _measureData = new();
        }

        /// <summary>
        /// Creates a new instance of the MeasureChordsEditor widget with the specified measure data,
        /// </summary>
        /// <param name="measureData">The measure data to be edited.</param>
        public MeasureChordsEditor(MeasureData measureData) {
            HeightRequest = _height; 
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _measureData = measureData;
        }

        /// <summary>
        /// Renders the MeasureChordsEditor widget, drawing the background and the beats with their associated chords.
        /// </summary>
        /// <param name="cr">The Cairo context to draw on.</param>
        /// <returns>True if the drawing was successful, otherwise false.</returns>
        protected override bool OnDrawn(Cairo.Context cr) {

            if (_measureData == null) {
                return true;
            }

            if (_measureData.ChordSequence == null) {
                return true;
            }

            DrawBackground(cr);
            DrawBeats(cr);

            return true;
        }

        /// <summary>
        /// Draws the background of the MeasureChordsEditor widget using the specified Cairo context,
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
        /// Draws the beats of the measure, including their associated chords, using the specified Cairo context.
        /// </summary>
        /// <param name="cr">The Cairo context to draw on.</param>
        private void DrawBeats(Cairo.Context cr) {
            if (_measureData == null) {
                return;
            }

            if (_measureData.ChordSequence == null) {
                return;
            }

            double width = Allocation.Width;
            double height = Allocation.Height;

            int beatCount = _measureData.ChordSequence.GetBeatCount();

            if (beatCount == 0) {
                return;
            }

            double beatWidth = width / beatCount;

            for (int i = 0; i < beatCount; i++) {
                DrawBeat(cr, _measureData.ChordSequence.ChordBeats[i], i * beatWidth, beatWidth, height);
            }
        }

        /// <summary>
        /// Draws a single beat and its associated chords at the specified position and size using the provided Cairo context.
        /// </summary>
        /// <param name="cr">The Cairo context to draw on.</param>
        /// <param name="chordBeat">The chord beat to be drawn.</param>
        /// <param name="x">The x-coordinate of the top-left corner of the beat.</param>
        /// <param name="width">The width of the beat.</param>
        /// <param name="height">The height of the beat.</param>
        private void DrawBeat(Cairo.Context cr, ChordBeat chordBeat, double x, double width, double height) {

            string beatChordText = chordBeat.ToHumanString();

            cr.SetSourceRGB(
                _colorPaletteManager.FrameFore.R, 
                _colorPaletteManager.FrameFore.G, 
                _colorPaletteManager.FrameFore.B
            );

            //TODO : Faire une fois au départ du draw
            Gtk.Settings settings = Gtk.Settings.Default;
            String fontFamily = (string)settings.GetProperty("gtk-font-name"); //TODO : A revoir pour récupérer correctement la police par défaut de l'application
            cr.SelectFontFace(fontFamily, FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(13); //TODO : A revoir pour récupérer correctement la taille de police par défaut de l'application

            TextExtents ext = cr.TextExtents(beatChordText);

            double tx = x + 4;
            double ty = height / 2 + ext.Height / 2;

            cr.MoveTo(tx, ty);
            cr.ShowText(beatChordText);

            // séparation visuelle
            //TODO : ne pas mettre de ligne si c'est le dernier beat
            cr.SetSourceRGB(
                _colorPaletteManager.FrameFore.R, 
                _colorPaletteManager.FrameFore.G, 
                _colorPaletteManager.FrameFore.B
            );
            cr.MoveTo(x+ width, 0);
            cr.LineTo(x + width, height);
            cr.Stroke();
        }


        /// <summary>
        /// Event handler for mouse button press events, 
        /// allowing users to edit or clear chords on specific beats based on the mouse button pressed and the position of the click.
        /// </summary>
        /// <param name="ev">The mouse button event.</param>
        /// <returns>True if the event was handled; otherwise, false.</returns>
        protected override bool OnButtonPressEvent(Gdk.EventButton ev) {

            if (_measureData == null) {
                return false;
            }

            if (_measureData.ChordSequence == null) {
                return false;
            }

            double width = Allocation.Width;
            int beatCount = _measureData.ChordSequence.GetBeatCount();

            double beatWidth = width / beatCount;

            int index = (int)(ev.X / beatWidth);

            if (index < 0 || index >= beatCount) {
                return false;
            }          

            if (ev.Button == 3) {
                EditBeat(index);
            }

            if (ev.Button == 1 && ev.State == ModifierType.ShiftMask) { //TODO : permettre aussi le double click
                ClearBeat(index);
                //TODO : ne semble pas fonctionner ?
            }

            return true;
        }

        /// <summary>
        /// Clears the chords associated with a specific beat in the measure, 
        /// and triggers the ChordsChanged event to notify listeners of the change.
        /// </summary>
        /// <param name="index">The index of the beat to clear.</param>
        private void ClearBeat(int index) {

            if (_measureData == null) {
                return;
            }

            if (_measureData.ChordSequence == null) {
                return;
            }

            ChordBeat chordBeat = _measureData.ChordSequence.ChordBeats[index];
            chordBeat.Clear();

            ChordsChanged?.Invoke(_measureData.ChordSequence);
        }

        /// <summary>
        /// Edits the chords associated with a specific beat in the measure by opening a chord picker popover,
        /// </summary>
        /// <param name="index">The index of the beat to edit.</param>
        private void EditBeat(int index) {
            if (_measureData == null) {
                return;
            }

            if (_measureData.ChordSequence == null) {
                return;
            }

           CreateElementFromUserChoice(index);
        }

        /// <summary>
        /// Creates a chord element for a specific beat based on user choice in the chord picker popover,
        /// </summary>
        /// <param name="beatIndex">The index of the beat to create the chord element for.</param>
        void CreateElementFromUserChoice(int beatIndex) {
            if (_measureData == null) {
                return;
            }

            if (_measureData.ChordSequence == null) {
                return;
            }

            ChordBeat chordBeat = _measureData.ChordSequence.ChordBeats[beatIndex];

            ChordPickerPopover popover = new(this);

            double width = Allocation.Width;
            double height = Allocation.Height;
            int beatCount = _measureData.ChordSequence.ChordBeats.Count;
            double beatWidth = width / beatCount;

            int beatX = (int)(beatIndex * beatWidth);

            popover.PointingTo = new Gdk.Rectangle(
                (int)beatX,
                0,
                (int)beatWidth,
                (int)height
            );

            RhythmRationalDuration remaining = chordBeat.GetRemainingDuration();

            popover.ElementSelected += element => {

                ChordBeat chordBeat = _measureData.ChordSequence.ChordBeats[beatIndex];

                if (element == null) {
                    return;
                } else {
                    if (element == null) {
                        return;
                    }

                    Chord chord = (Chord)element;

                    if (!chordBeat.CanAdd(chord)) {
                        return;
                    }

                    chordBeat.AddChord(chord);

                    ChordsChanged?.Invoke(_measureData.ChordSequence);
                }
            };

            popover.Open(chordBeat, remaining);
            
        }
        
        public event Action<ChordSequence>? ChordsChanged;

        /// <summary>
        /// Loads the measure data into the MeasureChordsEditor widget, 
        /// updating its internal state and triggering a redraw to reflect the new data.
        /// </summary>
        /// <param name="measureData"></param>
        public void LoadFromModel(MeasureData measureData) {
            _measureData = measureData;
            QueueDraw();
        }

        /// <summary>
        /// Updates the time signature of the measure data in the MeasureChordsEditor widget,
        /// </summary>
        /// <param name="timeSignature">The new time signature to apply to the measure data.</param>
        internal void UpdateTimeSignature(TimeSignature timeSignature) {
            if (_measureData == null) {
                return;
            }
            
            if (_measureData.ChordSequence != null) {
                _measureData.TimeSignature = timeSignature;
                QueueDraw();
            }
        }
    }
}
