using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public class SegmentData {

        public int Index {
            get;
            set;
        }

        public SongSettings SongSettings {
            get;
            set;
        }

        public List<MeasureData> Measures {
            get;
            set;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public SegmentData() {
            SongSettings = new SongSettings();
            Measures = new List<MeasureData>();
            List<MeasureGlobalMelody> measureStaffs = new();
            measureStaffs.Add(new MeasureGlobalMelody(0)); //Toujours au moins une portée par mesure
            Measures.Add(new MeasureData(0, SongSettings, new TimeSignature(), new KeySignature(), new ChordSequence(), measureStaffs, string.Empty)); // Toujours au moins une mesure
        }
        public SegmentData(int index, SongSettings songSettings) {
            Index = index;
            SongSettings = songSettings;
            Measures = new List<MeasureData>();
            List<MeasureGlobalMelody> measureStaffs = new();
            for (int i = 0; i < songSettings.StaffsSettings.Staffs.Count; i++) {
                measureStaffs.Add(new MeasureGlobalMelody(i)); //Toujours au moins une portée par mesure
            }
            Measures.Add(new MeasureData(0, SongSettings, new TimeSignature(), new KeySignature(), new ChordSequence(), measureStaffs, string.Empty)); // Toujours au moins une mesure


        }

        public SegmentData(int index, SongSettings songSettings, List<MeasureData> measures) {
            Index = index;
            SongSettings = songSettings;
            Measures = measures;
        }

        public SegmentDataDto ToDto() {

            List<MeasureDataDto> measures = new();
            foreach (MeasureData measure in Measures) {
                measures.Add(measure.ToDto());
            }

            return
                new SegmentDataDto(
                    Index,
                    measures
                );
        }

        public static SegmentData FromDto(SegmentDataDto dto, SongSettings songSettings) {

            List<MeasureData> measures = new();
            foreach (MeasureDataDto measure in dto.MeasureData) {
                measures.Add(MeasureData.FromDto(measure, songSettings));
            }

            SegmentData segment =
                new(
                    dto.Index,
                    songSettings,
                    measures
                );

            return segment;
        }

        internal void AddOrRemoveMesuresStaffs(int excpectedStaffCount) {
            foreach (MeasureData measure in Measures) {
                measure.AddOrRemoveStaffs(excpectedStaffCount);
            }
        }
    }
}
