using System;
using System.Globalization;
using Surveillance.App;
using Surveillance.App.RichPresence;
using Surveillance.Client.Steam;
using Surveillance.RichPresence.Discord;
using Surveillance.RichPresence.Tray;

namespace Surveillance.Client
{
    internal static class Program
    {
        private const string SteamId = "STEAM_ID";
        
        private static void Main()
        {
            if (Environment.GetEnvironmentVariable(SteamId) == null)
            {
                if (!SteamClient.IsLaunched)
                    throw new InvalidOperationException("Steam is not running");
                if (!SteamClient.IsConnected)
                    throw new InvalidOperationException("Steam is not connected");
                if (!SteamClient.IsGame(SurveillanceApp.DeadByDaylightAppId, SteamClient.AppState.Installed))
                    throw new InvalidOperationException("Dead by Daylight is not installed");
            
                var activeUser = SteamClient.ActiveUser;
                Environment.SetEnvironmentVariable(SteamId, activeUser.ToString(NumberFormatInfo.InvariantInfo));
            }

            var app = new SurveillanceApp(
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