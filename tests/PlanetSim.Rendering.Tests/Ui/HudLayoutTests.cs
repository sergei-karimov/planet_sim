using PlanetSim.Rendering.Ui;

namespace PlanetSim.Rendering.Tests.Ui;

public sealed class HudLayoutTests
{
    [Theory]
    [InlineData(10, 20, true)]
    [InlineData(110, 60, true)]
    [InlineData(111, 60, false)]
    public void RectangleEdgesAreInclusive(float x, float y, bool expected) =>
        Assert.Equal(expected, new UiRect(10, 20, 100, 40).Contains(new UiPoint(x, y)));

    [Theory]
    [InlineData(-10, 0)]
    [InlineData(50, 0.5)]
    [InlineData(110, 1)]
    public void SliderValueClampsToRange(float x, double expected) =>
        Assert.Equal(expected, HudLayout.SliderValue(new UiRect(0, 0, 100, 20), x), 12);

    [Fact]
    public void HiddenHelpDoesNotConsumePointer() =>
        Assert.False(HudLayout.HelpConsumesPointer(false, new UiPoint(10, 10), new UiRect(0, 0, 100, 100)));
}
