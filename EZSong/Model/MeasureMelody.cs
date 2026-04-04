using EZSong.Enums;
using EZSong.UI.Widgets.WidgetsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {


    public class MeasureMelody {

        public List<MelodyChord> MelodyChords;
        public MeasureRhythmPattern RhythmPattern;

        public bool HasCadency {
            get {
                if (RhythmPattern is null) {
                    return false;
                } else {
                    if (RhythmPattern.IsDurationValid()) {
                        return true;
                    }
                }
                return false;
            }
        }

        public MeasureMelody(List<MelodyChord> melodyChords, MeasureRhythmPattern rhythmPattern) {
            MelodyChords = melodyChords;
            RhythmPattern = rhythmPattern;
        }

        public string ToLilyPondString() {

            string lilyPondString = string.Empty;

            if (!HasCadency) {
                //Si pas de cadence, on considère que toutes les notes ont la même durée

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

            } else {

                int melodyChordIndex = 0;

                foreach (BeatPattern beat in RhythmPattern.Beats) {
                

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
            MeasureMelodyDto dto = new() {
                MelodyChords = MelodyChords.Select(mc => mc.ToDto()).ToList(),
                RhythmPattern = RhythmPattern.ToDto()
            };
            return dto;

        }

        public static MeasureMelody FromDto(MeasureMelodyDto melody) {
            
            List<MelodyChord> melodyChords = new();
            foreach (MelodyChordDto melodyChordDto in melody.MelodyChords) {
                melodyChords.Add(MelodyChord.FromDto(melodyChordDto));
            }

            MeasureMelody measureMelody = new(melodyChords, MeasureRhythmPattern.FromDto(melody.RhythmPattern, melody.RhythmPattern.TimeSignature));
            return measureMelody;

        }
    }
}
