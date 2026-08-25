using Raylib_cs;

namespace PlanetSim.Rendering.Tests;

public sealed class WindowConfigurationTests
{
    [Fact]
    public void DefaultFlagsKeepFramebufferInSyncOnHighDpiDisplays()
    {
        Assert.True(WindowConfiguration.DefaultFlags.HasFlag(ConfigFlags.HighDpiWindow));
        Assert.True(WindowConfiguration.DefaultFlags.HasFlag(ConfigFlags.ResizableWindow));
    }
}
