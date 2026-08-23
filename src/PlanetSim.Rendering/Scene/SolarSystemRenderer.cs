using System.Numerics;
using PlanetSim.Core.Math;
using PlanetSim.Core.Model;
using PlanetSim.Rendering.Assets;
using Raylib_cs;

namespace PlanetSim.Rendering.Scene;

public sealed class SolarSystemRenderer : IDisposable
{
    private readonly TextureCatalog _textures;
    private readonly Model _sphereModel;
    private readonly Vector3[] _stars;
    private bool _disposed;

    public SolarSystemRenderer(TextureCatalog textures)
    {
        _textures = textures;
        _sphereModel = Raylib.LoadModelFromMesh(Raylib.GenMeshSphere(1, 32, 32));
        var random = new Random(1977);
        _stars = Enumerable.Range(0, 300).Select(_ =>
        {
            var direction = Vector3.Normalize(new Vector3(
                random.NextSingle() * 2 - 1,
                random.NextSingle() * 2 - 1,
                random.NextSingle() * 2 - 1));
            return direction * 900;
        }).ToArray();
    }

    public void Draw3D(
        SimulationSnapshot snapshot,
        IReadOnlyDictionary<BodyId, IReadOnlyList<Vector3d>> trails,
        bool trailsVisible,
        BodyId? selectedBody)
    {
        foreach (var star in _stars) Raylib.DrawPoint3D(star, new Color(170, 180, 210, 255));
        if (trailsVisible) DrawTrails(trails, selectedBody);

        foreach (var body in snapshot.Bodies)
        {
            var position = SceneScale.Position(body.PositionMeters);
            var radius = SceneScale.Radius(body.Definition);
            if (_textures.TryGet(body.Definition.Id, out var texture))
            {
                var model = _sphereModel;
                Raylib.SetMaterialTexture(ref model, 0, MaterialMapIndex.Albedo, ref texture);
                Raylib.DrawModelEx(model, position, Vector3.UnitZ, 0,
                    new Vector3(radius), Color.White);
            }
            else
            {
                Raylib.DrawSphereEx(position, radius, 24, 24, FromRgba(body.Definition.FallbackRgba));
            }

            if (selectedBody == body.Definition.Id)
                Raylib.DrawSphereWires(position, radius * 1.18f, 16, 16, Color.Yellow);
        }
    }

    private static void DrawTrails(
        IReadOnlyDictionary<BodyId, IReadOnlyList<Vector3d>> trails,
        BodyId? selectedBody)
    {
        foreach (var pair in trails)
        {
            var points = pair.Value;
            if (points.Count < 2) continue;
            var color = pair.Key == selectedBody ? Color.Yellow : new Color(90, 120, 170, 150);
            for (var i = 1; i < points.Count; i++)
                Raylib.DrawLine3D(SceneScale.Position(points[i - 1]), SceneScale.Position(points[i]), color);
        }
    }

    private static Color FromRgba(uint value) => new(
        (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);

    public void Dispose()
    {
        if (_disposed) return;
        Raylib.UnloadModel(_sphereModel);
        _textures.Dispose();
        _disposed = true;
    }
}
