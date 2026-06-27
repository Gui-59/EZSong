using EZSong.Enums;
using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EZSong.Exporting.Lilypond {
    public class LilypondFileBuilder {

        const string _backslash = "\\";
        const string _dblquote = "\"";
        const string _opening_bracket = "{";
        const string _closing_bracket = "}";

        const string _lilypondTargetVersion = "2.24.2";

        const string _styleSheetName = "default-style";

        const string _lilypondvarSongchords = "songchords";
        const string _lilypondvarSonglyrics = "songlyrics";
        const string _lilypondvarSongmelody = "songmelody";

        private Song _song;
        private readonly ILilypondConverter _lilypondConverter;

        public LilypondFileBuilder(Song song) : this(song, new LilypondConverter()) {
        }

        public LilypondFileBuilder(Song song, ILilypondConverter converter) {
            _song = song;
            _lilypondConverter = converter;
        }

        public void GenerateLilypondFile(String outputFilePath) {

            String directory = Path.GetDirectoryName(outputFilePath) ?? string.Empty;
            if (!Directory.Exists(directory)) {
                _ = Directory.CreateDirectory(directory);
            }

            String fullScript = string.Empty;

            fullScript += GenerateLilypondScriptHeader();
            fullScript += GenerateLilypondSheetHeader();

            int staffIndex = 0;
            fullScript += GenerateLilypondSongmelodyVar(staffIndex);

            fullScript += GenerateLilypondSongchordsVar();
            fullScript += GenerateLilypondSonglyricsVar();
            fullScript += GenerateLilypondScoreAssembly();

            File.WriteAllText(outputFilePath, fullScript);
        }

        public void GeneratePdfFile(String outputFilePath) {
            String tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            String tempFileName = Path.GetFileNameWithoutExtension(outputFilePath) + ".ly";
            String tempFilePath = Path.Combine(tempDirectory, tempFileName);
            GenerateLilypondFile(tempFilePath);

            // On exécute une commande de génération sur le modèle : 
            // C:\path\to\lilypond.exe -I "C:\temp\dir\path" -I "C:\path\to\stylesheets\directory" -o "C:\output\file\path" "tempFileName.ly"   

            string lilypondExe = @"C:\lilypond\bin\lilypond.exe"; //TODO : paramétrer

            string exeDir = AppContext.BaseDirectory;
            string stylesheetDir = Path.Combine(exeDir, "Exporting", "Lilypond", "Stylesheets");

            string arguments =
                $"-I \"{tempDirectory}\" " +
                $"-I \"{stylesheetDir}\" " +
                $"-o \"{outputFilePath}\" " +
                $"\"{tempFileName}\"";

            ProcessStartInfo psi = new() {
                FileName = lilypondExe,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process? process = Process.Start(psi)) {
                if (process != null) {
                    _ = process.StandardOutput.ReadToEnd();

                    _ = process.StandardError.ReadToEnd();

                    process.WaitForExit();
                }
            }
        }

        private string GenerateLilypondScriptHeader() {
            StringBuilder sw = new();

			//version de LilyPond
            _ = sw.AppendLine($"{_backslash}version {_dblquote}{_lilypondTargetVersion}{_dblquote}");

            //Feuille de style
            _ = sw.AppendLine($"{_backslash}include {_dblquote}{_styleSheetName}.ly{_dblquote}");

            return sw.ToString();
        }

        private string GenerateLilypondSheetHeader() {
            StringBuilder sw = new();

			//Entête (titre et compositeur)
            _ = sw.AppendLine($"{_backslash}header {_opening_bracket} title = {_dblquote}{_song.Title}{_dblquote} composer = {_dblquote}{_song.Artist}{_dblquote} {_closing_bracket}");

            return sw.ToString();
        }

        private string GenerateLilypondSongmelodyVar(int staffIndex) {
            StringBuilder sw = new();

           /*
             * Rappel de la synthaxe : 
             * 
             * Les noms des notes doivent toujours être en minuscules 
             * (et doivent indiquer une durée totale exactement égale à la longeur de la mesure)            
             * c' ? do une octave au-dessus
             * c'' ? deux octaves au-dessus
             * c ? do central
             * c, ? do une octave en dessous
             * c,, ? deux octaves en dessous
             * 
             * Altérations : 
             * Altération	        Suffixe	
             * ? (dièse)	        is	    
             * ? (bémol)	        es	
             * ? (bécarre)	        !	
             * ?? (double dièse)	isis	
             * ?? (double bémol)	eses
             * 
             * On entoure les notes d’un accord avec < >.
             * 
             * 
             * La durée est indiquée par un chiffre après la note :
             * 
             * Durée	        Code LilyPond	Nom français
             * ronde	        1	            1
             * blanche	        2	            1/2
             * noire	        4	            1/4
             * croche	        8	            1/8
             * double croche    16	            1/16
             * triple croche    32	            1/32
             * 
             * Pour les notes pointées : 
             * On ajoute un "." après la durée
             * 
             * Un tuplet (ex. trois croches en une noire) se note avec \tuplet.
             * Exemple : "\tuplet 3/2 { c8 d e }"   (3 croches en place de 2)
             * 
             * Pour lier une note avec sa suivante : il suffit de mettre un tilde juste après la note
             * 
             * un silence est matérialisé par "r" (avec une durée comme pour les notes)
             * */
			//Mélodie
            _ = sw.AppendLine($"{_lilypondvarSongmelody} = {_opening_bracket}");

            foreach (MeasureData m in _song.Measures) {
                KeySignature keySignature = m.KeySignature;
                _ = sw.AppendLine(GenerateLilypondKeySignature(keySignature));

                TimeSignature timeSignature = new(m.TimeSignature.Beats, m.TimeSignature.BeatUnit);
                _ = sw.AppendLine(GenerateLilypondTimeSignature(timeSignature));

                _ = sw.AppendLine(_lilypondConverter.FormatMeasureMelody(m.GlobalMelody.Melody, m, staffIndex));

                _ = sw.AppendLine($"{_backslash}bar{_dblquote}|{_dblquote}");
            }

            _ = sw.AppendLine($"{_closing_bracket}");

            return sw.ToString();
        }

        private string GenerateLilypondTimeSignature(TimeSignature timeSignature) {
            return "\\time " + _lilypondConverter.FormatTimeSignature(timeSignature);
        }

        private string GenerateLilypondKeySignature(KeySignature keySignature) {
            // Pour l'instant, on réimplémente ici la conversion
            string lilypondCode = string.Empty;
			/*
             * Tonalité	    Armure		Notes altérées							Commande LilyPond
             * Do majeur	—			—										\key c \major
             * La mineur	—			—										\key a \minor
             * Sol majeur	1 ?			fa?										\key g \major
             * Mi mineur	1 ?			fa?										\key e \minor
             * Ré majeur	2 ?			fa?, do?								\key d \major
             * Si mineur	2 ?			fa?, do?								\key b \minor
             * La majeur	3 ?			fa?, do?, sol?							\key a \major
             * Fa? mineur	3 ?			fa?, do?, sol?							\key fis \minor
             * Mi majeur	4 ?			fa?, do?, sol?, ré?						\key e \major
             * Do? mineur	4 ?			fa?, do?, sol?, ré?						\key cis \minor
             * Si majeur	5 ?			fa?, do?, sol?, ré?, la?				\key b \major
             * Sol? mineur	5 ?			fa?, do?, sol?, ré?, la?				\key gis \minor
             * Fa? majeur	6 ?			fa?, do?, sol?, ré?, la?, mi?			\key fis \major
             * Ré? mineur	6 ?			fa?, do?, sol?, ré?, la?, mi?			\key dis \minor
             * Do? majeur	7 ?			fa?, do?, sol?, ré?, la?, mi?, si?		\key cis \major
             * La? mineur	7 ?			fa?, do?, sol?, ré?, la?, mi?, si?		\key ais \minor
             * Fa majeur	1 ?			si?										\key f \major
             * Ré mineur	1 ?			si?										\key d \minor
             * Si? majeur	2 ?			si?, mi?									\key bes \major
             * Sol mineur	2 ?			si?, mi?									\key g \minor
             * Mi? majeur	3 ?			si?, mi?, la?							\key ees \major
             * Do mineur	3 ?			si?, mi?, la?							\key c \minor
             * La? majeur	4 ?			si?, mi?, la?, ré?						\key aes \major
             * Fa mineur	4 ?			si?, mi?, la?, ré?						\key f \minor
             * Ré? majeur	5 ?			si?, mi?, la?, ré?, sol?					\key des \major
             * Si? mineur	5 ?			si?, mi?, la?, ré?, sol?					\key bes \minor
             * Sol? majeur	6 ?			si?, mi?, la?, ré?, sol?, do?				\key ges \major
             * Mi? mineur	6 ?			si?, mi?, la?, ré?, sol?, do?				\key ees \minor
             * Do? majeur	7 ?			si?, mi?, la?, ré?, sol?, do?, fa?		\key ces \major
             * La? mineur	7 ?			si?, mi?, la?, ré?, sol?, do?, fa?		\key aes \minor
             */

            lilypondCode += "\\key ";

            switch (keySignature.Note) {
                case NoteStep.A:
                    lilypondCode += "a";
                    break;
                case NoteStep.B:
                    lilypondCode += "b";
                    break;
                case NoteStep.C:
                    lilypondCode += "c";
                    break;
                case NoteStep.D:
                    lilypondCode += "d";
                    break;
                case NoteStep.E:
                    lilypondCode += "e";
                    break;
                case NoteStep.F:
                    lilypondCode += "f";
                    break;
                case NoteStep.G:
                    lilypondCode += "g";
                    break;
                default:
                    return string.Empty;
            }

            switch (keySignature.Alteration) {
                case Alteration.flat:
                    lilypondCode += "es";
                    break;
                case Alteration.sharp:
                    lilypondCode += "is";
                    break;
                case Alteration.neutral:
                    lilypondCode += string.Empty;
                    break;
                default:
                    return string.Empty;
            }

            lilypondCode += " ";

            switch (keySignature.Mode) {
                case SongMode.major:
                    lilypondCode += "\\major";
                    break;
                case SongMode.minor:
                    lilypondCode += "\\minor";
                    break;
                default:
                    return string.Empty;
            }

            return lilypondCode;
        }

        private string GenerateLilypondSongchordsVar() {
            StringBuilder sw = new();

			//accords
            _ = sw.AppendLine($"{_lilypondvarSongchords} =  {_backslash}chordmode {_opening_bracket} ");
            foreach (MeasureData m in _song.Measures) {
                _ = sw.AppendLine($"{_lilypondConverter.FormatChordSequence(m.ChordSequence)}");
                _ = sw.AppendLine($"{_backslash}bar{_dblquote}|{_dblquote}");
            }

            _ = sw.AppendLine($"{_closing_bracket} ");

            return sw.ToString();
        }

        private string GenerateLilypondSonglyricsVar() {
            StringBuilder sw = new();

			//Paroles sans découpage par syllabe (une phrase par mesure)
            _ = sw.AppendLine($"{_lilypondvarSonglyrics} = {_opening_bracket}");
            foreach (MeasureData m in _song.Measures) {
                _ = sw.AppendLine($"s1_{_backslash}markup {_opening_bracket} {_dblquote}{m.Lyrics}{_dblquote} {_closing_bracket} ");
                _ = sw.AppendLine($"{_backslash}bar{_dblquote}|{_dblquote}");
            }

            _ = sw.AppendLine($"{_closing_bracket} ");

            return sw.ToString();
        }

        private string GenerateLilypondScoreAssembly() {
            StringBuilder sw = new();

			//Code LilyPond pour l'assemblage de la partition 	
            _ = sw.AppendLine($"{_backslash}score {_opening_bracket}");
            _ = sw.AppendLine($"<<");

            _ = sw.AppendLine($"{_backslash}new Staff = {_dblquote}melStaff{_dblquote} <<");

            _ = sw.AppendLine($"{_backslash}new Voice = {_dblquote}mel{_dblquote} {_opening_bracket} {_backslash}{_lilypondvarSongmelody} {_closing_bracket}");
            _ = sw.AppendLine($" {_backslash}new Voice = {_dblquote}parolesMesures{_dblquote} {_opening_bracket} {_backslash}{_lilypondvarSonglyrics} {_closing_bracket}");
            _ = sw.AppendLine($">>");

            _ = sw.AppendLine($"{_backslash}new ChordNames {_backslash}with {_opening_bracket} alignAboveContext = {_dblquote}melStaff{_dblquote} {_closing_bracket} {_opening_bracket} {_backslash}{_lilypondvarSongchords} {_closing_bracket}");

            _ = sw.AppendLine($">>");

            _ = sw.AppendLine($"{_closing_bracket}");

            return sw.ToString();
        }
    }
}