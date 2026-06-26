using EZSong.MIDI.Enums;
using NFluidsynth;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//Attention : il faut placer dans les répertoire compilé les fichiers de "fluidsynth-v2.5.1-win10-x64-cpp11.zip" 

namespace EZSong.MIDI {
    public class EmbeddedMidiSynth : IDisposable {

        private readonly NFluidsynth.Settings _settings;
        private readonly Synth _synth;
        private readonly AudioDriver _driver;

        private readonly int _channel = 0; //TODO

        public EmbeddedMidiSynth(string soundFontPath, uint voiceBank, GMVoice gmVoice) {

            _settings = new NFluidsynth.Settings();
            _synth = new Synth(_settings);
            uint sfId = _synth.LoadSoundFont(soundFontPath, true);          
            _synth.ProgramSelect(_channel, sfId, voiceBank, (uint)gmVoice);
            _driver = new AudioDriver(_settings, _synth);
        }

        public void Dispose() {
            _driver?.Dispose();
            _synth?.Dispose();
            _settings?.Dispose();
        }

        /// <summary>
        /// Joue une note MIDI sur le canal 0 (0–15).
        /// </summary>
        public async Task EchoChordAsync(
        IEnumerable<int> noteNumbers,
        IEnumerable<int> velocities,
        int durationMs) {

            int index = 0;
            foreach (int noteNumber in noteNumbers) {
                Console.WriteLine("EchoChordAsync (NoteOn): note=" + noteNumber);
                _synth.NoteOn(_channel, noteNumber, velocities.ToArray()[index]);
                index++;
            }

            await Task.Delay(durationMs);

            foreach (int noteNumber in noteNumbers) {
                Console.WriteLine("EchoChordAsync (NoteOff) : note=" + noteNumber);
                _synth.NoteOff(_channel, noteNumber);
            }
        }

        /// <summary>
        /// Joue une note MIDI sur le canal 0 (0–15).
        /// </summary>
        public void PlayNote(int noteNumber, int velocity = 100) {
            Console.WriteLine("PlayNote : noteNumber=" + noteNumber);
            _synth.NoteOn(_channel, noteNumber, velocity);
        }

        /// <summary>
        /// Arrête une note MIDI sur le canal 0.
        /// </summary>
        public void StopNote(int noteNumber) {
            _synth.NoteOff(_channel, noteNumber);
        }

    }
}
