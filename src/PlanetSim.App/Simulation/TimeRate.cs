namespace PlanetSim.App.Simulation;

public readonly record struct TimeRate(double SliderValue, double SimulatedSecondsPerRealSecond)
{
    public const double MinimumRate = 1;
    public const double MaximumRate = 2_592_000;

    public static TimeRate FromSlider(double value)
    {
        var slider = System.Math.Clamp(value, 0, 1);
        var rate = System.Math.Exp(System.Math.Log(MinimumRate)
            + slider * (System.Math.Log(MaximumRate) - System.Math.Log(MinimumRate)));
        return new TimeRate(slider, rate);
    }
}
