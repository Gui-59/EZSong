namespace EZSong {

    public class RhythmPattern {
        public List<RhythmElement> Elements { get; } = new();

        public RhythmRationalDuration GetTotalDuration() {
            RhythmRationalDuration total = new(0, 1);

            foreach (RhythmElement e in Elements) {
                total += e.GetEffectiveDuration();
            }

            return total;
        }

        public int AttackCount {
            get {
                return Elements.Count(e => !e.IsRest);
            }
        }

        public override string ToString() {
            List<string> parts = new();

            int i = 0;

            while (i < Elements.Count) {
                RhythmElement e = Elements[i];

                if (e.Tuplet == null) {
                    parts.Add(ElementToString(e));
                    i++;
                    continue;
                }

                // Gestion d'un groupe de tuplet
                RhythmTuplet tuplet = e.Tuplet;

                List<string> tupletElements = new();

                int count = tuplet.Count;

                for (int j = 0; j < count && (i + j) < Elements.Count; j++) {
                    tupletElements.Add(ElementToString(Elements[i + j]));
                }

                parts.Add($"({tuplet.Count}:{tuplet.InTimeOf} {string.Join(" ", tupletElements)})");

                i += count;
            }

            return string.Join(" ", parts);
        }

        private string ElementToString(RhythmElement e) {
            string symbol = DurationToSymbol(e.Duration);

            if (e.IsRest) {
                symbol = "r" + symbol;
            }

            return symbol;
        }

        private string DurationToSymbol(RhythmRationalDuration d) {
            string baseSymbol = d.Denominator switch {
                1 => "𝅝",   // ronde
                2 => "𝅗𝅥",   // blanche
                4 => "♩",
                8 => "♪",
                16 => "𝅘𝅥𝅯",
                32 => "𝅘𝅥𝅰",
                _ => $"1/{d.Denominator}"
            };

            if (d.Dots == 0) {
                return baseSymbol;
            }

            return baseSymbol + new string('.', d.Dots);
        }
    }

}