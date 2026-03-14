using EZSong.Enums;
using EZSong.UI.Widgets.WidgetsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Serializable {
    [Serializable]
    public class MeasureMelody {

        public List<MelodyChord> MelodyChords;
        public List<RhythmEvent> Cadency;

        public bool HasCadency {
            get {
                if (Cadency is null) {
                    return false;
                } else {
                    if (Cadency.Count() > 0) {
                        return true;
                    }
                }
                return false;
            }
        }

        public MeasureMelody() {
            MelodyChords = new();
            Cadency = new();
        }

        public string ToLilyPondString() {

            string lilyPondString = string.Empty;

            if (!HasCadency) {
                //Si pas de cadence, on considère que toutes les notes ont la même durée

                lilyPondString += "\\tuplet "+ MelodyChords.Count+ "/1 { ";

                foreach (MelodyChord melodyChord in MelodyChords) {

                    //On force le style d'affichage pour la note (retour auto à la normale)
                    lilyPondString += Environment.NewLine + " \\once \\override NoteHead.style = #'harmonic-black ";
                    lilyPondString += Environment.NewLine + " \\once \\override Stem.transparent = ##t " ;
                    lilyPondString += Environment.NewLine + " \\once \\override Beam.transparent = ##t " ;
                    lilyPondString += Environment.NewLine + " \\once \\override TupletBracket.transparent = ##t " ;
                    lilyPondString += Environment.NewLine + " \\once \\override TupletNumber.transparent = ##t" ;

                    lilyPondString += Environment.NewLine;

                    if (melodyChord.Pitches.Count == 1) {
                        lilyPondString += melodyChord.Pitches[0].ToLilyPondString();
                        lilyPondString += "1";//Durée fixe
                        lilyPondString += " ";
                    } else {
                        //Dans Lilypond les notes d'un accords sont entre chevrons
                        lilyPondString += " < ";
                        foreach (Pitch pitch in melodyChord.Pitches) {
                            lilyPondString += pitch.ToLilyPondString();
                            
                            lilyPondString += " ";
                        }
                        lilyPondString += " > ";
                        lilyPondString += "1";//Durée fixe
                        lilyPondString += " ";
                    }
                }

                lilyPondString += "}";

            } else {

                int melodyChordIndex = 0;
                foreach (RhythmEvent rhythmEvent in Cadency) {
                    if (rhythmEvent.IsRest) {
                        lilyPondString += "r";
                        lilyPondString += rhythmEvent.Duration.ToLilyPondString();

                    } else {                     


                        if (MelodyChords[melodyChordIndex].Pitches.Count == 1) {
                            lilyPondString += MelodyChords[melodyChordIndex].Pitches[0].ToLilyPondString();
                            lilyPondString += rhythmEvent.Duration.ToLilyPondString();
                            lilyPondString += " ";
                        } else {
                            //Dans Lilypond les notes d'un accords sont entre chevrons
                            lilyPondString += " < ";
                            foreach (Pitch pitch in MelodyChords[melodyChordIndex].Pitches) {
                                lilyPondString += pitch.ToLilyPondString();

                                lilyPondString += " ";
                            }
                            lilyPondString += " > ";
                            lilyPondString += rhythmEvent.Duration.ToLilyPondString();
                            lilyPondString += " ";
                        }

                        melodyChordIndex++;
                    }
                }
            }

            return lilyPondString;
        }

        internal IEnumerable<WidgetMelodyChord> ToWidgetChords() {
            List<WidgetMelodyChord> widgetChords = new();

            foreach (MelodyChord melodyChord in MelodyChords) {
                widgetChords.Add(melodyChord.ToWidgetMelodyChord());
            }

            return widgetChords;
        }
    }
}
