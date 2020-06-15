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
        
        private static void Main(string[] args)
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

            if (Environment.GetEnvironmentVariable("STEAM_KEY") == null)
            {
                if(args.Length < 1)
                    throw new ArgumentOutOfRangeException(nameof(args), "Missing STEAM_KEY argument");
                Environment.SetEnvironmentVariable("STEAM_KEY", args[0]);
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