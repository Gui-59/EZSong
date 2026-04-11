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

namespace EZSong.UI.Widgets {
    public class MelodyMeasureEditor : DrawingArea {

        const int _notesPerOctave = 7;   // C D E F G A B

        private MIDI.UserSettings _midiUserSettings;

        // Public properties for configuration
        public int OctaveCount { get; set; } = 3; // number of octaves to display
        public int BaseOctave { get; set; } = 4;  // visual reference (middle C octave)
        public int NoteDiamondRadius { get; set; } = 7; //ZOOM / SCALE
        public int HorizontalSpacing { get; set; } = 26; // width reserved per chord slot
        public int CadenceHeight { get; set; } = 20;

        private int _topMargin;
        private int _leftMargin;

        // Model: sequence of chords (positioned sequentially)
        private List<WidgetMelodyChord> _melodyChords = new();

        // Cursor index: insertion point between chords (0..Count)
        private int _cursorIndex = 0;

        // Cursor blink
        private bool _cursorVisible = true;
        private uint _cursorTimeoutId;

        // Colors
        private RGBA _colorNatural = new() { Red = 0, Green = 0, Blue = 0, Alpha = 1 }; // black
        private RGBA _colorSharp = new() { Red = 0.0, Green = 0.0, Blue = 0.6, Alpha = 1 }; // blue
        private RGBA _colorFlat = new() { Red = 0.6, Green = 0.0, Blue = 0.0, Alpha = 1 }; // red
        private RGBA _colorCursor = new() { Red = 0.2, Green = 0.6, Blue = 0.2, Alpha = 1 }; // green

        // Interaction state
        private int _hoverColumn = -1;
        private int _hoverRow = -1;

        // Events
        public event EventHandler? ContentChanged; // raised when chords/cadency change
        public event EventHandler<int>? CursorChanged; // cursor index changed

        // Map note index to label (C D E F G A B)
        private static readonly string[] _noteNames = new[] { "C", "D", "E", "F", "G", "A", "B" };

        private EmbeddedMidiSynth _embeddedMidiSynth; //Pour echo MIDI

        public MelodyMeasureEditor() {

            _midiUserSettings = new MIDI.UserSettings();

            _topMargin = NoteDiamondRadius*2;
            _leftMargin = NoteDiamondRadius * 2;

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
            HeightRequest = _topMargin + OctaveCount * _notesPerOctave * NoteDiamondRadius * 2 + CadenceHeight + 8;

            // Input handlers
            ButtonPressEvent += OnButtonPress;
            MotionNotifyEvent += OnMotion;
            KeyPressEvent += OnKeyPress;

            // Start the cursor blink timer
            _cursorTimeoutId = GLib.Timeout.Add(500, OnCursorTimer);
            CanFocus = true;

            SoundFontManager soundFontManager = new();
            _embeddedMidiSynth = new(soundFontManager.GetCurrentSoundFontPath(), 0, _midiUserSettings.MidiInputEchoVoice);

        }

        protected override bool OnDrawn(Context cr) {
            // Clear background
            cr.SetSourceRGB(1, 1, 1);
            cr.Paint();

            // Compute geometry
            int rows = OctaveCount * _notesPerOctave;
            // draw staff-like grid (simple horizontal lines for notes)
            double noteAreaTop = _topMargin;
            double noteAreaHeight = rows * (NoteDiamondRadius * 2);
            double rowHeight = noteAreaHeight / rows;

            int highestbOctaveOffset = 0 + (int)Math.Floor((double)(OctaveCount/2)) + 1 + 1; //Octave du SI le plus aigu affichable

            int octaveOffset = highestbOctaveOffset;

            // Draw horizontal separators for each visible pitch
            for (int r = 0; r < rows; r++) {
                
                double y = noteAreaTop + r * rowHeight + rowHeight / 2.0;
                int noteIndex = rows - 1 - r; // top row = highest pitch
                int noteInOctave = noteIndex % _notesPerOctave;
                if (noteInOctave == 6) {
                    //A chaque fois qu'on retombe sur un SI, on descend d'une octa
                    octaveOffset--;
                }
                DrawPitchLine(cr, y, noteInOctave, octaveOffset);
                
            }

            // Draw vertical chord columns and existing chords
            for (int i = 0; i <= _melodyChords.Count; i++) {
                double x = _leftMargin + i * HorizontalSpacing;
                // subtle vertical tick
                cr.SetSourceRGBA(0.9, 0.9, 0.9, 1);
                cr.LineWidth = 1.0;
                cr.MoveTo(x, noteAreaTop - 4);
                cr.LineTo(x, noteAreaTop + noteAreaHeight + 4);
                cr.Stroke();
            }

            // Draw chords (diamonds)
            for (int i = 0; i < _melodyChords.Count; i++) {
                double x = _leftMargin + i * HorizontalSpacing;
                DrawChordAt(cr, _melodyChords[i], x, noteAreaTop, noteAreaHeight);
            }

            // Ajustement automatique du spacing si trop de colonnes
            int defaultSpacing = 28;
            int maxVisibleCols = Math.Max(1, (Allocation.Width - _leftMargin * 2) / defaultSpacing);
            HorizontalSpacing = _melodyChords.Count > maxVisibleCols
                ? (Allocation.Width - _leftMargin * 2) / _melodyChords.Count
                : defaultSpacing;

            // Draw cursor (vertical line between chords) at _cursorIndex
            if (_cursorVisible && HasFocus) {
                double x = _leftMargin + _cursorIndex * HorizontalSpacing - HorizontalSpacing / 2.0;
                if (x < _leftMargin) {
                    x = _leftMargin;
                }

                cr.SetSourceRGBA(_colorCursor.Red, _colorCursor.Green, _colorCursor.Blue, _colorCursor.Alpha);
                cr.LineWidth = 2.0;
                cr.MoveTo(x, noteAreaTop - 6);
                cr.LineTo(x, noteAreaTop + noteAreaHeight + 6);
                cr.Stroke();
            }

            // Draw cadence area below
            double cadenceTop = noteAreaTop + noteAreaHeight + 6;
            DrawCadence(cr, cadenceTop, Allocation.Width, CadenceHeight);

            return true;
        }

        private void DrawPitchLine(Context cr, double y, int noteInOctave, int octaveOffset) {
            /*
             * On se considère en clé de SOL. Et on represente une portée standard
             * 
             * On doit donc mettre en traits pleins (ligne de portée) :
             * E	2	1
             * G	4	1
             * B	6	1
             * D	1	2
             * F	3	2
             * 
             * Et en pointillés
             * D	1	0
             * F	3	0
             * A	5	0
             * C	0	1
             * A	5	2
             * C	0	3
             * 
             * Pour tout le reste : pas de ligne matérialisée
             */

            if (
                (noteInOctave == 2 && octaveOffset == 1)
                || (noteInOctave == 4 && octaveOffset == 1)
                || (noteInOctave == 6 && octaveOffset == 1)
                || (noteInOctave == 1 && octaveOffset == 2)
                || (noteInOctave == 3 && octaveOffset == 2)
             ) {
                //Ligne pleine
                cr.LineWidth = 2.0;
                cr.SetSourceRGBA(0, 0, 0, 1.0);
                cr.SetDash(Array.Empty<double>(), 0); // repasse en ligne pleine
                cr.MoveTo(0, y);
                cr.LineTo(Allocation.Width, y);
                cr.Stroke();
            } else if (
                (noteInOctave == 1 && octaveOffset == 0)
                || (noteInOctave == 3 && octaveOffset == 0)
                || (noteInOctave == 5 && octaveOffset == 0)
                || (noteInOctave == 0 && octaveOffset == 1)
                || (noteInOctave == 5 && octaveOffset == 2)
                || (noteInOctave == 0 && octaveOffset == 3)
            ) {
                //Ligne pointillée
                cr.LineWidth = 2.0;
                cr.SetSourceRGBA(0, 0, 0, 1.0);
                cr.SetDash(new double[] { 8.0, 4.0 }, 0);
                cr.MoveTo(0, y);
                cr.LineTo(Allocation.Width, y);
                cr.Stroke();
            } else {
                //Pas de ligne
            }

        }

        private void DrawChordAt(Context cr, WidgetMelodyChord chord, double x, double areaTop, double areaHeight) {
            int rows = OctaveCount * _notesPerOctave;
            double rowHeight = areaHeight / rows;

            // For each pitch draw a circle at appropriate row
            foreach (WidgetPitch p in chord.Pitches) {
                // compute global pitch index
                int globalIndex = p.OctaveOffset * _notesPerOctave + p.NoteIndex; // 0..rows-1 with 0 lowest
                // we display top = highest; compute row index from top:
                int rowFromTop = OctaveCount * _notesPerOctave - 1 - globalIndex;
                double y = areaTop + rowFromTop * rowHeight + rowHeight / 2.0;

                // slight horizontal offset for accidentals:
                double xOff = 0;
                double yOff = 0;
                if (p.Alteration == Alteration.sharp) {
                    xOff = 4;
                    yOff = -4;
                } else if (p.Alteration == Alteration.flat) {
                    xOff = -4;
                    yOff = 4;
                }

                double cx = x + xOff;
                double cy = y + yOff;

                // choose color
                if (p.Alteration == Alteration.neutral) {
                    cr.SetSourceRGBA(_colorNatural.Red, _colorNatural.Green, _colorNatural.Blue, _colorNatural.Alpha);
                } else if (p.Alteration == Alteration.sharp) {
                    cr.SetSourceRGBA(_colorSharp.Red, _colorSharp.Green, _colorSharp.Blue, _colorSharp.Alpha);
                } else {
                    cr.SetSourceRGBA(_colorFlat.Red, _colorFlat.Green, _colorFlat.Blue, _colorFlat.Alpha);
                }

                // draw diamond (square rotated 45°)
                double r = NoteDiamondRadius; // demi-diagonale
                cr.MoveTo(cx, cy - r);       // haut
                cr.LineTo(cx + r, cy);       // droite
                cr.LineTo(cx, cy + r);       // bas
                cr.LineTo(cx - r, cy);       // gauche
                cr.ClosePath();

                // remplissage du diamond
                cr.FillPreserve();
                cr.SetSourceRGBA(0, 0, 0, 1);
                cr.LineWidth = 1.0;
                cr.Stroke();

                //Lettre de la note (hors alteration) incrustée dans le losange

                string label = "";
                switch (p.NoteIndex) {
                    case 0:
                        label = "C";
                        break;
                    case 1:
                        label = "D";
                        break;
                    case 2:
                        label = "E";
                        break;
                    case 3:
                        label = "F";
                        break;
                    case 4:
                        label = "G";
                        break;
                    case 5:
                        label = "A";
                        break;
                    case 6:
                        label = "B";
                        break;
                }

                cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
                cr.SetFontSize(r * 1.5); // un peu proportionné à la taille du losange
                cr.SetSourceRGBA(1, 1, 1, 1); // blanc
                TextExtents te = cr.TextExtents(label);
                double tx = cx - (te.Width / 2 + te.XBearing);
                double ty = cy - (te.Height / 2 + te.YBearing);
                cr.MoveTo(tx, ty);
                cr.ShowText(label);
            }
        }

        private void DrawCadence(Context cr, double top, double width, double height) {

            // background
            cr.SetSourceRGBA(0.98, 0.98, 0.98, 1);
            cr.Rectangle(0, top, width, height);
            cr.Fill();
        }

        private bool OnCursorTimer() {
            _cursorVisible = !_cursorVisible;
            QueueDraw();
            return true;
        }

        // PUBLIC API: load external model into widget
        public void LoadFromModel(IEnumerable<WidgetMelodyChord> chords, int initialCursor = 0) {
            //todo : éviter le Select (lent)
            _melodyChords = chords != null ? new List<WidgetMelodyChord>(chords.Select(c => DeepCopyChord(c))) : new List<WidgetMelodyChord>();            _cursorIndex = Math.Max(0, Math.Min(_melodyChords.Count, initialCursor));
            QueueDraw();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private WidgetMelodyChord DeepCopyChord(WidgetMelodyChord s) {
            WidgetMelodyChord c = new();
            foreach (WidgetPitch p in s.Pitches) {
                c.Pitches.Add(new WidgetPitch(p.NoteIndex, p.OctaveOffset, p.Alteration));
            }

            return c;
        }

        public List<WidgetMelodyChord> ExportToModel() {
            //todo : éviter le Select (lent)
            return _melodyChords.Select(DeepCopyChord).ToList();
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
            int rows = OctaveCount * _notesPerOctave;
            double noteAreaTop = _topMargin;
            double noteAreaHeight = rows * (NoteDiamondRadius * 2);
            double rowHeight = noteAreaHeight / rows;
            column = (int)Math.Round((x - _leftMargin) / HorizontalSpacing);
            if (column < 0) {
                column = 0;
            }

            if (column > Math.Max(0, _melodyChords.Count)) {
                column = _melodyChords.Count;
            }
            // compute row from top
            if (y < noteAreaTop) {
                row = -1;
                return;
            }
            double rel = y - noteAreaTop;
            int r = (int)Math.Round(rel / rowHeight);
            if (r < 0) {
                r = 0;
            }

            if (r >= rows) {
                r = rows - 1;
            }

            row = r;
        }

        private void OnButtonPress(object o, ButtonPressEventArgs args) {
            GrabFocus(); // ensure keyboard focus

            double x = args.Event.X;
            double y = args.Event.Y;
            HitTest(x, y, out int col, out int row);

            bool isLeft = args.Event.Button == 1;
            bool isRight = args.Event.Button == 3;
            bool isMiddle = args.Event.Button == 2;

            if (y > _topMargin + OctaveCount * _notesPerOctave * NoteDiamondRadius * 2) {

                QueueDraw();
                ContentChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (isLeft) {
                // Move cursor to clicked column, but ensure within [0..Count]
                int newPos = Math.Max(0, Math.Min(_melodyChords.Count, col));
                _cursorIndex = newPos;
                CursorChanged?.Invoke(this, _cursorIndex);

                // S'il n'existe pas encore de colonne à cet endroit, on l’ajoute
                if (col >= _melodyChords.Count) {
                    while (_melodyChords.Count <= col) {
                        _melodyChords.Add(new WidgetMelodyChord());
                    }
                }

                // toggle note at this row inside chord
                _ = TogglePitchAt(_cursorIndex, row);
                QueueDraw();
                ContentChanged?.Invoke(this, EventArgs.Empty);
            } else if (isRight) {
                // Right click: if there's a chord at column, toggle or cycle alteration on the pitch nearest row
                int idx = Math.Min(_melodyChords.Count - 1, Math.Max(0, col));
                if (idx >= 0 && idx < _melodyChords.Count) {
                    // compute which pitch is clicked and cycle its alteration if present, otherwise add with flat
                    _ = CycleAlterAt(idx, row);
                    QueueDraw();
                    ContentChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private async Task TogglePitchAt(int chordIndex, int rowFromTop) {
            if (chordIndex < 0 || chordIndex >= _melodyChords.Count) {
                return;
            }

            // calculate SimplePitch coords
            int rows = OctaveCount * _notesPerOctave;
            int globalIndex = rows - 1 - rowFromTop; // 0 = lowest
            int octaveOffset = globalIndex / _notesPerOctave;
            int noteIndex = globalIndex % _notesPerOctave;

            WidgetMelodyChord chord = _melodyChords[chordIndex];
            WidgetPitch? existing = chord.Pitches.FirstOrDefault(
                p => p.NoteIndex == noteIndex && p.OctaveOffset == octaveOffset && p.Alteration == Alteration.neutral);
            if (existing != null) {
                // remove it
                _ = chord.Pitches.Remove(existing);
                // if chord becomes empty, keep it (empty chord allowed) — you can remove if desired
            } else {
                chord.Pitches.Add(new WidgetPitch(noteIndex, octaveOffset, Alteration.neutral));
                
                int noteNumber = GetNoteNumber(NoteNumberInFullOctaveFromIndexInOctave(noteIndex), 5 + octaveOffset, Alteration.neutral);
                await _embeddedMidiSynth.EchoChordAsync(new[] { noteNumber }, new[] { _midiUserSettings.MidiInputEchoVeloctiy }, _midiUserSettings.MidiInputEchoDurationMs);
            }
        }

        private int GetNoteNumber(int noteNumberInFullOctave, int midiOctave, Alteration alteration) {
            int ret = GetMidiCNoteFromOctave(midiOctave) + (noteNumberInFullOctave);

            switch (alteration) {
                case Alteration.flatflat:
                    ret -= 2;
                    break;
                case Alteration.flat:
                    ret -= 1;
                    break;
                case Alteration.sharp:
                    ret += 1;
                    break;
                case Alteration.sharpsharp:
                    ret += 2;
                    break;
            }

            return ret;
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

        //TODO : a migrer ailleurs
        //Retourne la position de la note de 1 à 12 dans l'octave
        // A partir de sa position dans l'octave
        private int NoteNumberInFullOctaveFromIndexInOctave(int noteIndexInOctave) {

            if (noteIndexInOctave==0) {
                return 0; //C
            }

            if (noteIndexInOctave==1) {
                return 2; //D 
            }

            if (noteIndexInOctave==2) {
                return 4; //E 
            }

            if (noteIndexInOctave==3) {
                return 5; //F 
            }

            if (noteIndexInOctave==4) {
                return 7; //G 
            }

            if (noteIndexInOctave==5) {
                return 9; //A 
            }

            if (noteIndexInOctave==6) {
                return 11; //B                                                                          
            }

            throw new Exception("NoteIndexInOctave invalide : " + noteIndexInOctave);
        }

        private async Task CycleAlterAt(int chordIndex, int rowFromTop) {
            if (chordIndex < 0 || chordIndex >= _melodyChords.Count) {
                return;
            }

            int rows = OctaveCount * _notesPerOctave;
            int globalIndex = rows - 1 - rowFromTop;
            int octaveOffset = globalIndex / _notesPerOctave;
            int noteIndex = globalIndex % _notesPerOctave;

            WidgetMelodyChord chord = _melodyChords[chordIndex];
            WidgetPitch? existing = chord.Pitches.FirstOrDefault(p => p.NoteIndex == noteIndex && p.OctaveOffset == octaveOffset);
            if (existing != null) {
                // cycle alteration
                if (existing.Alteration == Alteration.neutral) {
                    existing.Alteration = Alteration.sharp;
                } else if (existing.Alteration == Alteration.sharp) {
                    existing.Alteration = Alteration.flat;
                } else {
                    existing.Alteration = Alteration.neutral;
                }

                int noteNumber = GetNoteNumber(NoteNumberInFullOctaveFromIndexInOctave(existing.NoteIndex), 5 + existing.OctaveOffset, existing.Alteration);
                await _embeddedMidiSynth.EchoChordAsync(new[] { noteNumber }, new[] { _midiUserSettings.MidiInputEchoVeloctiy }, _midiUserSettings.MidiInputEchoDurationMs);
            } else {
                // add with flat by default
                chord.Pitches.Add(new WidgetPitch(noteIndex, octaveOffset, Alteration.flat));

                int noteNumber = GetNoteNumber(NoteNumberInFullOctaveFromIndexInOctave(noteIndex), 5 + octaveOffset, Alteration.flat);
                await _embeddedMidiSynth.EchoChordAsync(new[] { noteNumber }, new[] { _midiUserSettings.MidiInputEchoVeloctiy }, _midiUserSettings.MidiInputEchoDurationMs);
                await _embeddedMidiSynth.EchoChordAsync(new[] { noteNumber }, new[] { _midiUserSettings.MidiInputEchoVeloctiy }, _midiUserSettings.MidiInputEchoDurationMs);
            }
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
                if (_cursorIndex < _melodyChords.Count) {
                    _cursorIndex++;
                }

                //Echo MIDI : jouer la position qui vient d'être parcourue
                _ = EchoChord(_cursorIndex - 1);

                handled = true;
            } else if (key == Gdk.Key.Delete) {
                // delete next element at cursor
                if (_cursorIndex < _melodyChords.Count) {
                    _melodyChords.RemoveAt(_cursorIndex);
                    ContentChanged?.Invoke(this, EventArgs.Empty);
                }
                handled = true;
            } else if (key == Gdk.Key.BackSpace) {
                // delete previous element
                if (_cursorIndex > 0) {
                    _melodyChords.RemoveAt(_cursorIndex - 1);
                    _cursorIndex--;
                    ContentChanged?.Invoke(this, EventArgs.Empty);
                }
                handled = true;
            } else if (key == Gdk.Key.Return || key == Gdk.Key.KP_Enter) {
                // insert new empty chord at cursor position
                WidgetMelodyChord sc = new();
                _melodyChords.Insert(_cursorIndex, sc);
                _cursorIndex++;
                ContentChanged?.Invoke(this, EventArgs.Empty);
                handled = true;
            } else if (key == Gdk.Key.space) {
                // crée nouvelle position uniquement à l’espace
                if (_cursorIndex == _melodyChords.Count || _melodyChords.Count == 0) {
                    _melodyChords.Add(new WidgetMelodyChord());
                }
                _cursorIndex = Math.Min(_melodyChords.Count, _cursorIndex + 1);
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
            foreach (WidgetPitch pitch in _melodyChords[index].Pitches) {
                notes.Add(GetNoteNumber(NoteNumberInFullOctaveFromIndexInOctave(pitch.NoteIndex), 5 + pitch.OctaveOffset, pitch.Alteration));
                velocities.Add(_midiUserSettings.MidiInputEchoVeloctiy);
            }

            await _embeddedMidiSynth.EchoChordAsync(notes, velocities, _midiUserSettings.MidiInputEchoDurationMs);

        }

        // Placeholder for hooking external MIDI input. When MIDI note(s) arrive call this.
        // midiNoteNumbers: list of MIDI note numbers (e.g. 60 = C4)
        public void OnMidiNoteReceived(IEnumerable<int> midiNoteNumbers) {

            // Convert midi to our pitch model and insert or replace at cursor.
            // Very simple behaviour: create/replace chord at cursor with incoming notes, advance cursor.
            WidgetMelodyChord chord = new();
            foreach (int midi in midiNoteNumbers) {
                int midiC0 = 12; // MIDI 12 = C0
                int semitone = midi - midiC0; // semitone offset from C0
                // convert to our 7-note per octave index: approximate mapping (C D E F G A B)
                // This is a simplification: enharmonics and accidentals require more mapping.
                int octave = semitone / 12;
                int pc = semitone % 12;
                // map chromatic pitch class to noteIndex and alteration
                // chromatic to natural mapping (C=0,D=2,E=4,F=5,G=7,A=9,B=11)
                // find nearest natural:
                int noteIndex = 0;
                Alteration alt = Alteration.neutral;
                switch (pc) {
                    case 0:
                        noteIndex = 0;
                        alt = Alteration.neutral;
                        break; // C
                    case 1:
                        noteIndex = 0;
                        alt = Alteration.sharp;
                        break; // C#
                    case 2:
                        noteIndex = 1;
                        alt = Alteration.neutral;
                        break; // D
                    case 3:
                        noteIndex = 1;
                        alt = Alteration.sharp;
                        break; // D#
                    case 4:
                        noteIndex = 2;
                        alt = Alteration.neutral;
                        break; // E
                    case 5:
                        noteIndex = 3;
                        alt = Alteration.neutral;
                        break; // F
                    case 6:
                        noteIndex = 3;
                        alt = Alteration.sharp;
                        break; // F#
                    case 7:
                        noteIndex = 4;
                        alt = Alteration.neutral;
                        break; // G
                    case 8:
                        noteIndex = 4;
                        alt = Alteration.sharp;
                        break; // G#
                    case 9:
                        noteIndex = 5;
                        alt = Alteration.neutral;
                        break; // A
                    case 10:
                        noteIndex = 5;
                        alt = Alteration.sharp;
                        break; // A#
                    case 11:
                        noteIndex = 6;
                        alt = Alteration.neutral;
                        break; // B
                }

                int baseOctave = BaseOctave;
                int octaveOffset = octave - baseOctave;
                chord.Pitches.Add(new WidgetPitch(noteIndex, octaveOffset, alt));
            }
            Console.WriteLine(chord.ToLogString());



            // insert or replace
            if (_cursorIndex >= 0 && _cursorIndex < _melodyChords.Count) {
                _melodyChords[_cursorIndex] = chord;
            } else {
                _melodyChords.Add(chord);
            }
            _cursorIndex = Math.Min(_melodyChords.Count, _cursorIndex + 1);
            QueueDraw();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
