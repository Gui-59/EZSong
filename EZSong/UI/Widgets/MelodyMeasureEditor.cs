using Cairo;
using Gdk;
using Gtk;
using EZSong.Enums;
using EZSong.MIDI;
using EZSong.MIDI.Enums;
using EZSong.UI.Widgets.WidgetsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EZSong.Model;
using EZSong.Settings;

namespace EZSong.UI.Widgets {
    public class MelodyMeasureEditor : DrawingArea {

        private int _staffIndex;
        private MeasureData _measureData;

        // Model: sequence of chords (positioned sequentially)
        private List<WidgetMelodyChord> _widgetMelodyChords = new();

        private UserSettings _userSettings;

        private SoundFontManager _soundFontManager;

        // Public properties for configuration
        public int MinimumNoteHeight { get; set; } = 14;
        public int NoteWidth { get; set; } = 20; // width reserved per chord slot
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

        private EmbeddedMidiSynth _embeddedMidiSynth; //Pour echo MIDI

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

        public MelodyMeasureEditor() {

            _measureData = new MeasureData();

            _userSettings = new Settings.UserSettings();

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
            HeightRequest = (DisplayedOctaveCount * 12) * MinimumNoteHeight;

            // Input handlers
            ButtonPressEvent += OnButtonPress;
            MotionNotifyEvent += OnMotion;
            KeyPressEvent += OnKeyPress;

            // Start the cursor blink timer
            _cursorTimeoutId = GLib.Timeout.Add(500, OnCursorTimer);
            CanFocus = true;

            _soundFontManager = new();
            _embeddedMidiSynth = new(_soundFontManager.GetCurrentSoundFontPath(), 0, _userSettings.MidiInputDefaultVoice);

        }

        protected override bool OnDrawn(Context cr) {
            // Clear background
            cr.SetSourceRGB(1, 1, 1);
            cr.Paint();

            // Compute geometry
            int rows = DisplayedOctaveCount * 12;
            // draw staff-like grid (simple horizontal lines for notes)
            double totalHeight = rows * MinimumNoteHeight;
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
                double x = i * NoteWidth;
                // subtle vertical tick
                cr.SetSourceRGBA(0.9, 0.9, 0.9, 1);
                cr.LineWidth = 0.5;
                cr.MoveTo(x, -4);
                cr.LineTo(x, totalHeight + 4);
                cr.Stroke();
            }

            // Draw chords
            for (int i = 0; i < _widgetMelodyChords.Count; i++) {
                double center_x = i * NoteWidth + (NoteWidth / 2);
                DrawChordAt(cr, _widgetMelodyChords[i], center_x, 0, totalHeight);
            }


            // Ajustement automatique du spacing si trop de colonnes
            int defaultSpacing = 18;
            int maxVisibleCols = Math.Max(1, (Allocation.Width - (NoteWidth / 2) * 2) / defaultSpacing);
            NoteWidth = _widgetMelodyChords.Count > maxVisibleCols
                ? (Allocation.Width - (NoteWidth / 2) * 2) / _widgetMelodyChords.Count
                : defaultSpacing;

            // Draw cursor (vertical line between chords) at _cursorIndex
            if (_cursorVisible && HasFocus) {
                double x = (NoteWidth / 2) + _cursorIndex * NoteWidth - NoteWidth / 2.0;


                cr.SetSourceRGBA(1, 0, 0, 1); //TODO : couleur du curseur configurable
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
            if (octaveOffset == 0) {
                //Cas de l'octave "centrale"
                //L'octave centrale est différente selon si on est sur une mélodie ou des basses
                if (noteInOctave == 0 || noteInOctave == 2 || noteInOctave == 4 || noteInOctave == 5 || noteInOctave == 7 || noteInOctave == 9 || noteInOctave == 11) {
                    cr.SetSourceRGBA(0.8, 0.8, 1, 1.0); // clair pour les notes naturelles
                } else {
                    cr.SetSourceRGBA(0.6, 0.6, 0.8, 1.0); // plus foncé pour les altérations
                }
            } else {
                if (noteInOctave == 0 || noteInOctave == 2 || noteInOctave == 4 || noteInOctave == 5 || noteInOctave == 7 || noteInOctave == 9 || noteInOctave == 11) {
                    cr.SetSourceRGBA(0.6, 0.6, 0.6, 1.0); // clair pour les notes naturelles
                } else {
                    cr.SetSourceRGBA(0.4, 0.4, 0.4, 1.0); // plus foncé pour les altérations
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

        private void DrawChordAt(Context cr, WidgetMelodyChord chord, double center_x, double areaTop, double areaHeight) {
            int rows = DisplayedOctaveCount * 12;
            double rowHeight = areaHeight / rows;

            // For each pitch draw a circle at appropriate row
            foreach (WidgetPitch p in chord.Pitches) {
                int rowFromTop = GetRowFromTopFromMidiNoteNumber(p.MidiNoteNumber);
                double center_y = areaTop + rowFromTop * rowHeight + rowHeight / 2.0;

                // color
                cr.SetSourceRGBA(0, 0, 0, 1); //TODO ?

                //Draw rectangle
                cr.MoveTo(center_x - (NoteWidth / 2), center_y - (_actualNoteHeight /2)); //Coin Haut Gauche
                cr.LineTo(center_x + (NoteWidth / 2), center_y - (_actualNoteHeight / 2)); //Coin Haut Droite      
                cr.LineTo(center_x + (NoteWidth / 2), center_y + (_actualNoteHeight / 2));  //Coin Bas Droite      
                cr.LineTo(center_x - (NoteWidth / 2), center_y + (_actualNoteHeight / 2));   //Coin Bas Gauche    
                cr.ClosePath();

                // remplissage du diamond
                cr.FillPreserve();
                cr.SetSourceRGBA(0, 0, 0, 1);
                cr.LineWidth = 1.0;
                cr.Stroke();
            }
        }

        private bool OnCursorTimer() {
            _cursorVisible = !_cursorVisible;
            QueueDraw();
            return true;
        }

        // PUBLIC API: load external model into widget
        public void LoadFromModel(int staffIndex, MeasureData measureData, int initialCursor = 0) {
            _staffIndex = staffIndex;
            _measureData = measureData;
            _widgetMelodyChords = (List<WidgetMelodyChord>)measureData.Staffs[staffIndex].Melody.ToWidgetChords();
            _embeddedMidiSynth = new(_soundFontManager.GetCurrentSoundFontPath(), 0, _measureData.SongSettings.GetStaffVoice(0));
            _cursorIndex = Math.Max(0, Math.Min(_widgetMelodyChords.Count, initialCursor));
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
            
            column = (int)Math.Round((x - (NoteWidth / 2)) / NoteWidth);
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
                // remove it
                _ = editedChord.Pitches.Remove(existingPitch);
                // if chord becomes empty, keep it (empty chord allowed) — you can remove if desired
            } else {
                editedChord.Pitches.Add(new WidgetPitch(clickedMidiNoteNumber));
                
                
                await _embeddedMidiSynth.EchoChordAsync(editedChord.MidiNotes, editedChord.Velocities, _userSettings.MidiInputEchoDurationMs);
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
            List<int> notes = new();
            List<int> velocities = new();
            foreach (WidgetPitch widgetPitch in _widgetMelodyChords[index].Pitches) {
                notes.Add(widgetPitch.MidiNoteNumber);
                velocities.Add(_userSettings.MidiInputEchoVeloctiy);
            }

            await _embeddedMidiSynth.EchoChordAsync(notes, velocities, _userSettings.MidiInputEchoDurationMs);

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


    }
}
