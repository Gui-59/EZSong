using EZSong.Enums;
using EZSong.EnumsStringifier;
using EZSong.Exporting.Lilypond;
using EZSong.Settings;
using EZSong.UI.Widgets.WidgetsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    public class Pitch : IEquatable<Pitch>, IComparable<Pitch> {

        public NoteStep Note {
            get;
            set;
        }

        public Alteration Alteration {
            get;
            set;
        }

        public int MidiOctave {
            get;
            set;
        }
        public static Pitch MinValue {
            get;
            internal set;
        } = new Pitch(NoteStep.C, Alteration.neutral, 0);
        public static Pitch MaxValue {
            get;
            internal set;
        } = new Pitch(NoteStep.G, Alteration.neutral, 10);

        private readonly ILilypondConverter _lilypondConverter;

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public Pitch() {
            Note = NoteStep.C;
            Alteration = Alteration.neutral;
            MidiOctave = Constants.MelodyBaseOctave; //Par défaut
            _lilypondConverter = new LilypondConverter();
        }

        public Pitch(NoteStep note, Alteration alteration, int midiOctave) {

            if (midiOctave < 0 || midiOctave > 10) {
                throw new ArgumentOutOfRangeException("MidiOctave must be between 0 and 10.");
            }

            _lilypondConverter = new LilypondConverter();

            Note = note;
            Alteration = alteration;
            MidiOctave = midiOctave;
        }

        public string ToLilyPondString(int baseOctave) {

            string lilyPondString = string.Empty;

            lilyPondString += _lilypondConverter.NoteStepToLilyPondString(Note);
            lilyPondString += _lilypondConverter.AlterationToLilyPondString(Alteration);

            int sheetOctaveOffset = MidiOctave - baseOctave;

            if (sheetOctaveOffset > 0) {

                for (int i = 0; i < sheetOctaveOffset; i++) {
                    lilyPondString += "'";
                }
                
            } else if (sheetOctaveOffset < 0) {

                for (int i = 0; i < -1 * sheetOctaveOffset; i++) {
                    lilyPondString += ",";
                }

            }

            return lilyPondString;
        }

        internal WidgetPitch ToWidgetPitch() {
            return new WidgetPitch(ToMidiNoteNumber());
        }

        public int ToMidiNoteNumber() {
            return (MidiOctave * 12) + ((int)Note) + ((int)Alteration);
        }

        internal PitchDto ToDto() {
            return new PitchDto(Note, Alteration, MidiOctave);
        }

        public static Pitch FromMidi(int midi) {
            int pc = midi % 12;
            NoteStep note;
            Alteration alteration;
            (note, alteration) = pc switch {
                0 => (NoteStep.C, Alteration.neutral),
                1 => (NoteStep.C, Alteration.sharp),
                2 => (NoteStep.D, Alteration.neutral),
                3 => (NoteStep.D, Alteration.sharp),
                4 => (NoteStep.E, Alteration.neutral),
                5 => (NoteStep.F, Alteration.neutral),
                6 => (NoteStep.F, Alteration.sharp),
                7 => (NoteStep.G, Alteration.neutral),
                8 => (NoteStep.G, Alteration.sharp),
                9 => (NoteStep.A, Alteration.neutral),
                10 => (NoteStep.A, Alteration.sharp),
                11 => (NoteStep.B, Alteration.neutral),
                _ => throw new System.Diagnostics.UnreachableException()
            };
            return new Pitch(note, alteration, midi / 12);
        }

        // Opérateurs pour manipuler les hauteurs (en demi-tons)
        public static Pitch operator +(Pitch p, int semitones) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            return FromMidi(p.ToMidiNoteNumber() + semitones);
        }

        public static Pitch operator +(int semitones, Pitch p) {
            return p + semitones;
        }

        public static Pitch operator -(Pitch p, int semitones) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            return FromMidi(p.ToMidiNoteNumber() - semitones);
        }

        // Soustraction entre deux Pitch renvoie la différence en demi-tons (int)
        public static int operator -(Pitch left, Pitch right) {
            if (left is null) {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null) {
                throw new ArgumentNullException(nameof(right));
            }

            return left.ToMidiNoteNumber() - right.ToMidiNoteNumber();
        }

        // Modulo : permet d'obtenir le reste de la division utilisant la valeur MIDI
        public static Pitch operator %(Pitch left, int modulus) {
            if (left is null) {
                throw new ArgumentNullException(nameof(left));
            }

            if (modulus == 0) {
                throw new DivideByZeroException("Modulo by zero.");
            }

            return FromMidi(left.ToMidiNoteNumber() % modulus);
        }

        public static int operator %(int value, Pitch p) {
            if (p is null) {
                throw new ArgumentNullException(nameof(p));
            }

            int modulus = p.ToMidiNoteNumber();
            if (modulus == 0) {
                throw new DivideByZeroException("Modulo by zero.");
            }

            return value % modulus;
        }

        public static int operator %(Pitch left, Pitch right) {
            if (left is null) {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null) {
                throw new ArgumentNullException(nameof(right));
            }

            int modulus = right.ToMidiNoteNumber();
            if (modulus == 0) {
                throw new DivideByZeroException("Modulo by zero.");
            }

            return left.ToMidiNoteNumber() % modulus;
        }

        // Comparaisons basées sur la valeur MIDI
        public static bool operator >(Pitch left, Pitch right) {
            if (left is null) {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null) {
                throw new ArgumentNullException(nameof(right));
            }

            return left.ToMidiNoteNumber() > right.ToMidiNoteNumber();
        }

        public static bool operator <(Pitch left, Pitch right) {
            if (left is null) {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null) {
                throw new ArgumentNullException(nameof(right));
            }

            return left.ToMidiNoteNumber() < right.ToMidiNoteNumber();
        }

        public static bool operator >=(Pitch left, Pitch right) {
            if (left is null) {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null) {
                throw new ArgumentNullException(nameof(right));
            }

            return left.ToMidiNoteNumber() >= right.ToMidiNoteNumber();   
        }

        public static bool operator <=(Pitch left, Pitch right) {
            if (left is null) {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null) {
                throw new ArgumentNullException(nameof(right));
            }

            return left.ToMidiNoteNumber() <= right.ToMidiNoteNumber();
        }

        public static bool operator ==(Pitch left, Pitch right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }

            if (left is null || right is null) {
                return false;
            }

            return left.ToMidiNoteNumber() == right.ToMidiNoteNumber();
        }

        // Opérateur != implémenté
        public static bool operator !=(Pitch left, Pitch right) {
            if (ReferenceEquals(left, right)) {
                return false;
            }

            if (left is null || right is null) {
                return true;
            }

            return left.ToMidiNoteNumber() != right.ToMidiNoteNumber();
        }

        public bool Equals(Pitch? other) {
            // vérifier explicitement la nullité de other
            if (other is null) {
                return false;
            }
            return this == other;
        }

        public override bool Equals(object? obj) {
            if (obj is Pitch p) {
                return Equals(p);
            }

            return false;
        }

        public override int GetHashCode() {
            return ToMidiNoteNumber();
        }

        public int CompareTo(Pitch? other)
        {
            if (other is null) {
                return 1;
            }

            return ToMidiNoteNumber().CompareTo(other.ToMidiNoteNumber());
        }

        internal string ToHumanString() {
            string humanString = string.Empty;

            humanString += NoteStepStringifier.ToHumanString(Note, true);
            humanString += AlterationStringifier.ToHumanString(Alteration);
            humanString += MidiOctave.ToString();

            return humanString;
        }
    }
}
