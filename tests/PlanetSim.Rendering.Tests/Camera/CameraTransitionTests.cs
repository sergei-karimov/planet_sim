using System.Numerics;
using PlanetSim.Rendering.Camera;

namespace PlanetSim.Rendering.Tests.Camera;

public sealed class CameraTransitionTests
{
    [Fact]
    public void TransitionStartsAtCurrentTargetAndEndsAtDestination()
    {
        var transition = new CameraTransition(Vector3.Zero, new Vector3(10, 0, 0), 0.4f);
        Assert.Equal(Vector3.Zero, transition.Evaluate(0));
        Assert.Equal(new Vector3(10, 0, 0), transition.Evaluate(0.4f));
        Assert.Equal(new Vector3(10, 0, 0), transition.Evaluate(1));
    }

    [Fact]
    public void ControllerFollowsBodyAndCanReturnToOverview()
    {
        var controller = new CameraController();
        controller.Follow(new Vector3(10, 0, 0));
        controller.UpdateTarget(new Vector3(12, 0, 0), 1);
        Assert.Equal(CameraMode.FollowBody, controller.Mode);
        Assert.True(controller.Target.X > 0);
        controller.ReturnToSystemView();
        controller.UpdateTarget(Vector3.Zero, 1);
        Assert.Equal(CameraMode.SystemOverview, controller.Mode);
    }
}
