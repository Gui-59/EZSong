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

        public int StaffIndex {
            get;
            set;
        }
        public List<MelodyChord> MelodyChords
        {
            get;
            set;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public MeasureMelody() {
            StaffIndex = 0; //Par défaut
            MelodyChords = new List<MelodyChord>();
        }

        public MeasureMelody(int staffIndex) {
            StaffIndex = staffIndex;
            MelodyChords = new List<MelodyChord>();
        }

        public MeasureMelody(int staffIndex, List<MelodyChord> melodyChords) {
            StaffIndex = staffIndex;
            MelodyChords = melodyChords;
        }

        

        public string ToLilyPondString(MeasureData measureData) {

            string lilyPondString = string.Empty;

            //Définition de l'octave de référence (basé sur l'octave de base de la portée)
            int baseOctave = measureData.SongSettings.StaffsSettings.GetStaffBaseOctave(StaffIndex); 
            switch (baseOctave) {
                case 6:
                    lilyPondString += "\\fixed c'' {";
                    break;
                case 5:
                    lilyPondString += "\\fixed c' {";
                    break;
                case 4:
                    lilyPondString += "\\fixed c {";
                    break;
                case 3:
                    lilyPondString += "\\fixed c, {";
                    break;
                case 2:
                    lilyPondString += "\\fixed c,, {";
                    break;
            }

            bool hasValidCadency = measureData.Staffs[StaffIndex].Pattern.HasValidCadency(this, measureData.PrecedingMeasure);
            bool hasValidNoteCount = measureData.Staffs[StaffIndex].Pattern.IsCompatibleWithNoteCount(this, measureData.PrecedingMeasure);

            //Si la mesure précédente se terminait par une liaison,
            //alors le premier "MelodyChord" de la mesure actuelle doit être
            //le dernier "MelodyChord" de la mesure précédente
            //(comme si on l'avais ressaisie dans la mesure actuelle).
            List<MelodyChord> consideredMelodyChords = MelodyChords;
            if (measureData.PrecedingMeasure != null && measureData.PrecedingMeasure.Staffs[StaffIndex].Pattern.EndsWithTie()) {

                consideredMelodyChords = new();
                consideredMelodyChords.Add(measureData.PrecedingMeasure.Staffs[StaffIndex].Melody.MelodyChords.Last());
                foreach (MelodyChord melodyChord in MelodyChords) {
                    consideredMelodyChords.Add(melodyChord);
                }
            }

            if (!hasValidCadency || !hasValidNoteCount) { 
                //Si pas de cadence ou si le nombre de note est incompatible avec la cadence,
                //alors on considère que toutes les notes ont la même durée

                if (consideredMelodyChords.Count == 0) {
                    //Silence de mesure complete
                    lilyPondString += "R1"; 
                } else {
                

                    lilyPondString += "\\tuplet " + consideredMelodyChords.Count + "/1 { ";

                    foreach (MelodyChord melodyChord in consideredMelodyChords) {

                        //On force le style d'affichage pour la note (retour auto à la normale)
                        lilyPondString += Environment.NewLine + " \\once \\override NoteHead.style = #'harmonic-black ";
                        lilyPondString += Environment.NewLine + " \\once \\override Stem.transparent = ##t ";
                        lilyPondString += Environment.NewLine + " \\once \\override Beam.transparent = ##t ";
                        lilyPondString += Environment.NewLine + " \\once \\override TupletBracket.transparent = ##t ";
                        lilyPondString += Environment.NewLine + " \\once \\override TupletNumber.transparent = ##t";

                        lilyPondString += Environment.NewLine;

                        if (melodyChord.Pitches.Count == 1) {
                            lilyPondString += melodyChord.Pitches[0].ToLilyPondString(baseOctave);
                            lilyPondString += "1";//Durée fixe
                            lilyPondString += " ";
                        } else {
                            //Dans Lilypond les notes d'un accords sont entre chevrons
                            lilyPondString += " < ";
                            foreach (Pitch pitch in melodyChord.Pitches) {
                                lilyPondString += pitch.ToLilyPondString(baseOctave);

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
                

                foreach (BeatPattern beat in measureData.Staffs[StaffIndex].Pattern.Beats) {

                    int rhythmElementIndex = 0;
                    foreach (IRhythmElement rhythmElement in beat.Elements) {

                        if (rhythmElement.GetType() == typeof(RhythmTuplet)) {

                            RhythmTuplet rhythmTuplet = (RhythmTuplet)rhythmElement;
                            lilyPondString += "\\tuplet " + rhythmTuplet.Subdivisions.Count + "/1 { ";
                            foreach (RhythmSimpleElement rhythmSimpleElement in rhythmTuplet.Subdivisions) {
                                if (rhythmSimpleElement.IsRest()) {
                                    lilyPondString += "r";
                                    lilyPondString += rhythmSimpleElement.GetEffectiveDuration().ToLilyPondString();
                                } else {
                                    if (consideredMelodyChords[melodyChordIndex].Pitches.Count == 1) {
                                        lilyPondString += consideredMelodyChords[melodyChordIndex].Pitches[0].ToLilyPondString(baseOctave);
                                        lilyPondString += rhythmSimpleElement.GetEffectiveDuration().ToLilyPondString();
                                        lilyPondString += " ";
                                    } else {
                                        //Dans Lilypond les notes d'un accords sont entre chevrons
                                        lilyPondString += " < ";
                                        foreach (Pitch pitch in consideredMelodyChords[melodyChordIndex].Pitches) {
                                            lilyPondString += pitch.ToLilyPondString(baseOctave);
                                            lilyPondString += " ";
                                        }
                                        lilyPondString += " > ";
                                        lilyPondString += rhythmSimpleElement.GetEffectiveDuration().ToLilyPondString();
                                        lilyPondString += " ";
                                    }
                                    if (beat.Elements.Count() > rhythmElementIndex + 1 && beat.Elements[rhythmElementIndex+1].GetType() == typeof(RhythmTieFrom))                                    {
                                        //On ne passe pas à la note suivante si la note est liée à la suivante
                                    } else {
                                        melodyChordIndex++;
                                    }
                                }
                            }
                            lilyPondString += "}";

                        } else if (rhythmElement.GetType() == typeof(RhythmSimpleElement)) {
                            RhythmSimpleElement rhythmSimpleElement = (RhythmSimpleElement)rhythmElement;


                            if (rhythmSimpleElement.IsRest()) {
                                lilyPondString += "r";
                                lilyPondString += rhythmSimpleElement.GetEffectiveDuration().ToLilyPondString();

                            } else {

                                if (consideredMelodyChords[melodyChordIndex].Pitches.Count == 1) {
                                    lilyPondString += consideredMelodyChords[melodyChordIndex].Pitches[0].ToLilyPondString(baseOctave);
                                    lilyPondString += rhythmSimpleElement.GetEffectiveDuration().ToLilyPondString();
                                    lilyPondString += " ";
                                } else {
                                    //Dans Lilypond les notes d'un accords sont entre chevrons
                                    lilyPondString += " < ";
                                    foreach (Pitch pitch in consideredMelodyChords[melodyChordIndex].Pitches) {
                                        lilyPondString += pitch.ToLilyPondString(baseOctave);

                                        lilyPondString += " ";
                                    }
                                    lilyPondString += " > ";
                                    lilyPondString += rhythmSimpleElement.GetEffectiveDuration().ToLilyPondString();
                                    lilyPondString += " ";
                                }

                                if (beat.Elements.Count() > rhythmElementIndex + 1 && beat.Elements[rhythmElementIndex + 1].GetType() == typeof(RhythmTieFrom)) {
                                    //On ne passe pas à la note suivante si la note est liée à la suivante
                                } else {
                                    melodyChordIndex++;
                                }
                            }
                        } else if (rhythmElement.GetType() == typeof(RhythmTieFrom)) {
                            //Pour créer une liaison de prolongation – parfois aussi appelée liaison de tenue –, on ajoute un tilde '~' à la première note liée. 
                            lilyPondString += "~ ";
                        }

                        rhythmElementIndex++;
                    }
                }
            }

            //Fin Définition de l'octave de référence
            lilyPondString += "}";

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

            MeasureMelodyDto dto = new(StaffIndex, melodyChords);
            return dto;
        }

        public static MeasureMelody FromDto(MeasureMelodyDto melody) {
            
            List<MelodyChord> melodyChords = new();
            foreach (MelodyChordDto melodyChordDto in melody.MelodyChords) {
                melodyChords.Add(MelodyChord.FromDto(melodyChordDto));
            }

            MeasureMelody measureMelody = 
                new(
                    melody.StaffIndex,
                    melodyChords
                );
            return measureMelody;

        }
    }
}
