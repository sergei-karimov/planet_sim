using System.Numerics;
using PlanetSim.App;
using PlanetSim.App.Simulation;
using PlanetSim.Rendering.Assets;
using PlanetSim.Rendering.Camera;
using PlanetSim.Rendering.Input;
using PlanetSim.Rendering.Scene;
using PlanetSim.Rendering.Ui;
using Raylib_cs;

namespace PlanetSim.Rendering;

public sealed class PlanetSimGame
{
    private readonly SimulationController _controller = Composition.CreateController();
    private readonly CameraController _camera = new();
    private readonly HudRenderer _hud = new();
    private double _resetConfirmationDeadline;

    public void Run()
    {
        Raylib.SetConfigFlags(WindowConfiguration.DefaultFlags);
        Raylib.InitWindow(1280, 720, EnglishStrings.Title);
        if (!Raylib.IsWindowReady()) throw new InvalidOperationException("Raylib window initialization failed.");
        Raylib.SetTargetFPS(60);
        SolarSystemRenderer? renderer = null;
        try
        {
            var definitions = _controller.Status.Snapshot.Bodies.Select(body => body.Definition);
            var textureRoot = Path.Combine(AppContext.BaseDirectory, "assets", "textures");
            var textures = TextureCatalog.Load(definitions, textureRoot,
                warning => Console.Error.WriteLine($"Warning: {warning}"));
            renderer = new SolarSystemRenderer(textures);
            while (!Raylib.WindowShouldClose())
            {
                var delta = System.Math.Min(Raylib.GetFrameTime(), 0.25f);
                HandleInput();
                _controller.Update(delta);
                UpdateCameraTarget(delta);
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(3, 6, 16, 255));
                var camera = _camera.BuildCamera();
                Raylib.BeginMode3D(camera);
                renderer.Draw3D(_controller.Status.Snapshot, _controller.TrailPositions,
                    _controller.Status.TrailsVisible, _controller.Status.SelectedBody);
                Raylib.EndMode3D();
                var actions = _hud.Draw(_controller.Status, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                Raylib.DrawFPS(Raylib.GetScreenWidth() - 90, Raylib.GetScreenHeight() - 28);
                Raylib.EndDrawing();
                ApplyHudActions(actions);
            }
        }
        finally
        {
            renderer?.Dispose();
            if (Raylib.IsWindowReady()) Raylib.CloseWindow();
        }
    }

    private void HandleInput()
    {
        var mouse = Raylib.GetMousePosition();
        var overHud = _hud.IsPointerOverHud(mouse, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var movement = Raylib.GetMouseDelta();
        if (!overHud && Raylib.IsMouseButtonDown(MouseButton.Left)) _camera.Orbit(movement);
        if (!overHud && (Raylib.IsMouseButtonDown(MouseButton.Middle) || Raylib.IsMouseButtonDown(MouseButton.Right))) _camera.Pan(movement);
        if (!overHud) _camera.Zoom(Raylib.GetMouseWheelMove());
        if (!overHud && Raylib.IsMouseButtonPressed(MouseButton.Left)) PickBody(mouse);
        if (Raylib.IsKeyPressed(KeyboardKey.Space)) _controller.TogglePause();
        if (Raylib.IsKeyPressed(KeyboardKey.O)) _controller.ToggleTrails();
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) ReturnToSystemView();
        if (Raylib.IsKeyPressed(KeyboardKey.R)) RequestReset();
        if (Raylib.IsKeyPressed(KeyboardKey.H)) _hud.ToggleHelp();
    }

    private void PickBody(Vector2 mouse)
    {
        var raylibRay = Raylib.GetScreenToWorldRay(mouse, _camera.BuildCamera());
        var spheres = _controller.Status.Snapshot.Bodies.Select(body => new PickSphere(
            body.Definition.Id, SceneScale.Position(body.PositionMeters), SceneScale.Radius(body.Definition))).ToArray();
        var selected = BodyPicker.Pick(new Ray3(raylibRay.Position, raylibRay.Direction), spheres);
        if (selected is null) return;
        _controller.Select(selected.Value);
        _camera.Follow(SceneScale.Position(_controller.Status.Snapshot.GetBody(selected.Value).PositionMeters));
    }

    private void UpdateCameraTarget(float delta)
    {
        var target = _controller.Status.SelectedBody is { } selected
            ? SceneScale.Position(_controller.Status.Snapshot.GetBody(selected).PositionMeters)
            : Vector3.Zero;
        _camera.UpdateTarget(target, delta);
    }

    private void ApplyHudActions(HudActions actions)
    {
        if (actions.TogglePause) _controller.TogglePause();
        if (actions.ToggleTrails) _controller.ToggleTrails();
        if (actions.ReturnToSystemView) ReturnToSystemView();
        if (actions.NewSliderValue is { } slider) _controller.SetSlider(slider);
        if (actions.Reset) RequestReset();
        if (actions.ToggleHelp) _hud.ToggleHelp();
    }

    private void RequestReset()
    {
        if (_controller.Status.Snapshot.Elapsed <= TimeSpan.FromDays(1) || Raylib.GetTime() <= _resetConfirmationDeadline)
        {
            _controller.Reset();
            _camera.ReturnToSystemView();
            _resetConfirmationDeadline = 0;
        }
        else
        {
            _resetConfirmationDeadline = Raylib.GetTime() + 3;
        }
    }

    private void ReturnToSystemView()
    {
        _controller.ReturnToSystemView();
        _camera.ReturnToSystemView();
    }
}
