using Atk;
using EZSong.Enums;
using EZSong.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Reflection.PortableExecutable;
using System.Text;

namespace EZSong.Exporting.Lilypond {
    public class LilypondFileBuilder {

        const string _backslash = "\\"; //TODO : utiliser un verbatim string pour �viter d'avoir � doubler les backslashs
        const string _dblquote = "\""; //TODO : utiliser un verbatim string pour �viter d'avoir � doubler les guillemets
        const string _opening_bracket = "{"; //TODO : utiliser un verbatim string pour �viter d'avoir � doubler les accolades
        const string _closing_bracket = "}"; //TODO : utiliser un verbatim string pour �viter d'avoir � doubler les accolades

		/// <summary>
        /// LilyPond Target Version
        /// </summary>
        const string _lilypondTargetVersion = "2.24.2"; //TODO : param�trer la version de Lilypond � utiliser pour la g�n�ration du fichier .ly

        /// <summary>
        /// Lilypond Style Sheet Name
        /// </summary>
        const string _styleSheetName = "default-style"; //TODO : param�trer le nom de la feuille de style � utiliser pour la g�n�ration du fichier .ly (la feuille de style doit se trouver dans le r�pertoire "Stylesheets" du projet)


        private List<LilyPondVar> _lilypondSongMelodies;
        private List<LilyPondVar> _lilypondSongChords;
        private List<LilyPondVar> _lilypondSongLyrics;

        /// <summary>
        /// Represente the whole song
        /// </summary>
        private Song _song;

        /// <summary>
        /// Helper for conversion of musical objets to Lilypond syntax 
        /// </summary>
        private readonly ILilypondConverter _lilypondConverter;


        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="song"></param>
        public LilypondFileBuilder(Song song) : this(song, new LilypondConverter()) {
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="song"></param>
        /// <param name="converter"></param>
        public LilypondFileBuilder(Song song, ILilypondConverter converter) {
            _lilypondSongMelodies = new();
            _lilypondSongChords = new();
            _lilypondSongLyrics = new();
            _song = song;
            _lilypondConverter = converter;
        }

        /// <summary>
        /// Generate a LilyPond file from current song
        /// </summary>
        /// <param name="outputFilePath"></param>
        /// <exception cref="ArgumentException"></exception>
        public void GenerateLilypondFile(String outputFilePath) {

            String? directory = Path.GetDirectoryName(outputFilePath);

            if (directory == null) {
                throw new ArgumentException("Output file path is invalid.", nameof(outputFilePath));
            }

            if (!Directory.Exists(directory)) {
                _ = Directory.CreateDirectory(directory);
            }

            String fullScript = string.Empty;

            fullScript += GenerateLilypondScriptHeader();
            fullScript += GenerateLilypondSheetHeader();

            for (int segmentIndex = 0; segmentIndex < _song.Segments.Count(); segmentIndex++) {

                for (int staffIndex = 0; staffIndex < _song.SongSettings.StaffsSettings.Staffs.Count(); staffIndex++) {
                    SongElementCoordinates songElementCoordinates  = new(segmentIndex, staffIndex);
                    LilyPondVar lilypondMelodyVar = GenerateLilypondSongmelodyVar(songElementCoordinates);
                    _lilypondSongMelodies.Add(lilypondMelodyVar);
                    fullScript += lilypondMelodyVar.Value;
                }

                SongElementCoordinates songElementCoordinates2 = new(segmentIndex, null);

                LilyPondVar lilypondChordsVar = GenerateLilypondSongchordsVar(songElementCoordinates2);
                _lilypondSongChords.Add(lilypondChordsVar);
                fullScript += lilypondChordsVar.Value;

                LilyPondVar lilypondLyricsVar = GenerateLilypondSonglyricsVar(songElementCoordinates2);
                _lilypondSongLyrics.Add(lilypondLyricsVar);
                fullScript += lilypondLyricsVar.Value;
            }


            fullScript += GenerateLilypondScoreAssembly();

            File.WriteAllText(outputFilePath, fullScript);
        }

        /// <summary>
        /// Generate a PDF file from current song (using LilyPond)
        /// </summary>
        /// <param name="outputFilePath"></param>
        public void GeneratePdfFile(String outputFilePath) {

            if (!outputFilePath.ToLowerInvariant().EndsWith(".pdf")) {
                outputFilePath += ".pdf";
            }

            //TODO : cr�er une m�thode pour g�n�rer un nom de r�pertoire temporaire unique
            String tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);
            String tempFileName = Path.GetFileNameWithoutExtension(outputFilePath) + ".ly";
            String tempFilePath = Path.Combine(tempDirectory, tempFileName);
            GenerateLilypondFile(tempFilePath);

            //TODO : placer cette g�n�ration dans un thread s�par� pour ne pas bloquer l'interface utilisateur (et afficher une barre de progression)
            /////////////////////
            
            // On ex�cute une commande de g�n�ration sur le mod�le : 
            // C:\path\to\lilypond.exe -I "C:\temp\dir\path" -I "C:\path\to\stylesheets\directory" -o "C:\output\file\path" "tempFileName.ly"   

            string lilypondExe = @"C:\lilypond\bin\lilypond.exe"; //TODO : param�trer
            if (!File.Exists(lilypondExe)) {
                throw new FileNotFoundException("LilyPond introuvable : " + lilypondExe, lilypondExe);
            }

            string exeDir = AppContext.BaseDirectory;
            string stylesheetDir = Path.Combine(exeDir, "Exporting", "Lilypond", "Stylesheets");

            string outputPrefix = outputFilePath;
            if (outputPrefix.ToLowerInvariant().EndsWith(".pdf")) {
                outputPrefix = outputPrefix.Substring(0, outputPrefix.Length - 4);
            }

            string arguments =
                $"-I \"{tempDirectory}\" " +
                $"-I \"{stylesheetDir}\" " +
                $"-o \"{outputPrefix}\" " +
                $"\"{tempFileName}\"";

            ProcessStartInfo psi = new() {
                FileName = lilypondExe,
                Arguments = arguments,
                WorkingDirectory = tempDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process? process = Process.Start(psi)) {
                if (process != null) {
                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    Console.WriteLine(stdout);
                    Console.WriteLine(stderr);

                    if (process.ExitCode != 0) {
                        throw new InvalidOperationException("Echec PDF (code " + process.ExitCode + ").\n" + stderr);
                    }
                    return;
                } else {
                    throw new InvalidOperationException("Impossible de demarrer LilyPond.");
                }
            }
        }

        /// <summary>
        /// Generate a LilyPond script header from current song
        /// </summary>
        /// <returns></returns>
        private string GenerateLilypondScriptHeader() {
            StringBuilder sw = new();

			//version de LilyPond
            _ = sw.AppendLine($"{_backslash}version {_dblquote}{_lilypondTargetVersion}{_dblquote}");

            //Feuille de style
            _ = sw.AppendLine($"{_backslash}include {_dblquote}{_styleSheetName}.ly{_dblquote}");

            return sw.ToString();
        }

        /// <summary>
        /// Generate a LilyPond sheet header from current song
        /// </summary>
        /// <returns></returns>
        private string GenerateLilypondSheetHeader() {
            StringBuilder sw = new();

			//Ent�te (titre et compositeur)
            //TODO : Ajouter l'info "remarques"
            _ = sw.AppendLine(
                $"{_backslash}header " +
                $"{_opening_bracket} " +
                $"title = {_dblquote}{_song.Title}{_dblquote} " +
                $"composer = {_dblquote}{_song.Artist}{_dblquote}" +
                $"{_closing_bracket}");

            return sw.ToString();
        }

        /// <summary>
        /// Generate a LilyPond script melody variable from current song segment/staff
        /// </summary>
        /// <returns></returns>
        private LilyPondVar GenerateLilypondSongmelodyVar(SongElementCoordinates songElementCoordinates) {

            StringBuilder sw = new();
            LilyPondVar lilypondMelodyVar = new (SongLayer.Melody, songElementCoordinates);

            /*
              * Rappel de la synthaxe : 
              * 
              * Les noms des notes doivent toujours �tre en minuscules 
              * (et doivent indiquer une dur�e totale exactement �gale � la longeur de la mesure)            
              * c' ? do une octave au-dessus
              * c'' ? deux octaves au-dessus
              * c ? do central
              * c, ? do une octave en dessous
              * c,, ? deux octaves en dessous
              * 
              * Alt�rations : 
              * Alt�ration	        Suffixe	
              * ? (di�se)	        is	    
              * ? (b�mol)	        es	
              * ? (b�carre)	        !	
              * ?? (double di�se)	isis	
              * ?? (double b�mol)	eses
              * 
              * On entoure les notes d�un accord avec < >.
              * 
              * La dur�e est indiqu�e par un chiffre apr�s la note :
              * 
              * Dur�e	        Code LilyPond	Nom fran�ais
              * ronde	        1	            1
              * blanche	        2	            1/2
              * noire	        4	            1/4
              * croche	        8	            1/8
              * double croche    16	            1/16
              * triple croche    32	            1/32
              * 
              * Pour les notes point�es : 
              * On ajoute un "." apr�s la dur�e
              * 
              * Un tuplet (ex. trois croches en une noire) se note avec \tuplet.
              * Exemple : "\tuplet 3/2 { c8 d e }"   (3 croches en place de 2)
              * 
              * Pour lier une note avec sa suivante : il suffit de mettre un tilde juste apr�s la note
              * 
              * un silence est mat�rialis� par "r" (avec une dur�e comme pour les notes)
              * 
              */


            _ = sw.AppendLine($"{lilypondMelodyVar.Name} = {_opening_bracket}");

            KeySignature? previousKeySignature = null;
            foreach (MeasureData m in _song.Segments[songElementCoordinates.SegmentIndex].Measures) {
                KeySignature keySignature = m.KeySignature;
                // La commande \\key n'est réémise que lorsqu'elle change, afin de
                // générer correctement les changements de tonalité sans répéter
                // inutilement la même armure à chaque mesure.
                if (previousKeySignature is null
                    || previousKeySignature.Note != keySignature.Note
                    || previousKeySignature.Alteration != keySignature.Alteration
                    || previousKeySignature.Mode != keySignature.Mode) {
                    _ = sw.AppendLine(KeySignatureToLilyPondString(keySignature));
                    previousKeySignature = keySignature;
                }

                TimeSignature timeSignature = new(m.TimeSignature.Beats, m.TimeSignature.BeatUnit);
                _ = sw.AppendLine(TimeSignatureToLilyPondString(timeSignature));

                if (songElementCoordinates.StaffIndex is not null) {
                    _ = sw.AppendLine(_lilypondConverter.MeasureMelodyToLilyPondString(m.Staffs[(int)songElementCoordinates.StaffIndex].Melody, m));
                }

                _ = sw.AppendLine($"{_backslash}bar{_dblquote}|{_dblquote}");
            }

            _ = sw.AppendLine($"{_closing_bracket}");
            
            lilypondMelodyVar.Value = sw.ToString();
            

            return lilypondMelodyVar;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="timeSignature"></param>
        /// <returns></returns>
        //TODO : d�placer cette m�thode dans la classe LilypondConverter
        private string TimeSignatureToLilyPondString(TimeSignature timeSignature) {
            return "\\time " + _lilypondConverter.TimeSignatureToLilyPondString(timeSignature);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="keySignature"></param>
        /// <returns></returns>
        //TODO : d�placer cette m�thode dans la classe LilypondConverter
        private string KeySignatureToLilyPondString(KeySignature keySignature) {
            // Pour l'instant, on r�impl�mente ici la conversion (TODO)
            string lilypondCode = string.Empty;
			/*
             * Tonalit�	    Armure		Notes alt�r�es							Commande LilyPond
             * Do majeur	�			�										\key c \major
             * La mineur	�			�										\key a \minor
             * Sol majeur	1 ?			fa?										\key g \major
             * Mi mineur	1 ?			fa?										\key e \minor
             * R� majeur	2 ?			fa?, do?								\key d \major
             * Si mineur	2 ?			fa?, do?								\key b \minor
             * La majeur	3 ?			fa?, do?, sol?							\key a \major
             * Fa? mineur	3 ?			fa?, do?, sol?							\key fis \minor
             * Mi majeur	4 ?			fa?, do?, sol?, r�?						\key e \major
             * Do? mineur	4 ?			fa?, do?, sol?, r�?						\key cis \minor
             * Si majeur	5 ?			fa?, do?, sol?, r�?, la?				\key b \major
             * Sol? mineur	5 ?			fa?, do?, sol?, r�?, la?				\key gis \minor
             * Fa? majeur	6 ?			fa?, do?, sol?, r�?, la?, mi?			\key fis \major
             * R�? mineur	6 ?			fa?, do?, sol?, r�?, la?, mi?			\key dis \minor
             * Do? majeur	7 ?			fa?, do?, sol?, r�?, la?, mi?, si?		\key cis \major
             * La? mineur	7 ?			fa?, do?, sol?, r�?, la?, mi?, si?		\key ais \minor
             * Fa majeur	1 ?			si?										\key f \major
             * R� mineur	1 ?			si?										\key d \minor
             * Si? majeur	2 ?			si?, mi?									\key bes \major
             * Sol mineur	2 ?			si?, mi?									\key g \minor
             * Mi? majeur	3 ?			si?, mi?, la?							\key ees \major
             * Do mineur	3 ?			si?, mi?, la?							\key c \minor
             * La? majeur	4 ?			si?, mi?, la?, r�?						\key aes \major
             * Fa mineur	4 ?			si?, mi?, la?, r�?						\key f \minor
             * R�? majeur	5 ?			si?, mi?, la?, r�?, sol?					\key des \major
             * Si? mineur	5 ?			si?, mi?, la?, r�?, sol?					\key bes \minor
             * Sol? majeur	6 ?			si?, mi?, la?, r�?, sol?, do?				\key ges \major
             * Mi? mineur	6 ?			si?, mi?, la?, r�?, sol?, do?				\key ees \minor
             * Do? majeur	7 ?			si?, mi?, la?, r�?, sol?, do?, fa?		\key ces \major
             * La? mineur	7 ?			si?, mi?, la?, r�?, sol?, do?, fa?		\key aes \minor
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="segmentIndex"></param>
        /// <returns></returns>
        private LilyPondVar GenerateLilypondSongchordsVar(SongElementCoordinates songElementCoordinates) {

            LilyPondVar lilypondChordsVar = new(SongLayer.Chords, songElementCoordinates);           
            
            StringBuilder sw = new();

			//accords
            _ = sw.AppendLine($"{lilypondChordsVar.Name} = {_backslash}chordmode {_opening_bracket} ");
            foreach (MeasureData m in _song.Segments[songElementCoordinates.SegmentIndex].Measures) {
                _ = sw.AppendLine($"{_lilypondConverter.ChordSequenceToLilyPondString(m.ChordSequence)}");
                _ = sw.AppendLine($"{_backslash}bar{_dblquote}|{_dblquote}");
            }

            _ = sw.AppendLine($"{_closing_bracket} ");

            lilypondChordsVar.Value = sw.ToString();

            return lilypondChordsVar;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="segmentIndex"></param>
        /// <returns></returns>
        private LilyPondVar GenerateLilypondSonglyricsVar(SongElementCoordinates songElementCoordinates) {

            LilyPondVar lilypondLyricsVar = new(SongLayer.Lyrics, songElementCoordinates);

            StringBuilder sw = new();

			//Paroles sans d�coupage par syllabe (une phrase par mesure)
            _ = sw.AppendLine($"{lilypondLyricsVar.Name} = {_opening_bracket}");
            foreach (MeasureData m in _song.Segments[songElementCoordinates.SegmentIndex].Measures) {
                _ = sw.AppendLine($"s1_{_backslash}markup {_opening_bracket} {_dblquote}{m.Lyrics}{_dblquote} {_closing_bracket} ");
                _ = sw.AppendLine($"{_backslash}bar{_dblquote}|{_dblquote}");
            }

            _ = sw.AppendLine($"{_closing_bracket} ");

            lilypondLyricsVar.Value = sw.ToString();

            return lilypondLyricsVar;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        private string GenerateLilypondScoreAssembly() {
            StringBuilder sw = new();

			//Code LilyPond pour l'assemblage de la partition 	
            

            //On g�n�rer le score avec tout les segments en utilisant les variables de m�lodies, d'accords et de paroles g�n�r�es pr�c�demment

            foreach (SegmentData segment in _song.Segments) {
                _ = sw.AppendLine($"{_backslash}score {_opening_bracket}");
                _ = sw.AppendLine(GenerateLilypondSegmentAssembly(segment));
                _ = sw.AppendLine($"{_closing_bracket}");
            }

            return sw.ToString();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private string GenerateLilypondSegmentAssembly(SegmentData segment) {

            //Syst�me de port�es
            //https://lilypond.org/doc/v2.23/Documentation/learning/multiple-staves

            StringBuilder sw = new();

            String segmentTitle = "Segment "+ segment.Index; //Titre de segment (TODO : ajouter un champ "titre de segment" dans le mod�le de donn�es)
            _ = sw.AppendLine("\\header {");
            _ = sw.AppendLine("piece = \"" + segmentTitle + "\""); 
            _ = sw.AppendLine("}");

            _ = sw.AppendLine($"<<"); //D�but de syst�me de port�es

            for (int staffIndex = 0; staffIndex < _song.SongSettings.StaffsSettings.Staffs.Count(); staffIndex++) {

                _ = sw.AppendLine($"{_backslash}new Staff = {_dblquote}melStaff_{segment.Index}_{staffIndex}{_dblquote} <<"); //D�but de port�e

                SongElementCoordinates songElementCoordinates = new(segment.Index, staffIndex);

                int songElementMelodyIndex = _lilypondSongMelodies.FindIndex(l => l.Layer == SongLayer.Melody && l.Coord.Equals(songElementCoordinates));

                _ = sw.AppendLine(
                        $"{_backslash}new Voice = {_dblquote}mel_{segment.Index}_{staffIndex}{_dblquote} " +
                        $"{_opening_bracket} " +
                        $"{_backslash}{_lilypondSongMelodies[songElementMelodyIndex].Name} " +
                        $"{_closing_bracket}"
                    );

                if (staffIndex == 0) {
                    SongElementCoordinates songElementCoordinates2 = new(segment.Index, null);

                    int songElementLyricsIndex = _lilypondSongLyrics.FindIndex(l => l.Layer == SongLayer.Lyrics && l.Coord.Equals(songElementCoordinates2));

                    _ = sw.AppendLine(
                            $" {_backslash}new Voice = {_dblquote}parolesMesures{_dblquote} " +
                            $"{_opening_bracket} " +
                            $"{_backslash}{_lilypondSongLyrics[songElementLyricsIndex].Name} " +
                            $"{_closing_bracket}"
                        );
                }

                _ = sw.AppendLine($">>"); //Fin de port�e
            }

            SongElementCoordinates songElementCoordinates3 = new(segment.Index, null);

            int songElementChordsIndex = _lilypondSongChords.FindIndex(l => l.Layer == SongLayer.Chords && l.Coord.Equals(songElementCoordinates3));

            _ = sw.AppendLine(
                    $"{_backslash}new ChordNames {_backslash}with " +
                    $"{_opening_bracket} " +
                    $"alignAboveContext = {_dblquote}melStaff_{segment.Index}_0{_dblquote} " +
                    $"{_closing_bracket} " +
                    $"{_opening_bracket} " +
                    $"{_backslash}{_lilypondSongChords[songElementChordsIndex].Name} " +
                    $"{_closing_bracket}"
                );

            _ = sw.AppendLine($">>"); //Fin de syst�me de port�es

            return sw.ToString();
        }
    }
}
