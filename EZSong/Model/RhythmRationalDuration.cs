namespace EZSong.Model {

    public struct RhythmRationalDuration : IEquatable<RhythmRationalDuration> {
        public int Numerator {
            get;
        }
        public int Denominator {
            get;
        }
        public int Dots {
            get;
        }

        public RhythmRationalDuration(int numerator, int denominator, int dots) {
            Numerator = numerator;
            Denominator = denominator;
            Dots = dots;
        }

        public RhythmRationalDuration ApplyDots() {
            if (Dots == 0) {
                return this;
            }

            int num = Numerator;
            int den = Denominator;

            int factorNum = (int)Math.Pow(2, Dots + 1) - 1;
            int factorDen = (int)Math.Pow(2, Dots);

            return new RhythmRationalDuration(num * factorNum, den * factorDen, 0).Normalize();
        }

        public static RhythmRationalDuration operator +(
            RhythmRationalDuration a,
            RhythmRationalDuration b) {
            a = a.ApplyDots();
            b = b.ApplyDots();

            int num =
                a.Numerator * b.Denominator +
                b.Numerator * a.Denominator;

            int den =
                a.Denominator * b.Denominator;

            return new RhythmRationalDuration(num, den, 0).Normalize();
        }

        public RhythmRationalDuration Normalize() {


            int gcd = Gcd(Numerator, Denominator);
            if (gcd == 0) {
                throw new InvalidOperationException("Cannot normalize a duration with zero numerator and denominator.");
            }

            return new RhythmRationalDuration(Numerator / gcd, Denominator / gcd, Dots);
        }

        private static int Gcd(int a, int b) {
            while (b != 0) {
                (a, b) = (b, a % b);
            }

            return Math.Abs(a);
        }

        public bool Equals(RhythmRationalDuration other) {

            if (other.Numerator == 0 && other.Denominator == 0) {
                throw new InvalidOperationException("Cannot compare a duration with zero numerator and denominator.");
            }

            RhythmRationalDuration a = ApplyDots().Normalize();
            RhythmRationalDuration b = other.ApplyDots().Normalize();

            return
                a.Numerator == b.Numerator &&
                a.Denominator == b.Denominator;
        }

        public string ToLilyPondString() {
            string s = Denominator.ToString();

            if (Dots == 1) {
                s += ".";
            }

            if (Dots == 2) {
                s += "..";
            }

            return s;
        }
    }
}