using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Surveillance.App.Json;
using Surveillance.App.Models;
using Surveillance.RichPresence;
using Surveillance.Steam;
using Surveillance.Steam.Response;

namespace Surveillance.App
{
    public class SurveillanceClient : IApplication
    {
        private const uint DeadByDaylightAppId = 381210;
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly IRichPresence[] _richPresences;
        private readonly int _updateRate;

        private bool _running;

        private bool _dirty;
        private GameStateModel _gameState;

        public SurveillanceClient(params IRichPresence[] richPresences)
        {
            _richPresences = richPresences;
            _updateRate = richPresences.Select(rp => rp.UpdateRate).Max();
        }

        public void Run()
        {
            Logger.Info("Starting Surveillance");

            _running = true;

            try
            {
                foreach (var richPresence in _richPresences)
                    richPresence.Init(this);
            }
            catch (Exception e)
            {
                Logger.Error("Unable to initialize Rich Presence");
                Logger.Error(e);
                return;
            }
            
            RequestStatistics();
            
            try
            {
                while (_running)
                {
                    foreach (var richPresence in _richPresences)
                    {
                        richPresence.PollEvents();
                        if (_dirty)
                        {
                            richPresence.UpdateActivity(
                                _gameState.Character,
                                _gameState.Item,
                                _gameState.Details
                            );
                        }
                    }

                    _dirty = false;
                    Thread.Sleep(_updateRate);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            foreach (var richPresence in _richPresences)
                richPresence.Dispose();

            Logger.Info("Shutting down");
        }

        private async void RequestStatistics()
        {
            var steamKey = Environment.GetEnvironmentVariable("STEAM_KEY");
            SteamApi.Init(steamKey);
            
            var requestUri = BuildUri();

            while (_running)
            {
                var apiResponse = await SteamApi.Request<SteamUserStatsForGameResponse>(requestUri, DefaultJsonOptions.Instance);
                var apiResponseContent = apiResponse.Content;
                var playerStats = apiResponseContent.PlayerStats;
                
                Logger.Info("Received stats of player {0} for {1}", playerStats.SteamId, playerStats.GameName);

                var gameStats = playerStats.Stats;
                foreach (var statModel in gameStats)
                    Logger.Debug("{0} = {1}", statModel.Name, statModel.Value);

                var utcNow = DateTimeOffset.UtcNow;
                var offset = apiResponse.Expires.Subtract(utcNow);
                Logger.Trace("Next steam request in {0} (now: {1}, expires: {2})", offset, utcNow, apiResponse.Expires);
                await Task.Delay(offset);
            }
            
            SteamApi.Reset();
        }

        private static Uri BuildUri()
        {
            var builder = new UriBuilder("https://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v2/");

            var queryCollection = HttpUtility.ParseQueryString(builder.Query);
            queryCollection["key"] = Environment.GetEnvironmentVariable("STEAM_KEY") ?? throw new ArgumentException("Missing STEAM_KEY environment variable");
            queryCollection["appid"] = DeadByDaylightAppId.ToString(NumberFormatInfo.InvariantInfo);
            queryCollection["steamid"] = "76561198135169007";
            builder.Query = queryCollection.ToString() ?? throw new InvalidOperationException();

            return builder.Uri;
        }

        public void Close()
        {
            _running = false;
        }
    }
}