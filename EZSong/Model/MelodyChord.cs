using EZSong.UI.Widgets.WidgetsData;

namespace EZSong.Model {

    public class MelodyChord {

        public List<Pitch> Pitches { 
            get; 
            set; 
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public MelodyChord() {
            Pitches = new List<Pitch>();
        }

        public MelodyChord(List<Pitch> pitches) {
            Pitches = pitches;
        }

        public static MelodyChord FromDto(MelodyChordDto melodyChordDto) {
            List<Pitch> pitches = new();
            foreach (PitchDto pitchDto in melodyChordDto.Pitches) {
                pitches.Add(new Pitch(pitchDto.Note, pitchDto.Alteration, pitchDto.MidiOctave));
            }
            return new MelodyChord(pitches);

        }

        internal MelodyChordDto ToDto() {
            List<PitchDto> pitchDtos = new();
            foreach (Pitch pitch in Pitches) {
                pitchDtos.Add(pitch.ToDto());
            }
            return new MelodyChordDto() {
                Pitches = pitchDtos
            };

        }

        internal WidgetMelodyChord ToWidgetMelodyChord() {
            WidgetMelodyChord widgetChord = new();

            List<WidgetPitch> widgetPitches = new();
            foreach (Pitch pitch in Pitches) {
                widgetPitches.Add(pitch.ToWidgetPitch());
            }

            widgetChord.Pitches = widgetPitches;
            return widgetChord;
        }
    }
}
