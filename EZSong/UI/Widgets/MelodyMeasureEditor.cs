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

    /// <summary>
    /// A Gtk.DrawingArea widget that allows editing a melody measure by adding, removing, and modifying chords and pitches. 
    /// It supports mouse and keyboard interactions, as well as MIDI input for real-time note entry. 
    /// The widget displays a piano-roll-like interface with a configurable number of octaves and note heights.
    /// </summary>
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

        /// <summary>
        /// Adds a new WidgetMelodyChord to the internal list and updates the MeasureData model accordingly.
        /// </summary>
        /// <param name="widgetMelodyChord">A WidgetMelodyChord to add.</param>
        private void AddWidgetMelodyChord(WidgetMelodyChord widgetMelodyChord) {
            _widgetMelodyChords.Add(widgetMelodyChord);
            _measureData.Staffs[_staffIndex].Melody.MelodyChords = _widgetMelodyChords.Select(c => c.ToMelodyChord()).ToList();
        }

        /// <summary>
        /// Replaces the WidgetMelodyChord at the specified cursor index with a new chord and updates the MeasureData model accordingly.
        /// </summary>
        /// <param name="cursorIndex">The index of the chord to replace.</param>
        /// <param name="chord">The new WidgetMelodyChord to insert.</param>
        private void ReplaceMelodyChord(int cursorIndex, WidgetMelodyChord chord) {
            _widgetMelodyChords[cursorIndex] = chord;
            _measureData.Staffs[_staffIndex].Melody.MelodyChords = _widgetMelodyChords.Select(c => c.ToMelodyChord()).ToList();
        }

        /// <summary>
        /// Removes the WidgetMelodyChord at the specified cursor index and updates the MeasureData model accordingly.
        /// </summary>
        /// <param name="cursorIndex">The index of the chord to remove.</param>
        private void RemoveWidgetMelodyChord(int cursorIndex) {
            _widgetMelodyChords.RemoveAt(cursorIndex);
            _measureData.Staffs[_staffIndex].Melody.MelodyChords = _widgetMelodyChords.Select(c => c.ToMelodyChord()).ToList();
        }

        /// <summary>
        /// Constructor for the MelodyMeasureEditor widget. 
        /// Initializes the widget, sets up event handlers, and starts the cursor blink timer.
        /// </summary>
        /// <param name="embeddedMidiSynth">The embedded MIDI synthesizer to use for playback.</param>
        /// <param name="userSettings">The user settings to apply.</param>
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

        /// <summary>
        /// Disposes of the MelodyMeasureEditor widget, stopping the cursor blink timer and releasing any resources.
        /// </summary>
        public new void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            StopCursorTimer();
        }

        /// <summary>
        /// Stops the cursor blink timer if it is running, preventing further cursor visibility toggling.
        /// </summary>
        public void StopCursorTimer() {
            if (_cursorTimeoutId != 0) {
                _ = GLib.Source.Remove(_cursorTimeoutId);
                _cursorTimeoutId = 0;
            }
        }

        /// <summary>
        /// Computes the appropriate font size for the musical note symbol based on the targeted note symbol height in pixels.
        /// </summary>
        /// <param name="cr">The Cairo context used for measuring text extents.</param>
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


        /// <summary>
        /// Renders the melody measure editor, including note grid, chord columns, melody chords, and cursor, using the
        /// specified drawing context.
        /// </summary>
        /// <param name="cr">The drawing context used to render the visual elements.</param>
        /// <returns>True if the drawing operation completes successfully; otherwise, false.</returns>
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


        /// <summary>
        /// Draws a horizontal line representing a pitch area in the melody measure editor, with different colors for natural and altered notes based on the octave offset.
        /// </summary>
        /// <param name="cr">The drawing context used to render the pitch area.</param>
        /// <param name="y">The vertical position of the pitch area.</param>
        /// <param name="noteInOctave">The index of the note within the octave (0-11).</param>
        /// <param name="octaveOffset">The offset of the octave from the central octave.</param>
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

        /// <summary>
        /// Gets the MIDI note number corresponding to the clicked row in the melody measure editor, based on the upper displayed MIDI note number and the row index from the top.
        /// </summary>
        /// <param name="rowFromTop">The index of the row from the top of the melody measure editor.</param>
        /// <returns>The MIDI note number corresponding to the clicked row.</returns>
        private int GetMidiNoteNumberFromClickedRow(int rowFromTop) {
            int upperDisplayedMidiNoteNumber = GetUpperDisplayedMidiNoteNumber();
            return upperDisplayedMidiNoteNumber - (rowFromTop);
        }


        /// <summary>
        /// Gets the row index from the top of the melody measure editor corresponding to a given MIDI note number, based on the upper displayed MIDI note number.
        /// </summary>
        /// <param name="midiNoteNumber">The MIDI note number for which to get the row index.</param>
        /// <returns>The row index from the top of the melody measure editor corresponding to the given MIDI note number.</returns>
        private int GetRowFromTopFromMidiNoteNumber(int midiNoteNumber) {
            int upperDisplayedMidiNoteNumber = GetUpperDisplayedMidiNoteNumber();
            return upperDisplayedMidiNoteNumber - midiNoteNumber;
        }

        /// <summary>
        /// Gets the upper displayed MIDI note number based on the staff's base octave and the number of displayed octaves in the melody measure editor.
        /// </summary>
        /// <returns>The upper displayed MIDI note number.</returns>
        private int GetUpperDisplayedMidiNoteNumber() {
            int baseOctave = _measureData.SongSettings.StaffsSettings.GetStaffBaseOctave(_staffIndex);

            //TODO : Simplifier (pour être plus efficace)
            int octavesAboveOrBelowBaseOctave = (DisplayedOctaveCount - 1) / 2;
            Pitch UpperDisplayedMidiNoteNumberPitch = new(NoteStep.B, Alteration.neutral, baseOctave + octavesAboveOrBelowBaseOctave);
            return UpperDisplayedMidiNoteNumberPitch.ToWidgetPitch().MidiNoteNumber;
        }

        /// <summary>
        /// Draws a melody chord at the specified position in the melody measure editor, rendering each pitch as a note symbol at the appropriate row based on its MIDI note number.
        /// </summary>
        /// <param name="cr">The Cairo context used for drawing.</param>
        /// <param name="chord">The melody chord to draw.</param>
        /// <param name="center_x">The x-coordinate of the center position where the chord should be drawn.</param>
        /// <param name="areaTop">The y-coordinate of the top of the drawing area.</param>
        /// <param name="areaHeight">The height of the drawing area.</param>
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

        /// <summary>
        /// Event handler for the cursor blink timer. Toggles the visibility of the cursor and requests a redraw of the widget.
        /// </summary>
        /// <returns>True to keep the timer running.</returns>
        private bool OnCursorTimer() {
            _cursorVisible = !_cursorVisible;
            QueueDraw();
            return true;
        }

        /// <summary>
        /// Loads the melody measure editor with the specified staff index and measure data, initializing the internal state and notifying listeners of content changes if requested.
        /// </summary>
        /// <param name="staffIndex">The index of the staff to load.</param>
        /// <param name="measureData">The measure data to load.</param>
        /// <param name="notify">Whether to notify listeners of content changes.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void LoadFromModel(int staffIndex, MeasureData measureData, bool notify = true) {

            if (measureData == null) {
                throw new ArgumentNullException(nameof(measureData));
            }

            if (staffIndex < 0 ||
                staffIndex >= measureData.Staffs.Count) {
                throw new InvalidOperationException(
                    $"Impossible d'afficher la portée d'index {staffIndex} " +
                    $"dans la mesure {measureData.Index} : " +
                    $"la mesure ne contient que {measureData.Staffs.Count} portée(s).");
            }

            _staffIndex = staffIndex;
            _measureData = measureData;
            _widgetMelodyChords = (List<WidgetMelodyChord>)measureData.Staffs[staffIndex].Melody.ToWidgetMelodyChords();
            _cursorIndex = 0;
            QueueDraw();
            if (notify) {
                ContentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Copies a WidgetMelodyChord, creating a new instance with the same pitches. This is used to export the internal state without exposing references to the original objects.
        /// </summary>
        /// <param name="s">The WidgetMelodyChord to copy.</param>
        /// <returns>A new WidgetMelodyChord instance with the same pitches as the input.</returns>
        private WidgetMelodyChord DeepCopyChord(WidgetMelodyChord s) {
            WidgetMelodyChord c = new();
            foreach (WidgetPitch p in s.Pitches) {
                c.Pitches.Add(new WidgetPitch(p.MidiNoteNumber));
            }
            return c;
        }

        /// <summary>
        /// Exports the current state of the melody measure editor to a list of WidgetMelodyChord instances, creating deep copies to avoid exposing internal references.
        /// </summary>
        /// <returns>A list of WidgetMelodyChord instances representing the current state of the editor.</returns>
        public List<WidgetMelodyChord> ExportToModel() {
            //todo : éviter le Select (lent)
            return _widgetMelodyChords.Select(DeepCopyChord).ToList();
        }


        /// <summary>
        /// Event handler for mouse motion events. 
        /// Updates the hover column and row based on the mouse position, 
        /// allowing for visual feedback when hovering over different parts of the melody measure editor.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void OnMotion(object o, MotionNotifyEventArgs args) {
            double ax = args.Event.X;
            double ay = args.Event.Y;
            HitTest(ax, ay, out int col, out int row);
            _hoverColumn = col;
            _hoverRow = row;
            // not doing anything else for now
        }

        /// <summary>
        /// Tests the given x and y coordinates to determine which column and row in the melody measure editor they correspond to.
        /// </summary>
        /// <param name="x">The x-coordinate to test.</param>
        /// <param name="y">The y-coordinate to test.</param>
        /// <param name="column">The resulting column index.</param>
        /// <param name="row">The resulting row index.</param>
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


        /// <summary>
        /// Event handler for mouse button press events.
        /// </summary>
        /// <param name="o">The source of the event.</param>
        /// <param name="args">The event data.</param>
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


        /// <summary>
        /// Toggles the presence of a pitch at the specified cursor index and clicked row in the melody measure editor.
        /// </summary>
        /// <param name="cursorIndex">The index of the cursor in the melody measure editor.</param>
        /// <param name="clickedRowFromTop">The row index clicked from the top.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Event handler for key press events. 
        /// Handles navigation, editing, and insertion of chords and pitches based on the pressed key.
        /// </summary>
        /// <param name="o">The source of the event.</param>
        /// <param name="args">The event data.</param>
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

        /// <summary>
        /// Echoes the chord at the specified index using the embedded MIDI synthesizer, 
        /// playing the notes with the configured velocity and duration.
        /// </summary>
        /// <param name="index">The index of the chord to echo.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Event handler for receiving MIDI note numbers. Converts the incoming MIDI notes to WidgetPitch instances, creates a new WidgetMelodyChord, and inserts or replaces it at the current cursor position. 
        /// Advances the cursor after insertion and triggers a redraw and content change notification.
        /// </summary>
        /// <param name="midiNoteNumbers">The incoming MIDI note numbers.</param>
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


        /// <summary>
        /// Refreshes the displayed segment of the melody measure editor by updating the internal state with the specified displayed segment index and measure data.
        /// </summary>
        /// <param name="displayedSegmentIndex">The index of the segment to display.</param>
        /// <param name="measureData">The measure data to display.</param>
        internal void RefreshDisplayedSegment(int displayedSegmentIndex, MeasureData measureData) {
            _segmentIndex = displayedSegmentIndex;
            _measureData = measureData;
            RefreshDisplayedStaff(_staffIndex);
        }

        /// <summary>
        /// Refreshes the displayed staff in the melody measure editor by updating the internal state with the specified displayed staff index and regenerating the list of WidgetMelodyChord instances from the measure data.
        /// </summary>
        /// <param name="displayedStaffIndex">The index of the staff to display.</param>
        internal void RefreshDisplayedStaff(int displayedStaffIndex) {
            _staffIndex = displayedStaffIndex;
            _widgetMelodyChords = (List<WidgetMelodyChord>)_measureData.Staffs[_staffIndex].Melody.ToWidgetMelodyChords();
            _cursorIndex = 0;
            QueueDraw();
        }
    }
}
