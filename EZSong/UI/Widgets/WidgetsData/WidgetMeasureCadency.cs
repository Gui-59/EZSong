using EZSong.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.WidgetsData {
    public class WidgetMeasureCadency {

        public List<RhythmEvent> CadencyElements;

        public WidgetMeasureCadency() {
            CadencyElements = new List<RhythmEvent>();
        }

        public WidgetMeasureCadency(List<RhythmEvent> cadency) {
            CadencyElements = cadency;
        }
    }
}
