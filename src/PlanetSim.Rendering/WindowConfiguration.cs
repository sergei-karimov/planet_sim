using Raylib_cs;

namespace PlanetSim.Rendering;

public static class WindowConfiguration
{
    public const ConfigFlags DefaultFlags =
        ConfigFlags.ResizableWindow |
        ConfigFlags.VSyncHint |
        ConfigFlags.HighDpiWindow;
}
