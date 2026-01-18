using GLib;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using EZSong.MIDI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EZSong.MIDI {

    /*
     * Gère la capture en temps réel des notes jouées sur un clavier MIDI.
     */
    public class MidiInputManager : IDisposable {
        private InputDevice? _inputDevice;
        private readonly List<int> _currentlyPressedNotes = new();
        private readonly object _lock = new();
        private System.Timers.Timer? _groupingTimer;

        /// <summary>
        /// Délai d’attente avant de considérer qu’un accord est complet (en ms).
        /// </summary>
        public int GroupingDelayMs { get; set; } = 40;

        /// <summary>
        /// Déclenché lorsqu’une ou plusieurs notes (accord) sont jouées.
        /// </summary>
        public event Action<IReadOnlyList<int>>? NotesPlayed;

        private EmbeddedMidiSynth _embeddedMidiSynth; //Pour echo MIDI

        public MidiInputManager() {
            SoundFontManager soundFontManager = new();
            _embeddedMidiSynth = new(soundFontManager.GetCurrentSoundFontPath(), 0, GMVoice.PIANO_ElectricPiano1);
        }

        public static IEnumerable<string> GetAvailableDevices() {
            return InputDevice.GetAll().Select(d => d.Name);
        }

        public bool Open(string deviceName) {
            Close();

            InputDevice? device = InputDevice.GetAll().FirstOrDefault(d => d.Name == deviceName);
            if (device == null) {
                return false;
            }

            _inputDevice = device;
            _inputDevice.EventReceived += OnMidiEventReceived;
            _inputDevice.StartEventsListening();
            return true;
        }

        public void Close() {
            if (_inputDevice != null) {
                _inputDevice.EventReceived -= OnMidiEventReceived;
                _inputDevice.Dispose();
                _inputDevice = null;
            }

            lock (_lock) {
                _currentlyPressedNotes.Clear();
            }

            _groupingTimer?.Stop();
            _groupingTimer?.Dispose();
            _groupingTimer = null;
        }

        private void OnMidiEventReceived(object? sender, MidiEventReceivedEventArgs e) {
            if (e.Event is NoteOnEvent noteOn && noteOn.Velocity > 0) {
                lock (_lock) {
                    if (!_currentlyPressedNotes.Contains(noteOn.NoteNumber)) {
                        _currentlyPressedNotes.Add(noteOn.NoteNumber);
                        //Echo MIDI
                        _embeddedMidiSynth.PlayNote(noteOn.NoteNumber, noteOn.Velocity);
                    }
                }

                // Redémarre le timer à chaque nouvelle note
                ResetGroupingTimer();
            } else if (e.Event is NoteOffEvent noteOff) {
                lock (_lock) {
                    _ = _currentlyPressedNotes.Remove(noteOff.NoteNumber);
                    //Arrêt Echo MIDI
                    _embeddedMidiSynth.StopNote(noteOff.NoteNumber);
                }
            } else if (e.Event is NoteOnEvent noteZero && noteZero.Velocity == 0) {
                lock (_lock) {
                    _ = _currentlyPressedNotes.Remove(noteZero.NoteNumber);
                    //Arrêt Echo MIDI
                    _embeddedMidiSynth.StopNote(noteZero.NoteNumber);
                }
            }
        }

        private void ResetGroupingTimer() {
            if (_groupingTimer == null) {
                _groupingTimer = new System.Timers.Timer(GroupingDelayMs);
                _groupingTimer.AutoReset = false;
                _groupingTimer.Elapsed += (_, _) => EmitGroupedNotes();
            }

            _groupingTimer.Stop();
            _groupingTimer.Start();
        }

        private void EmitGroupedNotes() {
            List<int> snapshot;
            lock (_lock) {
                snapshot = _currentlyPressedNotes.ToList();
            }

            if (snapshot.Count > 0) {
                NotesPlayed?.Invoke(snapshot);
            }
        }

        public void Dispose() {
            Close();
        }
    }
}
