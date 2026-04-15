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


        public GlobalMelodyEditor() {
            MelodyMeasureEditor = new MelodyMeasureEditor();
            _rhythmEditor = new MeasureRhythmEditor();

            BuildUI();  
        }

        public void LoadFromModel(MeasureGlobalMelody globalMelody) {
            MelodyMeasureEditor.LoadFromModel(globalMelody.Melody.ToWidgetChords(), initialCursor: 0);
            _rhythmEditor.LoadFromModel(globalMelody);
        }

        private void BuildUI() {

            Box row = new(Orientation.Vertical, 0); //On met en superposé les elements qui définissent une mesure

            // Editeur de mélodie/cadence
            MelodyMeasureEditor = new();
            

            // Handler local : met à jour la measure associée (capture 'measure' et 'editor')
            MelodyMeasureEditor.ContentChanged += (s, e) => {
                MeasureMelody newMeasureMelody = new();
                newMeasureMelody.MelodyChords = new List<MelodyChord>();
                foreach (WidgetMelodyChord widgetChord in MelodyMeasureEditor.ExportToModel()) {
                    newMeasureMelody.MelodyChords.Add(widgetChord.ToMelodyChord());
                }
                MelodyChanged?.Invoke(newMeasureMelody);
                
            };

            MelodyMeasureEditor.NoteCountChanged += MelodyMeasureEditor_NoteCountChanged;



            MelodyMeasureEditor.WidthRequest = 250;
            row.PackStart(MelodyMeasureEditor, true, false, 0);

            MelodyMeasureEditor.ShowAll();

            _rhythmEditor = new();
            _rhythmEditor.PatternChanged += pattern => {
                PatternChanged?.Invoke(pattern);
                QueueDraw();
            };



            TimeSignature ts = new(4, 4); //TODO : faire en sorte que ce soit défini par la mesure et que ça puisse être modifié via l'interface             


            row.PackStart(_rhythmEditor, true, false, 0);

            Add(row);

            ShowAll();
        }

        private void MelodyMeasureEditor_NoteCountChanged(object? sender, int chordCount) {
            _rhythmEditor.CurrentMelodyChordsCount = chordCount;
        }
    }
}
