using PlanetSim.App;

namespace PlanetSim.App.Tests.Simulation;

public sealed class LiveCompositionTests
{
    [Fact]
    public void DefaultCompositionAdvancesAllTenBodiesAndResetRestoresInitialSnapshot()
    {
        var controller = Composition.CreateController(maxStepsPerFrame: 2_000);
        var initial = controller.Status.Snapshot;
        controller.SetSlider(1);
        controller.Update(1);
        var advanced = controller.Status.Snapshot;
        Assert.Equal(10, advanced.Bodies.Count);
        Assert.True(advanced.Elapsed > TimeSpan.Zero);
        Assert.Contains(advanced.Bodies.Zip(initial.Bodies), pair =>
            pair.First.PositionMeters != pair.Second.PositionMeters);
        controller.Reset();
        Assert.Equal(initial.Bodies, controller.Status.Snapshot.Bodies);
    }
}
