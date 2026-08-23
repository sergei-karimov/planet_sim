using PlanetSim.App.Simulation;
using PlanetSim.Core.Model;
using PlanetSim.Core.Physics;

namespace PlanetSim.App.Tests.Simulation;

public sealed class SimulationControllerTests
{
    [Fact]
    public void UpdateCapsWorkAndReportsEffectiveRate()
    {
        var engine = new FakeEngine();
        var controller = new SimulationController(engine, maxStepsPerFrame: 10);
        controller.SetSlider(1);
        controller.Update(1.0 / 60.0);
        Assert.Equal(10, engine.StepCalls);
        Assert.True(controller.Status.EffectiveSecondsPerRealSecond
            < controller.Status.RequestedSecondsPerRealSecond);
    }

    [Fact]
    public void PausePreventsAdvancementAndResetRestoresDefaults()
    {
        var engine = new FakeEngine();
        var controller = new SimulationController(engine, 100);
        controller.TogglePause();
        controller.Update(60);
        Assert.Equal(0, engine.StepCalls);
        controller.Select(BodyId.Earth);
        controller.Reset();
        Assert.Equal(1, engine.ResetCalls);
        Assert.False(controller.Status.IsPaused);
        Assert.Null(controller.Status.SelectedBody);
    }

    [Fact]
    public void InvalidPhysicsPausesAndKeepsLastValidSnapshot()
    {
        var engine = new FakeEngine { ThrowOnStep = true };
        var controller = new SimulationController(engine, 100);
        var valid = controller.Status.Snapshot;
        controller.SetSlider(1);
        controller.Update(1);
        Assert.True(controller.Status.IsPaused);
        Assert.Same(valid, controller.Status.Snapshot);
        Assert.NotNull(controller.Status.ErrorMessage);
    }

    private sealed class FakeEngine : ISimulationEngine
    {
        private readonly SimulationSnapshot _snapshot = new(TimeSpan.Zero, []);
        public double FixedStepSeconds => 1_800;
        public TimeSpan Elapsed => TimeSpan.FromSeconds(StepCalls * FixedStepSeconds);
        public int StepCalls { get; private set; }
        public int ResetCalls { get; private set; }
        public bool ThrowOnStep { get; init; }
        public void Step()
        {
            if (ThrowOnStep) throw new InvalidOperationException("broken physics");
            StepCalls++;
        }
        public void Reset() { ResetCalls++; StepCalls = 0; }
        public SimulationSnapshot CreateSnapshot() => _snapshot with { Elapsed = Elapsed };
    }
}
