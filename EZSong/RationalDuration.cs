using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZSong {
    public readonly struct RationalDuration : IEquatable<RationalDuration> {
        public int Numerator {
            get;
        }
        public int Denominator {
            get;
        }

        public RationalDuration(int numerator, int denominator) {
            Numerator = numerator;
            Denominator = denominator;
        }

        public static RationalDuration operator +(RationalDuration a, RationalDuration b)
            => new RationalDuration(
                a.Numerator * b.Denominator + b.Numerator * a.Denominator,
                a.Denominator * b.Denominator
            ).Normalize();

        public RationalDuration Normalize() {
            int gcd = Gcd(Numerator, Denominator);
            return new RationalDuration(Numerator / gcd, Denominator / gcd);
        }

        private static int Gcd(int a, int b) {
            while (b != 0) {
                (a, b) = (b, a % b);
            }

            return Math.Abs(a);
        }

        public bool Equals(RationalDuration other) {
            return Normalize().Numerator == other.Normalize().Numerator &&
            Normalize().Denominator == other.Normalize().Denominator;
        }
    }

}
