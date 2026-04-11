using EZSong.Enums;
using EZSong.UI.Widgets.WidgetsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {

    public class MeasureMelody {

        public List<MelodyChord> MelodyChords
        {
            get;
            set;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public MeasureMelody() { 
            MelodyChords = new List<MelodyChord>();
        }

        public MeasureMelody(List<MelodyChord> melodyChords) {
            MelodyChords = melodyChords;
        }

        private bool HasCadency(MeasureData measureData) {
          
            if (measureData.RhythmPattern is null) {
                return false;
            } else {
                if (measureData.RhythmPattern.IsDurationValid()) {
                    return true;
                }
            }
            return false;
            
        }

        public string ToLilyPondString(MeasureData measureData) {

            string lilyPondString = string.Empty;

            if (!HasCadency(measureData) || !measureData.RhythmPattern.IsDurationValid()) { //TODO
                //Si pas de cadence, on considère que toutes les notes ont la même durée

                if (MelodyChords.Count == 0) {
                    //Silence de mesure complete
                    lilyPondString += "R1"; 
                } else {
                

                    lilyPondString += "\\tuplet " + MelodyChords.Count + "/1 { ";

                    foreach (MelodyChord melodyChord in MelodyChords) {

                        //On force le style d'affichage pour la note (retour auto à la normale)
                        lilyPondString += Environment.NewLine + " \\once \\override NoteHead.style = #'harmonic-black ";
                        lilyPondString += Environment.NewLine + " \\once \\override Stem.transparent = ##t ";
                        lilyPondString += Environment.NewLine + " \\once \\override Beam.transparent = ##t ";
                        lilyPondString += Environment.NewLine + " \\once \\override TupletBracket.transparent = ##t ";
                        lilyPondString += Environment.NewLine + " \\once \\override TupletNumber.transparent = ##t";

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
                }

            } else {

                int melodyChordIndex = 0;

                foreach (BeatPattern beat in measureData.RhythmPattern.Beats) {
                
                    foreach (RhythmElement rhythmEvent in beat.Elements) {

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

        internal MeasureMelodyDto ToDto() {

            List<MelodyChordDto> melodyChords = new();
            foreach (MelodyChord melodyChord in MelodyChords) {
                melodyChords.Add(melodyChord.ToDto());
            }

            MeasureMelodyDto dto = new() {
                MelodyChords = melodyChords
            };
            return dto;
        }

        public static MeasureMelody FromDto(MeasureMelodyDto melody) {
            
            List<MelodyChord> melodyChords = new();
            foreach (MelodyChordDto melodyChordDto in melody.MelodyChords) {
                melodyChords.Add(MelodyChord.FromDto(melodyChordDto));
            }

            MeasureMelody measureMelody = 
                new(
                    melodyChords
                );
            return measureMelody;

        }
    }
}
