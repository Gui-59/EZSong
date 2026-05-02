using EZSong.Model;

namespace EZSong.UI.Widgets.WidgetsData {
    public class WidgetMeasureCadency {

        public List<RhythmSimpleElement> CadencyElements;

        public WidgetMeasureCadency() {
            CadencyElements = new List<RhythmSimpleElement>();
        }

        public WidgetMeasureCadency(List<RhythmSimpleElement> cadency) {
            CadencyElements = cadency;
        }
    }
}
