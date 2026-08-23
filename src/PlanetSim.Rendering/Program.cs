namespace PlanetSim.Rendering;

public static class Program
{
    [STAThread]
    public static int Main()
    {
        try
        {
            new PlanetSimGame().Run();
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"PlanetSim could not start: {exception.Message}");
            return 1;
        }
    }
}
