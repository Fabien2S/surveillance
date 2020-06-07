namespace Surveillance.App
{
    internal static class Program
    {
        private static void Main()
        {
            var client = new SurveillanceClient();
            client.Run();
        }
    }
}