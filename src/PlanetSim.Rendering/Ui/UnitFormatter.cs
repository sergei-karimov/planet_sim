using System.Globalization;

namespace PlanetSim.Rendering.Ui;

public static class UnitFormatter
{
    public const double MetersPerKilometer = 1_000;
    public const double MetersPerAstronomicalUnit = 149_597_870_700;

    public static string Distance(double meters) =>
        System.Math.Abs(meters) >= MetersPerAstronomicalUnit * 0.01
            ? string.Format(CultureInfo.InvariantCulture, "{0:0.000} AU", meters / MetersPerAstronomicalUnit)
            : string.Format(CultureInfo.InvariantCulture, "{0:N0} km", meters / MetersPerKilometer);

    public static string Speed(double metersPerSecond) =>
        string.Format(CultureInfo.InvariantCulture, "{0:0.00} km/s", metersPerSecond / MetersPerKilometer);

    public static string Mass(double kilograms) =>
        kilograms.ToString("0.000E+00", CultureInfo.InvariantCulture) + " kg";

    public static string Diameter(double meters) =>
        string.Format(CultureInfo.InvariantCulture, "{0:N0} km", meters / MetersPerKilometer);
}
