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
    public class MeasureChordsEditor : DrawingArea {

        const int _height = 20; //Taille fixe pour éviter les problèmes de redimensionnement. TODO : calculer la taille en fonction de la police

        private MeasureData? _measureData;

        private ColorPaletteManager _colorPaletteManager = new();

        public MeasureChordsEditor() {
            HeightRequest = _height; 
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _measureData = new();
        }

        public MeasureChordsEditor(MeasureData measureData) {
            HeightRequest = _height; 
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _measureData = measureData;
        }

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

        private void DrawBackground(Cairo.Context cr) {
            cr.SetSourceRGB(
                _colorPaletteManager.FrameBg.R, 
                _colorPaletteManager.FrameBg.G, 
                _colorPaletteManager.FrameBg.B
            );
            cr.Paint();
        }

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

        private void EditBeat(int index) {
            if (_measureData == null) {
                return;
            }

            if (_measureData.ChordSequence == null) {
                return;
            }

           CreateElementFromUserChoice(index);
        }

        //Popover qui permet de choisir l'accord, la durée et le type d'accord (majeur, mineur, 7ème, etc.)
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

        // PUBLIC API: load external model into widget
        public void LoadFromModel(MeasureData measureData) {
            _measureData = measureData;
            QueueDraw();
        }

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
