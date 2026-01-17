using EZSong.Enums;
using EZSong.EnumsStringifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EZSong.Serializable {

    [Serializable]
    public class Chord {

        /* 
         * Notes : 
         * - Un accord sixième est une septième doublement diminuée (double dim).
         */

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

        public NoteDuration Duration;

        public NoteStep RootNote;
        public Alteration RootNoteAlteration;
        public ChordMode ThirdNoteMode;
        public ChordMode FithNoteMode;
        public ChordMode SeventhNoteMode;
        public ChordMode NinthNoteMode;

        public Chord() {
            // Constructeur par défaut (requis pour la sérialisation)
            Duration = new();
        }

        public Chord(string uiChordString) {

            //Valeurs par défaut
            IsSilentChord = false;
            RootNote = NoteStep.C;
            RootNoteAlteration = Alteration.neutral;
            Duration = new(WholeFraction.QUARTER, 0); //TODO (par défaut on met la durée d'une noire non pointée)
            ThirdNoteMode = ChordMode.None;
            FithNoteMode = ChordMode.None;
            SeventhNoteMode = ChordMode.None;
            NinthNoteMode = ChordMode.None;
                        
            if (uiChordString == "") {
                IsSilentChord = true;
                return;
            }

            string[] uiChordSplit = uiChordString.Split(":");

            string beforeColon = uiChordSplit[0];
           
            string afterColon = "";
            if (uiChordSplit.Count() > 1) {
                afterColon = uiChordSplit[1];
            }

            int dotsCount = beforeColon.Count(t => t == '.');
            string firstHalfRest = beforeColon.Trim('.');

            //La durée de la note est la valeur numérique indiquée en fin de première partie
            WholeFraction wholeFraction = WholeFraction.WHOLE;
            int firstDigitIndex = firstHalfRest.IndexOfAny("0123456789".ToCharArray());
            if (firstDigitIndex > 0) {
                switch (Int32.Parse(firstHalfRest.Substring(firstDigitIndex))) {
                    case 1:
                        wholeFraction = WholeFraction.WHOLE;
                        break;
                    case 2:
                        wholeFraction = WholeFraction.HALF;
                        break;
                    case 4:
                        wholeFraction = WholeFraction.QUARTER;
                        break;
                    case 8:
                        wholeFraction = WholeFraction.EIGHTH;
                        break;
                    case 16:
                        wholeFraction = WholeFraction.SIXTEENTH;
                        break;
                    case 32:
                        wholeFraction = WholeFraction.THIRTYSECOND;
                        break;
                    default:
                        wholeFraction = WholeFraction.WHOLE;
                        break;
                }

                
                firstHalfRest = firstHalfRest.Trim("0123456789".ToCharArray());
            }

            Duration = new(wholeFraction, dotsCount); 

            if (firstHalfRest.ToLowerInvariant().EndsWith("isis")) {
                RootNoteAlteration = Alteration.sharpsharp;
                firstHalfRest = firstHalfRest.Remove(firstHalfRest.Length - 4);
            } else if (firstHalfRest.ToLowerInvariant().EndsWith("eses")) {
                RootNoteAlteration = Alteration.flat;
                firstHalfRest = firstHalfRest.Remove(firstHalfRest.Length - 4);
            } else if (firstHalfRest.ToLowerInvariant().EndsWith("is")) {
                RootNoteAlteration = Alteration.sharp;
                firstHalfRest = firstHalfRest.Remove(firstHalfRest.Length - 2);
            } else if (firstHalfRest.ToLowerInvariant().EndsWith("es")) {
                RootNoteAlteration = Alteration.flat;
                firstHalfRest = firstHalfRest.Remove(firstHalfRest.Length - 2);
            }

            int readCharCount = 0;

            //Reste juste la fondamentale
            switch (firstHalfRest.Substring(0, 1).ToLowerInvariant()) {
                case "c":
                    RootNote = NoteStep.C;
                    readCharCount += 1;
                    break;
                case "d":
                    RootNote = NoteStep.D;
                    readCharCount += 1;
                    break;
                case "e":
                    RootNote = NoteStep.E;
                    readCharCount += 1;
                    break;
                case "f":
                    RootNote = NoteStep.F;
                    readCharCount += 1;
                    break;
                case "g":
                    RootNote = NoteStep.G;
                    readCharCount += 1;
                    break;
                case "a":
                    RootNote = NoteStep.A;
                    readCharCount += 1;
                    break;
                case "b":
                    RootNote = NoteStep.B;
                    readCharCount += 1;
                    break;
            }

            //TODO : détection des autres composantes de l'accord
            ThirdNoteMode = ChordMode.None;
            FithNoteMode = ChordMode.None;
            SeventhNoteMode = ChordMode.None;
            NinthNoteMode = ChordMode.None;
        }

        public string ToLilyPondString() {

            if (IsSilentChord) {
                return "";            
            }

            string lilyPondString = "";
            lilyPondString += NoteStepStringifier.ToLilyPondString(RootNote);
            lilyPondString += AlterationStringifier.ToLilyPondString(RootNoteAlteration);

            lilyPondString += Duration.ToLilyPondString();


            lilyPondString += ":";
            

            switch (ThirdNoteMode) {
                case ChordMode.Minor:
                    lilyPondString += "m";
                    break;
                case ChordMode.Major:
                    lilyPondString += "";
                    break;
                case ChordMode.Aug:
                    lilyPondString += "aug";
                    break;
                case ChordMode.None:
                    lilyPondString += "5";
                    break;
                default:
                    lilyPondString += "";
                    break;
            }

            //TODO : Compléter les cas

            return lilyPondString.Trim(':');
        }

        public string ToHumanString() {
            //TODO : Générer une chaine plus "human friendly"
            return ToGuiString();   
        }

        public string ToGuiString() {
            //TODO (pour le moment, on la syntaxe de saisie dans la GUI doit suivre la syntaxe de LilyPond) 
            return ToLilyPondString();
        }


    }
}
