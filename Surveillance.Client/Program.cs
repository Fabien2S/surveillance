using System;
using System.Text.Json;
using Surveillance.App;
using Surveillance.App.Json;
using Surveillance.App.RichPresence;
using Surveillance.Client.Steam;
using Surveillance.RichPresence.Discord;
using Surveillance.RichPresence.Tray;

namespace Surveillance.Client
{
    internal static class Program
    {
        private static void Main()
        {
            if (!SteamClient.IsLaunched)
                throw new InvalidOperationException("Steam is not running");
            if (!SteamClient.IsConnected)
                throw new InvalidOperationException("Steam is not connected");
            if (!SteamClient.IsGame(SurveillanceApp.DeadByDaylightAppId, SteamClient.AppState.Installed))
                throw new InvalidOperationException("Dead by Daylight is not installed");

            var type = typeof(Program);
            var path = type.Namespace + ".Resources.GameStates.json";
            var resourceStream = type.Assembly.GetManifestResourceStream(path);
            var gameStates = JsonSerializer.DeserializeAsync<GameState[]>(resourceStream, DefaultJsonOptions.Instance).Result;

            var app = new SurveillanceApp(
                SteamClient.ActiveUser,
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