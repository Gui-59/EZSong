using EZSong.Enums;
using EZSong.EnumsStringifier;
using EZSong.Exporting.Lilypond;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EZSong.Model {

    public class Chord {

        /*
         * Rappel de la syntaxe pour LilyPond
         * 
         * | Type d’accord | Exemple  | Résultat visible | Signification         |
         * | ------------- | -------- | ---------------- | --------------------- |
         * | Majeur        | `c`      | C                | Do majeur             |
         * | Mineur        | `a:m`    | Am               | La mineur             |
         * | 7e            | `g:7`    | G7               | Sol septième          |
         * | Maj7          | `c:maj7` | Cmaj7            | Do majeur 7           |
         * | m7            | `a:m7`   | Am7              | La mineur 7           |
         * | Sus4          | `d:sus4` | Dsus4            | Ré suspendu 4         |
         * | Sus2          | `g:sus2` | Gsus2            | Sol suspendu 2        |
         * | Power chord   | `e:5`    | E5               | Mi + Si (sans tierce) |
         * | Diminué       | `b:dim`  | Bdim             | Si diminué            |
         * | Augmenté      | `c:aug`  | Caug             | Do augmenté           |
         * | 6e            | `f:6`    | F6               | Fa sixte              |
         * | 9e            | `c:9`    | C9               | Do neuvième           |
         * | Maj9          | `c:maj9` | Cmaj9            | Do majeur 9           |
         * 
         * La durée de l'accord doit être indiquée avant le ":"
         *
         */

        public bool IsSilentChord;

        public RhythmRationalDuration Duration { 
            get; 
            set; 
        }

        public NoteStep RootNote {
            get; 
            set;
        }
        public Alteration RootNoteAlteration {
            get; 
            set;
        }

        public ChordType ChordType {
            get; 
            set;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public Chord() {
            IsSilentChord = false;
            RootNote = NoteStep.C;
            RootNoteAlteration = Alteration.neutral;
            //TODO (par défaut on met la durée d'une noire non pointée)
            Duration = new(1, 4, 0); 
            ChordType = ChordType.NoneOrMajor;
        }

        public Chord(
            bool isSilentChord, 
            RhythmRationalDuration duration, 
            NoteStep rootNote, 
            Alteration rootNoteAlteration,
            ChordType chordType
        ) {
            IsSilentChord = isSilentChord;
            Duration = duration;    
            RootNote = rootNote;    
            RootNoteAlteration = rootNoteAlteration;    
            ChordType = chordType;
        }

        public string ToLilyPondString() {

            if (IsSilentChord) {
                return ""; //TODO : gérer les silences d'accords dans LilyPond (peut-être avec \skip ou \rest)       
            }

            ILilypondConverter _lilypondConverter = new LilypondConverter();

            string lilyPondString = "";
            lilyPondString += _lilypondConverter.NoteStepToLilyPondString(RootNote);
            lilyPondString += _lilypondConverter.AlterationToLilyPondString(RootNoteAlteration);

            lilyPondString += Duration.ToLilyPondString();

            switch (ChordType) { //TODO : créer une méthode ToLilyPondString() dans ChordTypeStringifier et l'appeler ici
                case ChordType.NoneOrMajor:
                    break;
                case ChordType.Minor:
                    lilyPondString += ":m";
                    break;
                case ChordType.Seventh:
                    lilyPondString += ":7";
                    break;
                case ChordType.MinorSeventh:
                    lilyPondString += ":m7";
                    break;
                case ChordType.MajorSeventh:
                    lilyPondString += ":maj7";
                    break;
                case ChordType.PowerChord:
                    lilyPondString += ":5";
                    break;
                case ChordType.Sixth:
                    lilyPondString += ":6";
                    break;
                case ChordType.MinorSixth:
                    lilyPondString += ":m6";
                    break;
                case ChordType.SuspendedSecond:
                    lilyPondString += ":sus2";
                    break;
                case ChordType.SuspendedFourth:
                    lilyPondString += ":sus4";
                    break;
                case ChordType.Diminished:
                    lilyPondString += ":dim";
                    break;
                case ChordType.Augmented:
                    lilyPondString += ":aug";
                    break;
                case ChordType.DiminishedSeventh:
                    lilyPondString += ":dim7";
                    break;
                case ChordType.AugmentedSeventh:
                    lilyPondString += ":aug7";
                    break;
                case ChordType.AddSecond:
                    lilyPondString += ":add2";
                    break;
                case ChordType.AddFourth:
                    lilyPondString += ":add4";
                    break;
                case ChordType.AddSixth:
                    lilyPondString += ":add6";
                    break;
                case ChordType.AddNinth:
                    lilyPondString += ":add9";
                    break;
                case ChordType.Ninth:
                    lilyPondString += ":9";
                    break;
                case ChordType.MinorNinth:
                    lilyPondString += ":m9";
                    break;
                case ChordType.MajorNinth:
                    lilyPondString += ":maj9";
                    break;
                case ChordType.Eleventh:
                    lilyPondString += ":11";
                    break;
                case ChordType.MinorEleventh:
                    lilyPondString += ":m11";
                    break;
                case ChordType.MajorEleventh:
                    lilyPondString += ":maj11";
                    break;
                case ChordType.Thirteenth:
                    lilyPondString += ":13";
                    break;
                case ChordType.MinorThirteenth:
                    lilyPondString += ":m13";
                    break;
                case ChordType.MajorThirteenth:
                    lilyPondString += ":maj13";
                    break;
                case ChordType.MinorMajorSeventh:
                    lilyPondString += ":m(maj7)";
                    break;
                case ChordType.SixthNinth:
                    lilyPondString += ":6/9";
                    break;
                case ChordType.SeventhMinusFive:
                    lilyPondString += ":7-5";
                    break;
                case ChordType.SeventhPlusFive:
                    lilyPondString += ":7+5";
                    break;
                case ChordType.MinorSeventhFlatFive:
                    lilyPondString += ":m7b5";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }

            return lilyPondString.Trim(':');
        }

        public string ToHumanString() {
            string humanString = "";

            if (IsSilentChord) {
                humanString += "¤"; //TODO : gérer les silences d'accords dans la représentation humaine (peut-être avec un symbole spécial)
            } else {
                humanString += NoteStepStringifier.ToHumanString(RootNote, false); //TODO : gérer la notation internationale (avec un paramètre de configuration)
                if (RootNoteAlteration != Alteration.neutral) {
                    humanString += AlterationStringifier.ToHumanString(RootNoteAlteration);
                }
                humanString += ChordTypeStringifier.ToHumanString(ChordType);
            }
            return humanString;
        }

        internal ChordDto ToDto() {
            return new ChordDto(
                IsSilentChord, 
                Duration, 
                RootNote, 
                RootNoteAlteration, 
                ChordType
            );
        }

        public static Chord FromDto(ChordDto chordDto) {
            return new Chord (
                chordDto.IsSilentChord,
                chordDto.Duration,
                chordDto.RootNote,
                chordDto.RootNoteAlteration,
                chordDto.ChordType
            );
        }
    }
}