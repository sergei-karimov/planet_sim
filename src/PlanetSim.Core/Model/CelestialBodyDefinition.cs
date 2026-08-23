namespace PlanetSim.Core.Model;

public sealed record CelestialBodyDefinition(
    BodyId Id,
    string DisplayName,
    BodyKind Kind,
    double MassKg,
    double DiameterMeters,
    string TextureAsset,
    uint FallbackRgba)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
            throw new ArgumentException("Display name is required.", nameof(DisplayName));
        if (!double.IsFinite(MassKg) || MassKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(MassKg), "Mass must be positive and finite.");
        if (!double.IsFinite(DiameterMeters) || DiameterMeters <= 0)
            throw new ArgumentOutOfRangeException(nameof(DiameterMeters), "Diameter must be positive and finite.");
        if (string.IsNullOrWhiteSpace(TextureAsset))
            throw new ArgumentException("Texture asset is required.", nameof(TextureAsset));
    }
}
