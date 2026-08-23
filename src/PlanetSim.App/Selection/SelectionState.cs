using PlanetSim.Core.Model;

namespace PlanetSim.App.Selection;

public sealed class SelectionState
{
    public BodyId? SelectedBody { get; private set; }
    public void Select(BodyId body) => SelectedBody = body;
    public void ReturnToSystemView() => SelectedBody = null;
}
