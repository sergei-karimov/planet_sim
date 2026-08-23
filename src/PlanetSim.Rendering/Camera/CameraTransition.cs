using System.Numerics;

namespace PlanetSim.Rendering.Camera;

public sealed record CameraTransition(Vector3 Start, Vector3 Destination, float DurationSeconds)
{
    public Vector3 Evaluate(float elapsedSeconds)
    {
        if (DurationSeconds <= 0) return Destination;
        var t = System.Math.Clamp(elapsedSeconds / DurationSeconds, 0, 1);
        var smooth = t * t * (3 - 2 * t);
        return Vector3.Lerp(Start, Destination, smooth);
    }
}
