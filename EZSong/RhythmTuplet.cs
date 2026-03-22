namespace EZSong {

    public class RhythmTuplet {
        public int Count {
            get;
        }
        public int InTimeOf {
            get;
        }

        public RhythmTuplet(int count, int inTimeOf) {
            Count = count;
            InTimeOf = inTimeOf;
        }

        public double Ratio {
            get {
                return (double)InTimeOf / Count;
            }
        }
    }

}