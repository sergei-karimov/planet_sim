using System.Globalization;
using System.Numerics;
using PlanetSim.App.Simulation;
using PlanetSim.Core.Model;
using Raylib_cs;

namespace PlanetSim.Rendering.Ui;

public readonly record struct UiPoint(float X, float Y);
public readonly record struct UiRect(float X, float Y, float Width, float Height)
{
    public bool Contains(UiPoint point) => point.X >= X && point.X <= X + Width
        && point.Y >= Y && point.Y <= Y + Height;
}

public static class HudLayout
{
    public static double SliderValue(UiRect bounds, float x) =>
        System.Math.Clamp((x - bounds.X) / bounds.Width, 0, 1);

    public static bool HelpConsumesPointer(bool visible, UiPoint point, UiRect bounds) =>
        visible && bounds.Contains(point);
}

public readonly record struct HudActions(
    bool TogglePause = false,
    bool Reset = false,
    bool ReturnToSystemView = false,
    bool ToggleTrails = false,
    bool ToggleHelp = false,
    double? NewSliderValue = null)
{
    public static HudActions None => new();
}

public sealed class HudRenderer
{
    private static readonly Color Panel = new(10, 16, 30, 225);
    private static readonly Color Accent = new(74, 144, 226, 255);
    private static readonly UiRect PauseButton = new(20, 72, 74, 28);
    private static readonly UiRect ResetButton = new(100, 72, 74, 28);
    private static readonly UiRect TrailsButton = new(180, 72, 74, 28);
    private static readonly UiRect SystemButton = new(260, 72, 110, 28);
    private static readonly UiRect Slider = new(20, 122, 350, 16);
    private bool _helpVisible = true;

    public bool IsPointerOverHud(Vector2 mouse, int screenWidth, int screenHeight)
    {
        var point = new UiPoint(mouse.X, mouse.Y);
        var info = new UiRect(screenWidth - 300, 10, 290, 225);
        var help = new UiRect(10, screenHeight - 58, System.Math.Min(760, screenWidth - 20), 48);
        return new UiRect(10, 10, 420, 145).Contains(point)
            || info.Contains(point)
            || HudLayout.HelpConsumesPointer(_helpVisible, point, help);
    }

    public HudActions Draw(SimulationStatus status, int screenWidth, int screenHeight)
    {
        Raylib.DrawRectangleRounded(new Rectangle(10, 10, 420, 145), 0.08f, 8, Panel);
        Raylib.DrawText(status.IsPaused ? EnglishStrings.Paused : EnglishStrings.Running, 20, 18, 20,
            status.IsPaused ? Color.Gold : Color.Lime);
        Raylib.DrawText($"{EnglishStrings.SimulationTime}: {FormatElapsed(status.Snapshot.Elapsed)}", 130, 20, 16, Color.LightGray);
        Raylib.DrawText($"{EnglishStrings.RequestedRate}: {FormatRate(status.RequestedSecondsPerRealSecond)}", 20, 45, 15, Color.LightGray);
        Raylib.DrawText($"{EnglishStrings.EffectiveRate}: {FormatRate(status.EffectiveSecondsPerRealSecond)}", 220, 45, 15, Color.LightGray);

        var mouse = Raylib.GetMousePosition();
        var clicked = Raylib.IsMouseButtonPressed(MouseButton.Left);
        DrawButton(PauseButton, status.IsPaused ? EnglishStrings.Resume : EnglishStrings.Pause);
        DrawButton(ResetButton, EnglishStrings.Reset);
        DrawButton(TrailsButton, EnglishStrings.Trails);
        DrawButton(SystemButton, EnglishStrings.SystemView);

        Raylib.DrawText("Time rate", 20, 104, 14, Color.LightGray);
        Raylib.DrawRectangle((int)Slider.X, (int)Slider.Y, (int)Slider.Width, (int)Slider.Height, new Color(45, 55, 75, 255));
        Raylib.DrawCircle((int)(Slider.X + Slider.Width * status.SliderValue), (int)(Slider.Y + Slider.Height / 2), 8, Accent);

        var actions = new HudActions(
            TogglePause: clicked && PauseButton.Contains(new UiPoint(mouse.X, mouse.Y)),
            Reset: clicked && ResetButton.Contains(new UiPoint(mouse.X, mouse.Y)),
            ReturnToSystemView: clicked && SystemButton.Contains(new UiPoint(mouse.X, mouse.Y)),
            ToggleTrails: clicked && TrailsButton.Contains(new UiPoint(mouse.X, mouse.Y)));
        if (Raylib.IsMouseButtonDown(MouseButton.Left) && Slider.Contains(new UiPoint(mouse.X, mouse.Y)))
            actions = actions with { NewSliderValue = HudLayout.SliderValue(Slider, mouse.X) };

        DrawSelectedBody(status, screenWidth);
        if (_helpVisible) DrawHelp(screenHeight, screenWidth);
        if (status.ErrorMessage is not null)
            Raylib.DrawText(status.ErrorMessage, 20, 165, 18, Color.Red);
        return actions;
    }

    public void ToggleHelp() => _helpVisible = !_helpVisible;

    private static void DrawSelectedBody(SimulationStatus status, int screenWidth)
    {
        if (status.SelectedBody is null) return;
        var body = status.Snapshot.GetBody(status.SelectedBody.Value);
        var sun = status.Snapshot.GetBody(BodyId.Sun);
        var distance = (body.PositionMeters - sun.PositionMeters).Length;
        var x = screenWidth - 290;
        Raylib.DrawRectangleRounded(new Rectangle(x - 10, 10, 290, 225), 0.08f, 8, Panel);
        var lines = new[]
        {
            body.Definition.DisplayName,
            $"{EnglishStrings.BodyType}: {body.Definition.Kind}",
            $"{EnglishStrings.Mass}: {UnitFormatter.Mass(body.Definition.MassKg)}",
            $"{EnglishStrings.Diameter}: {UnitFormatter.Diameter(body.Definition.DiameterMeters)}",
            $"{EnglishStrings.SunDistance}: {UnitFormatter.Distance(distance)}",
            $"{EnglishStrings.Speed}: {UnitFormatter.Speed(body.SpeedMetersPerSecond)}",
            EnglishStrings.PlanetScale,
            EnglishStrings.SunScale
        };
        for (var i = 0; i < lines.Length; i++)
            Raylib.DrawText(lines[i], x, 20 + i * 25, i == 0 ? 22 : 15, i == 0 ? Color.White : Color.LightGray);
    }

    private static void DrawHelp(int screenHeight, int screenWidth)
    {
        var width = System.Math.Min(760, screenWidth - 20);
        Raylib.DrawRectangleRounded(new Rectangle(10, screenHeight - 58, width, 48), 0.08f, 8, Panel);
        Raylib.DrawText(EnglishStrings.Help, 18, screenHeight - 52, 14, Color.LightGray);
    }

    private static void DrawButton(UiRect rect, string label)
    {
        var hovered = rect.Contains(new UiPoint(Raylib.GetMouseX(), Raylib.GetMouseY()));
        Raylib.DrawRectangleRounded(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height), 0.2f, 6,
            hovered ? Accent : new Color(38, 58, 88, 255));
        Raylib.DrawText(label, (int)rect.X + 8, (int)rect.Y + 6, 15, Color.White);
    }

    private static string FormatElapsed(TimeSpan elapsed) =>
        string.Format(CultureInfo.InvariantCulture, "{0}d {1:00}:{2:00}:{3:00}",
            (int)elapsed.TotalDays, elapsed.Hours, elapsed.Minutes, elapsed.Seconds);

    private static string FormatRate(double rate) => rate <= 0 ? "0"
        : string.Format(CultureInfo.InvariantCulture, "{0:0.##} sim s/s", rate);
}
