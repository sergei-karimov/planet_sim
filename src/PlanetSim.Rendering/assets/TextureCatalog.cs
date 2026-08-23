using PlanetSim.Core.Model;
using Raylib_cs;

namespace PlanetSim.Rendering.Assets;

public sealed class TextureCatalog : IDisposable
{
    private readonly Dictionary<BodyId, Texture2D> _textures = [];
    private bool _disposed;

    private TextureCatalog() { }

    public static TextureCatalog Load(
        IEnumerable<CelestialBodyDefinition> definitions,
        string assetRoot,
        Action<string> logWarning)
    {
        var catalog = new TextureCatalog();
        foreach (var definition in definitions)
        {
            var path = Path.Combine(assetRoot, definition.TextureAsset);
            if (!File.Exists(path))
            {
                logWarning($"Texture missing for {definition.DisplayName}: {path}");
                continue;
            }
            var texture = Raylib.LoadTexture(path);
            if (!Raylib.IsTextureValid(texture))
            {
                logWarning($"Texture invalid for {definition.DisplayName}: {path}");
                continue;
            }
            catalog._textures[definition.Id] = texture;
        }
        return catalog;
    }

    public bool TryGet(BodyId id, out Texture2D texture) => _textures.TryGetValue(id, out texture);

    public void Dispose()
    {
        if (_disposed) return;
        foreach (var texture in _textures.Values) Raylib.UnloadTexture(texture);
        _textures.Clear();
        _disposed = true;
    }
}
