

namespace EZSong.Model {
    public class RhythmTupletDto {
        public int Count {
            get; set;
        }
        public int InTimeOf {
            get; set;
        }

        public RhythmTupletDto(int count, int inTimeOf) {
            Count = count;
            InTimeOf = inTimeOf;
        }

        public static RhythmTuplet FromDto(RhythmTupletDto tuplet) {
            if (tuplet == null) {
                return new RhythmTuplet(1, 1);
            }
            return new RhythmTuplet(tuplet.Count, tuplet.InTimeOf);

        }

        internal static RhythmTupletDto ToDto(RhythmTuplet tuplet) {
            if (tuplet == null) {
                return new RhythmTupletDto(1, 1);
            }
            return new RhythmTupletDto(tuplet.Count, tuplet.InTimeOf);
        }
    }
}