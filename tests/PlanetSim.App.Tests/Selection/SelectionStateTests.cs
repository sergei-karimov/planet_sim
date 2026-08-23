using PlanetSim.App.Selection;
using PlanetSim.Core.Model;

namespace PlanetSim.App.Tests.Selection;

public sealed class SelectionStateTests
{
    [Fact]
    public void SelectAndReturnToSystemViewAreExplicit()
    {
        var state = new SelectionState();
        state.Select(BodyId.Earth);
        Assert.Equal(BodyId.Earth, state.SelectedBody);
        state.ReturnToSystemView();
        Assert.Null(state.SelectedBody);
    }
}
