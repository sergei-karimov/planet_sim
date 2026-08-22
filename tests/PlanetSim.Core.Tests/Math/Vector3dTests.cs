using PlanetSim.Core.Math;

namespace PlanetSim.Core.Tests.Math;

public sealed class Vector3dTests {
    [Fact]
    public void ArithmeticAndLengthUseDoublePrecision() {
        var a = new Vector3d(1, 2, 3);
        var b = new Vector3d(4, -2, 1);

        Assert.Equal(new Vector3d(5, 0, 4), a + b);
        Assert.Equal(new Vector3d(-3, 4, 2), a - b);
        Assert.Equal(new Vector3d(2, 4, 6), a * 2);
        Assert.Equal(14, a.LengthSquared);
        Assert.Equal(System.Math.Sqrt(14), a.Length, 12);
    }

    [Fact]
    public void FiniteAndInterpolationHelpersAreDeterministic() {
        Assert.True(Vector3d.Zero.IsFinite);
        Assert.False(new Vector3d(double.NaN, 0, 0).IsFinite);
        Assert.Equal(new Vector3d(2.5, 5, 7.5),
            Vector3d.Lerp(Vector3d.Zero, new Vector3d(10, 20, 30), 0.25));
    }
}
