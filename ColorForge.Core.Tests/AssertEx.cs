using ColorForge.Core;

internal static class AssertEx {
    public static void NearlyEqual(
        double expected,
        double actual,
        double epsilon = 1e-6) {
        Assert.True(
            Math.Abs(expected - actual) <= epsilon,
            $"Expected {expected}, actual {actual}");
    }

    public static void NearlyEqual(
    LinearRgbColor expected,
    LinearRgbColor actual,
    double epsilon = 1e-6) {
        NearlyEqual(expected.R, actual.R, epsilon );
        NearlyEqual(expected.G, actual.G, epsilon );
        NearlyEqual(expected.B, actual.B, epsilon );
    }
}