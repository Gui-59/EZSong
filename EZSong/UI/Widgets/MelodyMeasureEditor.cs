using Cairo;
using EZSong.Enums;
using EZSong.MIDI;
using EZSong.MIDI.Enums;
using EZSong.Model;
using EZSong.Settings;
using EZSong.UI.UX;
using EZSong.UI.Widgets.Helpers;
using EZSong.UI.Widgets.WidgetsData;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets {
    public class MelodyMeasureEditor : DrawingArea {

        private bool _disposed;

        private UserSettings _userSettings;

        private int _segmentIndex;
        private int _staffIndex;
        private MeasureData _measureData;

        // Model: sequence of chords (positioned sequentially)
        private List<WidgetMelodyChord> _widgetMelodyChords = new();

        

        private ColorPaletteManager _colorPaletteManager = new();

        private string _musicalFontFamily = new Settings.UserSettings().MusicalFontFamily;
        private Helpers.UICompositeGlyph _noteSymbolCompositeGlyph;
        private int _noteSymbolFontSize = -1; //Will be computed

        

        // Public properties for configuration
        public int TargetedNoteSymbolHeightPx { get; set; } = 8; //Prefered : 8
        public int TargetedNoteFrameWidthPx { get; set; } = 16; // width reserved per chord slot. Prefered : 2 * TargetedNoteSymbolHeightPx
        public int DisplayedOctaveCount { get; set; } = 3; // number of octaves to display; must be even

        // Cursor index: insertion point between chords (0..Count)
        private int _cursorIndex = 0;
        private double _actualNoteHeight = 1; //Will be computed

        // Cursor blink
        private bool _cursorVisible = true;
        private uint _cursorTimeoutId;

        // Interaction state
        private int _hoverColumn = -1;
        private int _hoverRow = -1;

        // Events
        public event EventHandler? ContentChanged; // raised when chords/cadency change
        public event EventHandler<int>? CursorChanged; // cursor index changed

        //Pour echo MIDI
        private EmbeddedMidiSynth _embeddedMidiSynth; 


        private void AddWidgetMelodyChord(WidgetMelodyChord widgetMelodyChord) {
            _widgetMelodyChords.Add(widgetMelodyChord);
            _measureData.Staffs[_staffIndex].Melody.MelodyChords = _widgetMelodyChords.Select(c => c.ToMelodyChord()).ToList();
        }

        private void ReplaceMelodyChord(int cursorIndex, WidgetMelodyChord chord) {
            _widgetMelodyChords[cursorIndex] = chord;
            _measureData.Staffs[_staffIndex].Melody.MelodyChords = _widgetMelodyChords.Select(c => c.ToMelodyChord()).ToList();
        }

        private void RemoveWidgetMelodyChord(int cursorIndex) {
            _widgetMelodyChords.RemoveAt(cursorIndex);
            _measureData.Staffs[_staffIndex].Melody.MelodyChords = _widgetMelodyChords.Select(c => c.ToMelodyChord()).ToList();
        }

        public MelodyMeasureEditor(EmbeddedMidiSynth embeddedMidiSynth, UserSettings userSettings) {

            _measureData = new MeasureData();

            _userSettings = userSettings;
            // Enable events
            AddEvents((int)(
                EventMask.ButtonPressMask |
                EventMask.ButtonReleaseMask |
                EventMask.PointerMotionMask |
                EventMask.KeyPressMask |
                EventMask.KeyReleaseMask |
                EventMask.FocusChangeMask
            ));

            // Set a minimum height
            HeightRequest = (DisplayedOctaveCount * 12) * TargetedNoteSymbolHeightPx;

            //Define note symbol
            _noteSymbolCompositeGlyph = new();
            _noteSymbolCompositeGlyph.AddGlyph(new UIGlyph(Enums.Glyph.UndefindedDurationNote));          

            // Input handlers
            ButtonPressEvent += OnButtonPress;
            MotionNotifyEvent += OnMotion;
            KeyPressEvent += OnKeyPress;

            // Start the cursor blink timer
            _cursorTimeoutId = GLib.Timeout.Add(500, OnCursorTimer);
            CanFocus = true;

            _embeddedMidiSynth = embeddedMidiSynth;
        }

        public new void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            StopCursorTimer();
        }

        public void StopCursorTimer() {
            if (_cursorTimeoutId != 0) {
                _ = GLib.Source.Remove(_cursorTimeoutId);
                _cursorTimeoutId = 0;
            }
        }

        private void ComputeMusicalFontSize(Context cr) {
            cr.SelectFontFace(_musicalFontFamily, FontSlant.Normal, FontWeight.Normal);
            
            for (int i = 1; i < 100; i++) {
                _noteSymbolFontSize = i;
                cr.SetFontSize(_noteSymbolFontSize);
                TextExtents ext = cr.TextExtents(_noteSymbolCompositeGlyph.ToString());
                if (ext.Height > TargetedNoteSymbolHeightPx) {
                    _noteSymbolFontSize -= 1;
                    return;
                }
                
            }
            return;            
        }

        protected override bool OnDrawn(Context cr) {

            //Calculate note symbol size
            if (_noteSymbolFontSize <= 0) {                
                ComputeMusicalFontSize(cr);
            }

            // Clear background
            cr.SetSourceRGB(
                _colorPaletteManager.FrameBg.R,
                _colorPaletteManager.FrameBg.G,
                _colorPaletteManager.FrameBg.B
            );
            cr.Paint();

            // Compute geometry
            int rows = DisplayedOctaveCount * 12;
            // draw staff-like grid (simple horizontal lines for notes)
            double totalHeight = rows * TargetedNoteSymbolHeightPx;
            _actualNoteHeight = totalHeight / rows;

            // Recherche de l'index d'octave la plus aigue actuellement affichée (point de départ)
            int octavesAboveOrBelowBaseOctave = (DisplayedOctaveCount - 1) / 2;
            int octaveOffset = 0 + octavesAboveOrBelowBaseOctave; //La ligne du haut est la plus aigue

            bool isUpperOctave = true;

            // Draw horizontal separators for each visible pitch
            for (int r = 0; r < rows; r++) {
                double y = (r * _actualNoteHeight) + (_actualNoteHeight / 2.0);
                int noteIndex = rows - 1 - r; // top row = highest pitch
                int noteIntexInOctave = noteIndex % 12;
                if (noteIntexInOctave == 11) {
                    //A chaque fois qu'on retombe sur la note la plus haute d'une octave, on descend d'une octave
                    //Mais on ne tiens pas compte du 'si' le plus haut
                    if (isUpperOctave) {
                        isUpperOctave = false;
                    } else {
                        octaveOffset--;
                    }   
                }
                
                DrawPitchArea(cr, y, noteIntexInOctave, octaveOffset);
            }

            // Draw vertical chord columns and existing chords
            for (int i = 0; i <= _widgetMelodyChords.Count; i++) {
                double x = i * TargetedNoteFrameWidthPx;
                // subtle vertical tick
                cr.SetSourceRGBA(
                    _colorPaletteManager.FrameSubtleLine.R,
                    _colorPaletteManager.FrameSubtleLine.G,
                    _colorPaletteManager.FrameSubtleLine.B,
                    _colorPaletteManager.FrameSubtleLine.A
                );
                cr.LineWidth = 0.5;
                cr.MoveTo(x, -4);
                cr.LineTo(x, totalHeight + 4);
                cr.Stroke();
            }

            // Draw melody chords
            for (int i = 0; i < _widgetMelodyChords.Count; i++) {
                double center_x = i * TargetedNoteFrameWidthPx + (TargetedNoteFrameWidthPx / 2);
                DrawMelodyChordAt(cr, _widgetMelodyChords[i], center_x, 0, totalHeight);
            }

            // Ajustement automatique du spacing si trop de colonnes
            int defaultSpacing = TargetedNoteFrameWidthPx; //TODO : rendre configurable
            int maxVisibleCols = Math.Max(1, (Allocation.Width - (TargetedNoteFrameWidthPx / 2) * 2) / defaultSpacing);
            TargetedNoteFrameWidthPx = _widgetMelodyChords.Count > maxVisibleCols
                ? (Allocation.Width - (TargetedNoteFrameWidthPx / 2) * 2) / _widgetMelodyChords.Count
                : defaultSpacing;

            // Draw cursor (vertical line between chords) at _cursorIndex
            if (_cursorVisible && HasFocus) {
                double x = (TargetedNoteFrameWidthPx / 2) + _cursorIndex * TargetedNoteFrameWidthPx - TargetedNoteFrameWidthPx / 2.0;
                cr.SetSourceRGBA(
                    _colorPaletteManager.CursorLine.R, 
                    _colorPaletteManager.CursorLine.G, 
                    _colorPaletteManager.CursorLine.B, 
                    _colorPaletteManager.CursorLine.A
                );
                cr.LineWidth = 2.5;
                cr.MoveTo(x,  -6);
                cr.LineTo(x, totalHeight + 6);
                cr.Stroke();
            }

            return true;
        }

        private void DrawPitchArea(Context cr, double y, int noteInOctave, int octaveOffset) {

            //Attention : on compte ici 12 notes par octaves (une ligne = 1 demi-ton)

            //on fait un piano roll, donc on ne met pas de clé de sol ni de clé de fa, ni de clé d’ut. On ne met pas non plus de lignes supplémentaires pour les notes en dehors de la portée. On se contente d’un repère visuel simple pour aider à placer les notes.

            //Ligne pleine
            cr.LineWidth = _actualNoteHeight;
            if (octaveOffset % 2 == 0) {
                //Cas de l'octave "centrale" (et autres octaves paires)
                //L'octave centrale est différente selon si on est sur une mélodie ou des basses
                if (noteInOctave == 0 || noteInOctave == 2 || noteInOctave == 4 || noteInOctave == 5 || noteInOctave == 7 || noteInOctave == 9 || noteInOctave == 11) {
                    cr.SetSourceRGBA(
                        _colorPaletteManager.NaturalEvenPianoKey.R,
                        _colorPaletteManager.NaturalEvenPianoKey.G, 
                        _colorPaletteManager.NaturalEvenPianoKey.B, 
                        _colorPaletteManager.NaturalEvenPianoKey.A
                    ); // notes naturelles
                } else {
                    cr.SetSourceRGBA(
                        _colorPaletteManager.AlteredEvenPianoKey.R, 
                        _colorPaletteManager.AlteredEvenPianoKey.G, 
                        _colorPaletteManager.AlteredEvenPianoKey.B, 
                        _colorPaletteManager.AlteredEvenPianoKey.A
                    ); // altérations
                }
            } else {
                if (noteInOctave == 0 || noteInOctave == 2 || noteInOctave == 4 || noteInOctave == 5 || noteInOctave == 7 || noteInOctave == 9 || noteInOctave == 11) {
                    cr.SetSourceRGBA(
                        _colorPaletteManager.NaturalOddPianoKey.R, 
                        _colorPaletteManager.NaturalOddPianoKey.G, 
                        _colorPaletteManager.NaturalOddPianoKey.B,
                        _colorPaletteManager.NaturalOddPianoKey.A
                    ); // notes naturelles
                } else {
                    cr.SetSourceRGBA(
                        _colorPaletteManager.AlteredOddPianoKey.R, 
                        _colorPaletteManager.AlteredOddPianoKey.G, 
                        _colorPaletteManager.AlteredOddPianoKey.B, 
                        _colorPaletteManager.AlteredOddPianoKey.A
                    ); // altérations
                }
            }
            cr.SetDash(Array.Empty<double>(), 0); // repasse en ligne pleine
            cr.MoveTo(0, y);
            cr.LineTo(Allocation.Width, y);
            cr.Stroke();
        }

        private int GetMidiNoteNumberFromClickedRow(int rowFromTop) {
            int upperDisplayedMidiNoteNumber = GetUpperDisplayedMidiNoteNumber();
            return upperDisplayedMidiNoteNumber - (rowFromTop);
        }

        private int GetRowFromTopFromMidiNoteNumber(int midiNoteNumber) {
            int upperDisplayedMidiNoteNumber = GetUpperDisplayedMidiNoteNumber();
            return upperDisplayedMidiNoteNumber - midiNoteNumber;
        }

        private int GetUpperDisplayedMidiNoteNumber() {
            int baseOctave = _measureData.SongSettings.StaffsSettings.GetStaffBaseOctave(_staffIndex);

            //TODO : Simplifier (pour être plus efficace)
            int octavesAboveOrBelowBaseOctave = (DisplayedOctaveCount - 1) / 2;
            Pitch UpperDisplayedMidiNoteNumberPitch = new(NoteStep.B, Alteration.neutral, baseOctave + octavesAboveOrBelowBaseOctave);
            return UpperDisplayedMidiNoteNumberPitch.ToWidgetPitch().MidiNoteNumber;
        }

        private void DrawMelodyChordAt(Context cr, WidgetMelodyChord chord, double center_x, double areaTop, double areaHeight) {
            int rows = DisplayedOctaveCount * 12;
            double rowHeight = areaHeight / rows;

            // For each pitch draw a circle at appropriate row
            foreach (WidgetPitch p in chord.Pitches) {
                int rowFromTop = GetRowFromTopFromMidiNoteNumber(p.MidiNoteNumber);
                double center_y = areaTop + rowFromTop * rowHeight + rowHeight / 2.0;

                //Draw note shape                
                cr.SetSourceRGBA(
                    _colorPaletteManager.PianoNoteBg.R,
                    _colorPaletteManager.PianoNoteBg.G,
                    _colorPaletteManager.PianoNoteBg.B,
                    _colorPaletteManager.PianoNoteBg.A
                 );
                cr.SelectFontFace(_musicalFontFamily, FontSlant.Normal, FontWeight.Normal);
                cr.SetFontSize(_noteSymbolFontSize);

                TextExtents ext = cr.TextExtents(_noteSymbolCompositeGlyph.ToString());
                double tx = center_x - (ext.Width/2);

                cr.MoveTo(tx, center_y);
                cr.ShowText(_noteSymbolCompositeGlyph.ToString());
            }
        }

        private bool OnCursorTimer() {
            _cursorVisible = !_cursorVisible;
            QueueDraw();
            return true;
        }

        // PUBLIC API: load external model into widget
        public void LoadFromModel(int staffIndex, MeasureData measureData) {
            _staffIndex = staffIndex;
            _measureData = measureData;
            _widgetMelodyChords = (List<WidgetMelodyChord>)measureData.Staffs[staffIndex].Melody.ToWidgetMelodyChords();
            _cursorIndex = Math.Max(0, Math.Min(_widgetMelodyChords.Count, 0));
            QueueDraw();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private WidgetMelodyChord DeepCopyChord(WidgetMelodyChord s) {
            WidgetMelodyChord c = new();
            foreach (WidgetPitch p in s.Pitches) {
                c.Pitches.Add(new WidgetPitch(p.MidiNoteNumber));
            }
            return c;
        }

        public List<WidgetMelodyChord> ExportToModel() {
            //todo : éviter le Select (lent)
            return _widgetMelodyChords.Select(DeepCopyChord).ToList();
        }       

        // Interaction handlers

        private void OnMotion(object o, MotionNotifyEventArgs args) {
            double ax = args.Event.X;
            double ay = args.Event.Y;
            HitTest(ax, ay, out int col, out int row);
            _hoverColumn = col;
            _hoverRow = row;
            // not doing anything else for now
        }

        // Return row index (0..rows-1 from top) and column (0..count)
        private void HitTest(double x, double y, out int column, out int row) {
            
            column = (int)Math.Round((x - (TargetedNoteFrameWidthPx / 2)) / TargetedNoteFrameWidthPx);
            if (column < 0) {
                column = 0;
            }

            if (column > Math.Max(0, _widgetMelodyChords.Count)) {
                column = _widgetMelodyChords.Count;
            }

            // compute row from top
            if (y < 0) {
                row = -1;
                return;
            }

            int r = (int)Math.Floor(y / _actualNoteHeight);
            if (r < 0) {
                r = 0;
            }

            int displayedRowCount = DisplayedOctaveCount * 12;
            if (r >= displayedRowCount) {
                r = displayedRowCount - 1;
            }

            row = r;
        }

        private void OnButtonPress(object o, ButtonPressEventArgs args) {
            GrabFocus(); // ensure keyboard focus

            double x = args.Event.X;
            double y = args.Event.Y;
            HitTest(x, y, out int col, out int clickedRowFromTop);

            bool isLeft = args.Event.Button == 1;
            bool isRight = args.Event.Button == 3;
            bool isMiddle = args.Event.Button == 2;

            if (y > (DisplayedOctaveCount * 12) * _actualNoteHeight) {

                QueueDraw();
                ContentChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (isLeft) {
                // Move cursor to clicked column, but ensure within [0..Count]
                int newPos = Math.Max(0, Math.Min(_widgetMelodyChords.Count, col));
                _cursorIndex = newPos;
                CursorChanged?.Invoke(this, _cursorIndex);

                // S'il n'existe pas encore de colonne à cet endroit, on l’ajoute
                if (col >= _widgetMelodyChords.Count) {
                    while (_widgetMelodyChords.Count <= col) {
                        AddWidgetMelodyChord(new WidgetMelodyChord());
                    }
                }

                // toggle note at this row inside chord
                _ = TogglePitchAt(_cursorIndex, clickedRowFromTop);
                QueueDraw();
                ContentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task TogglePitchAt(int cursorIndex, int clickedRowFromTop) {

            if (cursorIndex < 0 || cursorIndex >= _widgetMelodyChords.Count) {
                return;
            }

            WidgetMelodyChord editedChord = _widgetMelodyChords[cursorIndex];

            int clickedMidiNoteNumber = GetMidiNoteNumberFromClickedRow(clickedRowFromTop);
                        
            WidgetPitch? existingPitch = editedChord.Pitches.FirstOrDefault(
                p => p.MidiNoteNumber == clickedMidiNoteNumber);
            if (existingPitch != null) {
                _ = editedChord.Pitches.Remove(existingPitch);
            } else {
                editedChord.Pitches.Add(new WidgetPitch(clickedMidiNoteNumber));
                await _embeddedMidiSynth.EchoChordAsync(_measureData.GetGMVoiceForStaff(_staffIndex), editedChord.MidiNotes, editedChord.Velocities, _userSettings.MidiInputEchoDurationMs);
            }
        }

        //TODO : a migrer ailleurs
        private int GetMidiCNoteFromOctave(int midiOctave) {

            if (midiOctave == 1) {
                return 0;
            }
            if (midiOctave == 2) {
                return 24;
            }
            if (midiOctave == 3) {
                return 36;
            }
            if (midiOctave == 4) {
                return 48;
            }
            if (midiOctave == 5) {
                return 60;
            }
            if (midiOctave == 6) {
                return 72;
            }
            if (midiOctave == 7) {
                return 84;
            }
            if (midiOctave == 8) {
                return 96;
            }
            if (midiOctave == 9) {
                return 108;
            }
            if (midiOctave == 10) {
                return 120;
            }

            return 0;
        }   

        private void OnKeyPress(object o, KeyPressEventArgs args) {
            // Navigation and edit
            Gdk.Key key = args.Event.Key;
            bool handled = false;

            if (key == Gdk.Key.Left) {
                if (_cursorIndex > 0) {
                    _cursorIndex--;
                }

                handled = true;
            } else if (key == Gdk.Key.Right) {
                if (_cursorIndex < _widgetMelodyChords.Count) {
                    _cursorIndex++;
                }

                //Echo MIDI : jouer la position qui vient d'être parcourue
                _ = EchoChord(_cursorIndex - 1);

                handled = true;
            } else if (key == Gdk.Key.Delete) {
                // delete next element at cursor
                if (_cursorIndex < _widgetMelodyChords.Count) {
                    RemoveWidgetMelodyChord(_cursorIndex);
              
                    ContentChanged?.Invoke(this, EventArgs.Empty);
                }
                handled = true;
            } else if (key == Gdk.Key.BackSpace) {
                // delete previous element
                if (_cursorIndex > 0) {
                    RemoveWidgetMelodyChord(_cursorIndex - 1);
                    _cursorIndex--;
                    ContentChanged?.Invoke(this, EventArgs.Empty);
                }
                handled = true;
            } else if (key == Gdk.Key.Return || key == Gdk.Key.KP_Enter) {
                // insert new empty chord at cursor position
                WidgetMelodyChord sc = new();
                _widgetMelodyChords.Insert(_cursorIndex, sc);
                _cursorIndex++;
                ContentChanged?.Invoke(this, EventArgs.Empty);
                handled = true;
            } else if (key == Gdk.Key.space) {
                // crée nouvelle position uniquement à l’espace
                if (_cursorIndex == _widgetMelodyChords.Count || _widgetMelodyChords.Count == 0) {
                    AddWidgetMelodyChord(new WidgetMelodyChord());
                }
                _cursorIndex = Math.Min(_widgetMelodyChords.Count, _cursorIndex + 1);
                handled = true;
            }

            if (handled) {
                QueueDraw();
                CursorChanged?.Invoke(this, _cursorIndex);
                args.RetVal = true;
            } else {
                args.RetVal = false;
            }
        }

        private async Task EchoChord(int index) {
            if (index < 0 || index >= _widgetMelodyChords.Count) {
                return;
            }

            List<int> notes = new();
            List<int> velocities = new();
            foreach (WidgetPitch widgetPitch in _widgetMelodyChords[index].Pitches) {
                notes.Add(widgetPitch.MidiNoteNumber);
                velocities.Add(_userSettings.MidiInputEchoVeloctiy);
            }

            await _embeddedMidiSynth.EchoChordAsync(_measureData.GetGMVoiceForStaff(_staffIndex), notes, velocities, _userSettings.MidiInputEchoDurationMs);
        }

        // Placeholder for hooking external MIDI input. When MIDI note(s) arrive call this.
        // midiNoteNumbers: list of MIDI note numbers (e.g. 60 = C4)
        public void OnMidiNoteReceived(IEnumerable<int> midiNoteNumbers) {

            // Convert midi to our pitch model and insert or replace at cursor.
            // Very simple behaviour: create/replace chord at cursor with incoming notes, advance cursor.
            WidgetMelodyChord chord = new();
            foreach (int midiNoteNumber in midiNoteNumbers) {
                chord.Pitches.Add(new WidgetPitch(midiNoteNumber));
            }
            Console.WriteLine(chord.ToLogString());

            // insert or replace
            if (_cursorIndex >= 0 && _cursorIndex < _widgetMelodyChords.Count) {
                ReplaceMelodyChord(_cursorIndex, chord);
            } else {
                AddWidgetMelodyChord(chord);
            }
            _cursorIndex = Math.Min(_widgetMelodyChords.Count, _cursorIndex + 1);
            QueueDraw();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void RefreshDisplayedSegment(int displayedSegmentIndex, MeasureData measureData) {
            _segmentIndex = displayedSegmentIndex;
            _measureData = measureData;
            RefreshDisplayedStaff(_staffIndex);
        }

        internal void RefreshDisplayedStaff(int displayedStaffIndex) {
            _staffIndex = displayedStaffIndex;
            _widgetMelodyChords = (List<WidgetMelodyChord>)_measureData.Staffs[_staffIndex].Melody.ToWidgetMelodyChords();
            _cursorIndex = Math.Max(0, Math.Min(_widgetMelodyChords.Count, 0));
            QueueDraw();
        }
    }
}
