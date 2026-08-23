using System.Numerics;
using Raylib_cs;

namespace PlanetSim.Rendering.Camera;

public sealed class CameraController
{
    private const float TransitionDuration = 0.4f;
    private float _yaw = 0.8f;
    private float _pitch = 0.45f;
    private float _distance = 540;
    private CameraTransition? _transition;
    private float _transitionElapsed;

    public CameraMode Mode { get; private set; } = CameraMode.SystemOverview;
    public Vector3 Target { get; private set; }

    public void Orbit(Vector2 mouseDelta)
    {
        _yaw -= mouseDelta.X * 0.005f;
        _pitch = System.Math.Clamp(_pitch + mouseDelta.Y * 0.005f, -1.5f, 1.5f);
    }

    public void Zoom(float wheelDelta) =>
        _distance = System.Math.Clamp(_distance * System.MathF.Exp(-wheelDelta * 0.12f), 0.05f, 2_000f);

    public void Pan(Vector2 mouseDelta)
    {
        if (Mode != CameraMode.SystemOverview) return;
        var right = new Vector3(System.MathF.Cos(_yaw), -System.MathF.Sin(_yaw), 0);
        var up = Vector3.UnitZ;
        Target += (-right * mouseDelta.X + up * mouseDelta.Y) * (_distance * 0.0015f);
    }

    public void Follow(Vector3 bodyPosition)
    {
        Mode = CameraMode.FollowBody;
        BeginTransition(bodyPosition);
        _distance = System.Math.Min(_distance, 8f);
    }

    public void ReturnToSystemView()
    {
        Mode = CameraMode.SystemOverview;
        BeginTransition(Vector3.Zero);
        _distance = 540;
    }

    public void UpdateTarget(Vector3 trackedPosition, float deltaSeconds)
    {
        _transitionElapsed += System.Math.Max(0, deltaSeconds);
        if (_transition is not null)
        {
            var destination = Mode == CameraMode.FollowBody ? trackedPosition : Vector3.Zero;
            _transition = _transition with { Destination = destination };
            Target = _transition.Evaluate(_transitionElapsed);
            if (_transitionElapsed >= _transition.DurationSeconds) _transition = null;
        }
        else if (Mode == CameraMode.FollowBody)
        {
            Target = trackedPosition;
        }
    }

    public Camera3D BuildCamera()
    {
        var horizontal = _distance * System.MathF.Cos(_pitch);
        var offset = new Vector3(
            horizontal * System.MathF.Cos(_yaw),
            horizontal * System.MathF.Sin(_yaw),
            _distance * System.MathF.Sin(_pitch));
        return new Camera3D
        {
            Position = Target + offset,
            Target = Target,
            Up = Vector3.UnitZ,
            FovY = 45,
            Projection = CameraProjection.Perspective
        };
    }

    private void BeginTransition(Vector3 destination)
    {
        _transition = new CameraTransition(Target, destination, TransitionDuration);
        _transitionElapsed = 0;
    }
}
