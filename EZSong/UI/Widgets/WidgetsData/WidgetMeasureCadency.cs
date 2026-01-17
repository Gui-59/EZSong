using EZSong.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets.WidgetsData {
    public class WidgetMeasureCadency {

        public List<CadencyElement> CadencyElements;

        public WidgetMeasureCadency() {
            CadencyElements = new List<CadencyElement>();
        }

        public WidgetMeasureCadency(List<CadencyElement> cadency) {
            CadencyElements = cadency;
        }
    }
}
