using EZSong.Enums;
using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI {

    /// <summary>
    /// TODO
    /// </summary>
    public class SelectableValues {

        public KeySignature DefaultKeySignature;
        public Dictionary<string, string> Tonalities;

        public int DefaultUpperTimeSig;
        public int[] UpperTimeSigs;

        public int DefaultLowerTimeSig;
        public int[] LowerTimeSigs;

        public SelectableValues() {

            DefaultKeySignature = new(NoteStep.C, Alteration.neutral, SongMode.major);

            Tonalities = new();

            // Tonalités majeures (cercle des quintes) : 15 tonalités.
            (NoteStep Note, Alteration Alteration)[] majorTonics = {
                (NoteStep.C, Alteration.neutral),
                (NoteStep.G, Alteration.neutral),
                (NoteStep.D, Alteration.neutral),
                (NoteStep.A, Alteration.neutral),
                (NoteStep.E, Alteration.neutral),
                (NoteStep.B, Alteration.neutral),
                (NoteStep.F, Alteration.sharp),
                (NoteStep.C, Alteration.sharp),
                (NoteStep.F, Alteration.neutral),
                (NoteStep.B, Alteration.flat),
                (NoteStep.E, Alteration.flat),
                (NoteStep.A, Alteration.flat),
                (NoteStep.D, Alteration.flat),
                (NoteStep.G, Alteration.flat),
                (NoteStep.C, Alteration.flat),
            };

            // Tonalités mineures relatives : 15 tonalités.
            (NoteStep Note, Alteration Alteration)[] minorTonics = {
                (NoteStep.A, Alteration.neutral),
                (NoteStep.E, Alteration.neutral),
                (NoteStep.B, Alteration.neutral),
                (NoteStep.F, Alteration.sharp),
                (NoteStep.C, Alteration.sharp),
                (NoteStep.G, Alteration.sharp),
                (NoteStep.D, Alteration.sharp),
                (NoteStep.D, Alteration.neutral),
                (NoteStep.G, Alteration.neutral),
                (NoteStep.C, Alteration.neutral),
                (NoteStep.F, Alteration.neutral),
                (NoteStep.B, Alteration.flat),
                (NoteStep.E, Alteration.flat),
                (NoteStep.A, Alteration.flat),
                (NoteStep.D, Alteration.flat),
            };

            foreach ((NoteStep note, Alteration alteration) in majorTonics) {
                KeySignature key = new(note, alteration, SongMode.major);
                Tonalities[key.ToDropDownId()] = key.ToDropDownLabel();
            }
            foreach ((NoteStep note, Alteration alteration) in minorTonics) {
                KeySignature key = new(note, alteration, SongMode.minor);
                Tonalities[key.ToDropDownId()] = key.ToDropDownLabel();
            }

            DefaultUpperTimeSig = 4;
            UpperTimeSigs = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            DefaultLowerTimeSig = 4;
            LowerTimeSigs = new[] { 1, 2, 4, 8, 16, 32 };
        }
    }
}