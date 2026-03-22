using EZSong.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
