using EZSong.Model;

namespace EZSong.UI.Widgets.WidgetsData {
    public class WidgetMeasureCadency {

        public List<RhythmElement> CadencyElements;

        public WidgetMeasureCadency() {
            CadencyElements = new List<RhythmElement>();
        }

        public WidgetMeasureCadency(List<RhythmElement> cadency) {
            CadencyElements = cadency;
        }
    }
}
