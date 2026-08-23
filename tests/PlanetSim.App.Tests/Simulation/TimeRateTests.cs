using PlanetSim.App.Simulation;

namespace PlanetSim.App.Tests.Simulation;

public sealed class TimeRateTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2_592_000)]
    public void SliderMapsEndpointsToSecondsPerSecond(double slider, double expected) =>
        Assert.Equal(expected, TimeRate.FromSlider(slider).SimulatedSecondsPerRealSecond, 6);

    [Fact]
    public void SliderMidpointIsGeometricMean() =>
        Assert.Equal(System.Math.Sqrt(2_592_000),
            TimeRate.FromSlider(0.5).SimulatedSecondsPerRealSecond, 6);

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(2, 1)]
    public void SliderIsClamped(double input, double expected) =>
        Assert.Equal(expected, TimeRate.FromSlider(input).SliderValue);
}
