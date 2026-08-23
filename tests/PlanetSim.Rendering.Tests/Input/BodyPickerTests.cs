using System.Numerics;
using PlanetSim.Core.Model;
using PlanetSim.Rendering.Input;

namespace PlanetSim.Rendering.Tests.Input;

public sealed class BodyPickerTests
{
    [Fact]
    public void PickReturnsNearestPositiveSphereIntersection()
    {
        var ray = new Ray3(Vector3.Zero, Vector3.UnitZ);
        PickSphere[] candidates =
        [
            new(BodyId.Mars, new Vector3(0, 0, 10), 2),
            new(BodyId.Earth, new Vector3(0, 0, 5), 1)
        ];
        Assert.Equal(BodyId.Earth, BodyPicker.Pick(ray, candidates));
    }

    [Fact]
    public void PickHandlesOriginInsideSphereAndMissesBehindRay()
    {
        Assert.Equal(BodyId.Earth, BodyPicker.Pick(
            new Ray3(Vector3.Zero, Vector3.UnitX),
            [new PickSphere(BodyId.Earth, Vector3.Zero, 2)]));
        Assert.Null(BodyPicker.Pick(
            new Ray3(Vector3.Zero, Vector3.UnitZ),
            [new PickSphere(BodyId.Mars, new Vector3(0, 0, -5), 1)]));
    }
}
