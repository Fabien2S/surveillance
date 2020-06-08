using System.Text.Json;
using Surveillance.App;
using Surveillance.App.Json;
using Surveillance.RichPresence;
using Surveillance.RichPresence.Discord;
using Surveillance.RichPresence.Tray;

namespace Surveillance
{
    internal static class Program
    {
        private static void Main()
        {
            var type = typeof(SurveillanceApp);
            var path = type.Namespace + ".Resources.GameStates.json";
            var resourceStream = type.Assembly.GetManifestResourceStream(path);
            var gameStates = JsonSerializer.DeserializeAsync<GameState[]>(resourceStream, DefaultJsonOptions.Instance).Result;

            var app = new SurveillanceApp(
                gameStates,
                new IRichPresence[]
                {
                    new TrayRichPresence(),
                    new DiscordRichPresence(), 
                }
            );
            app.Run();
        }
    }
}