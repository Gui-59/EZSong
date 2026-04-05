namespace EZSong.Model {

    public class RhythmTuplet {
        public int Count {
            get;
        }
        public int InTimeOf {
            get;
        }

        //Constructeur vide (requis pour la (dé)sérialisation JSON)
        public RhythmTuplet() {
            Count = 1;
            InTimeOf = 1;
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