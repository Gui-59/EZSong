namespace EZSong.Model {

    public class RhythmTuplet: IRhythmElement {
        public List<RhythmRationalDuration> Subdivisions {
            get;
        }
        private RhythmRationalDuration _globalDuration;

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public RhythmTuplet() {
            Subdivisions = new List<RhythmRationalDuration>();
            _globalDuration = new RhythmRationalDuration(1, 1, 0);
        }

        public RhythmTuplet(List<RhythmRationalDuration> subdivisions, RhythmRationalDuration globalDuration) {
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
    }

}