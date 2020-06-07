using Surveillance.RichPresence.Tray;

namespace Surveillance.App
{
    internal static class Program
    {
        private static void Main()
        {
            var client = new SurveillanceClient(
                new TrayRichPresence()
            );
            client.Run();
        }
    }
}