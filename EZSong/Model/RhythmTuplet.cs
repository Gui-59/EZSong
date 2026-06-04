namespace EZSong.Model {

    public class RhythmTuplet: IRhythmElement {
        public List<RhythmSimpleElement> Subdivisions {
            get;
        }
        private RhythmRationalDuration _globalDuration;

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public RhythmTuplet() {
            Subdivisions = new List<RhythmSimpleElement>();
            _globalDuration = new RhythmRationalDuration(1, 1, 0);
        }

        public RhythmTuplet(List<RhythmSimpleElement> subdivisions, RhythmRationalDuration globalDuration) {
            Subdivisions = subdivisions;
            _globalDuration = globalDuration;
        }

        public RhythmRationalDuration GetEffectiveDuration() {
            return _globalDuration;
        }

        public int DotCount() {
            return 0;
        }

        public bool IsTiedToNext() {
            return false;
        }

        public bool IsRest() {
            return false;
        }


        public static RhythmTuplet FromDto(RhythmTupletDto rhythmTypletDto) {
            if (rhythmTypletDto == null) {
                return new RhythmTuplet();
            }
            return new RhythmTuplet(rhythmTypletDto.Subdivisions, rhythmTypletDto.GlobalDuration);
        }

        internal static RhythmTupletDto ToDto(RhythmTuplet rhythmTuplet) {
            if (rhythmTuplet == null) {
                return new RhythmTupletDto(new List<RhythmSimpleElement>(), new RhythmRationalDuration(1, 1, 0));
            }
            return new RhythmTupletDto(rhythmTuplet.Subdivisions, rhythmTuplet.GetEffectiveDuration());

        }
    }

}