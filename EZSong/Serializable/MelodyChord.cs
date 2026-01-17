using EZSong.UI.Widgets.WidgetsData;

namespace EZSong.Serializable {

    [Serializable]
    public class MelodyChord {

        public List<Pitch> Pitches;

        public MelodyChord() {
            //Contructeur par défaut (requis pour la sérialisation)
            Pitches = new ();
        }

        public MelodyChord(List<Pitch> pitches) {
            Pitches = pitches;
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
