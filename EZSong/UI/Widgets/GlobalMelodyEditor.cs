using EZSong.Model;
using EZSong.UI.Widgets.WidgetsData;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.UI.Widgets {
    public class GlobalMelodyEditor : Frame {


        public MelodyMeasureEditor MelodyMeasureEditor {
            get; 
            private set;
        }
        private MeasureRhythmEditor _rhythmEditor;

        public event Action<MeasureMelody>? MelodyChanged;
        public event Action<MeasureRhythmPattern>? PatternChanged;


        public GlobalMelodyEditor(MeasureData measureData) {
            MelodyMeasureEditor = new();
            MelodyMeasureEditor.LoadFromModel(measureData, initialCursor: 0);
            _rhythmEditor = new();
            _rhythmEditor.LoadFromModel(measureData);
            BuildUI();
        }

        private void BuildUI() {

            Box row = new(Orientation.Vertical, 0); //On met en superposé les elements qui définissent une mesure
            

            // Handler local : met à jour la measure associée (capture 'measure' et 'editor')
            MelodyMeasureEditor.ContentChanged += (s, e) => {

                

                MeasureMelody newMeasureMelody = new();
                newMeasureMelody.MelodyChords = new List<MelodyChord>();
                foreach (WidgetMelodyChord widgetChord in MelodyMeasureEditor.ExportToModel()) {
                    newMeasureMelody.MelodyChords.Add(widgetChord.ToMelodyChord());
                }
                MelodyChanged?.Invoke(newMeasureMelody);
                
            };

            MelodyMeasureEditor.WidthRequest = 250;
            row.PackStart(MelodyMeasureEditor, true, false, 0);

            MelodyMeasureEditor.ShowAll();

            _rhythmEditor.PatternChanged += pattern => {
                PatternChanged?.Invoke(pattern);
                QueueDraw();
            };

            row.PackStart(_rhythmEditor, true, false, 0);

            Add(row);

            ShowAll();
        }

        internal void UpdateTimeSignature(TimeSignature timeSignature) {
            _rhythmEditor.UpdateTimeSignature(timeSignature);
        }
    }
}
