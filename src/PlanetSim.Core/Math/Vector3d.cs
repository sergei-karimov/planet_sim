namespace PlanetSim.Core.Math;

public readonly record struct Vector3d(double X, double Y, double Z)
{
    public static Vector3d Zero => new(0, 0, 0);
    public double LengthSquared => X * X + Y * Y + Z * Z;
    public double Length => System.Math.Sqrt(LengthSquared);
    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);

    public static Vector3d operator +(Vector3d a, Vector3d b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3d operator -(Vector3d a, Vector3d b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3d operator -(Vector3d value) => new(-value.X, -value.Y, -value.Z);

    public static Vector3d operator *(Vector3d value, double scalar) =>
        new(value.X * scalar, value.Y * scalar, value.Z * scalar);

    public static Vector3d operator *(double scalar, Vector3d value) => value * scalar;

    public static Vector3d operator /(Vector3d value, double scalar) =>
        new(value.X / scalar, value.Y / scalar, value.Z / scalar);

    public static double Dot(Vector3d a, Vector3d b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3d Cross(Vector3d a, Vector3d b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public static Vector3d Lerp(Vector3d a, Vector3d b, double amount) =>
        a + (b - a) * amount;
}
