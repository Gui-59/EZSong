using Cairo;
using EZSong.Model;
using EZSong.UI.Widgets.Helpers;
using EZSong.UI.Widgets.WidgetsData;
using Gdk;
using Gtk;
using System;
using System.Runtime.CompilerServices;

namespace EZSong.UI.Widgets {
    public class MeasureChordsEditor : DrawingArea {

        private MeasureData? _measureData;

        public MeasureChordsEditor() {
            HeightRequest = 20; //Taille fixe pour éviter les problèmes de redimensionnement
            AddEvents((int)Gdk.EventMask.ButtonPressMask);
            _measureData = new();
        }

        public MeasureChordsEditor(MeasureData measureData) {
            HeightRequest = 20; //Taille fixe pour éviter les problèmes de redimensionnement
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
            cr.SetSourceRGB(1, 1, 0.8);
            cr.Paint();
        }

        private void DrawBeats(Context cr) {
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
                double x = i * beatWidth;

                DrawBeat(cr, _measureData.ChordSequence.ChordBeats[i], x, beatWidth, height);
            }
        }

        private void DrawBeat(Context cr, ChordBeat chordBeat, double x, double width, double height) {

            string beatChordText = chordBeat.ToHumanString();

            cr.SetSourceRGB(0, 0, 0);

            //TODO : Faire une fois au départ du draw
            //TODO : Permettre de changer la police dans les settings
            cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
            cr.SetFontSize(20);

            TextExtents ext = cr.TextExtents(beatChordText);

            double tx = x + (width - ext.Width) / 2;
            double ty = height / 2;

            cr.MoveTo(tx, ty);
            cr.ShowText(beatChordText);
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



            //TODO : faire un popover qui permet de choisir l'accord, la durée et le type d'accord (majeur, mineur, 7ème, etc.)
           CreateElementFromUserChoice(index);

        }

        //popover qui permet de choisir l'accord, la durée et le type d'accord (majeur, mineur, 7ème, etc.)
        void CreateElementFromUserChoice(int beatIndex) {
            if (_measureData == null) {
                return;
            }
            if (_measureData.ChordSequence == null) {
                return;
            }

            ChordBeat chordBeat = _measureData.ChordSequence.ChordBeats[beatIndex];

            ChordPickerPopover popover = new(this); //popover qui permet de choisir l'accord, la durée et le type d'accord (majeur, mineur, 7ème, etc.)

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
