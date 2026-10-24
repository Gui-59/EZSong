using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong.Model {
    public struct SongElementCoordinates {
        
        public int SegmentIndex;
        public int? StaffIndex;


        public SongElementCoordinates(int segmentIndex, int? staffIndex) {
            SegmentIndex = segmentIndex;
            StaffIndex = staffIndex;            
        }
    }
}
