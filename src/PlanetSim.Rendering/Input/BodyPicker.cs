using System.Numerics;
using PlanetSim.Core.Model;

namespace PlanetSim.Rendering.Input;

public readonly record struct Ray3(Vector3 Origin, Vector3 Direction);
public readonly record struct PickSphere(BodyId Id, Vector3 Center, float Radius);

public static class BodyPicker
{
    public static BodyId? Pick(Ray3 ray, IReadOnlyList<PickSphere> candidates)
    {
        if (ray.Direction.LengthSquared() <= float.Epsilon) return null;
        var direction = Vector3.Normalize(ray.Direction);
        BodyId? nearest = null;
        var nearestDistance = float.PositiveInfinity;

        foreach (var sphere in candidates)
        {
            if (sphere.Radius <= 0) continue;
            var offset = ray.Origin - sphere.Center;
            var b = 2 * Vector3.Dot(offset, direction);
            var c = offset.LengthSquared() - sphere.Radius * sphere.Radius;
            var discriminant = b * b - 4 * c;
            if (discriminant < 0) continue;
            var root = System.MathF.Sqrt(discriminant);
            var near = (-b - root) * 0.5f;
            var far = (-b + root) * 0.5f;
            var distance = near >= 0 ? near : far;
            if (distance >= 0 && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = sphere.Id;
            }
        }
        return nearest;
    }
}
