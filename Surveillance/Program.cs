using Surveillance.RichPresence.Discord;
using Surveillance.RichPresence.Tray;

namespace Surveillance
{
    internal static class Program
    {
        private static void Main()
        {
            var app = new SurveillanceApp(
                new TrayRichPresence(),
                new DiscordRichPresence()
            );
            app.Run();
        }
    }
}