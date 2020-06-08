using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Surveillance.App;
using Surveillance.App.Json;
using Surveillance.RichPresence;
using Surveillance.Steam;
using Surveillance.Steam.Models;
using Surveillance.Steam.Response;

namespace Surveillance
{
    public class SurveillanceApp : ISurveillanceApp
    {
        private const uint DeadByDaylightAppId = 381210;
        private const int UpdateInterval = 1_000;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly GameState[] _gameStates;
        private readonly IRichPresence[] _richPresences;
        private readonly int _updateRate;

        private bool _running;

        private bool _dirty;
        private GameState _gameState;
        private SteamGameStatModel[] _gameStats;
        private ulong _activeUser;

        public SurveillanceApp(GameState[] gameStates, IRichPresence[] richPresences)
        {
            _gameStates = gameStates;
            _richPresences = richPresences;
            _updateRate = richPresences.Select(rp => rp.UpdateRate).Max();
        }

        public void Run()
        {
            Logger.Info("Starting Surveillance");

            _running = true;
            
            if (!SteamClient.IsLaunched)
                throw new InvalidOperationException("Steam is not running");
            if (!SteamClient.IsConnected)
                throw new InvalidOperationException("Steam is not connected");
            if (!SteamClient.IsGame(DeadByDaylightAppId, SteamClient.AppState.Installed))
                throw new InvalidOperationException("Dead by Daylight is not installed");

            _activeUser = SteamClient.ActiveUser;
            Logger.Debug("[Steam] Logged in as {0}", _activeUser);

            RunRichPresenceLoop();
            RunStatsRequestLoop();

            try
            {
                while (_running)
                {
                    foreach (var richPresence in _richPresences)
                        richPresence.PollEvents();
                    Thread.Sleep(UpdateInterval);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            Logger.Info("Shutting down");
        }

        private async void RunRichPresenceLoop()
        {
            foreach (var richPresence in _richPresences)
                await richPresence.Init(this);

            while (_running)
            {
                if (!_dirty)
                    continue;
                
                foreach (var richPresence in _richPresences)
                    richPresence.UpdateGameState(_gameState);

                _dirty = false;
                await Task.Delay(_updateRate);
            }
            
            foreach (var richPresence in _richPresences)
                richPresence.Dispose();
        }

        private async void RunStatsRequestLoop()
        {
            var steamKey = Environment.GetEnvironmentVariable("STEAM_KEY");
            SteamApi.Init(steamKey);

            var requestUri = BuildUri();

            while (_running)
            {
                var apiResponse =
                    await SteamApi.Request<SteamUserStatsForGameResponse>(requestUri, DefaultJsonOptions.Instance);
                var apiResponseContent = apiResponse.Content;
                var playerStats = apiResponseContent.PlayerStats;

                Logger.Info("Received stats of player {0} for {1}", playerStats.SteamId, playerStats.GameName);

                UpdateGameState(playerStats.Stats);

                var res = new SteamGameStatModel[_gameStats.Length];
                Array.Copy(_gameStats, res, _gameStats.Length);
                res[22] = new SteamGameStatModel
                {
                    Name = "DBD_UncloakAttack",
                    Value = 128755
                };
                UpdateGameState(res);

                var utcNow = DateTimeOffset.UtcNow;
                var offset = apiResponse.Expires.Subtract(utcNow);
                Logger.Trace("Next steam request in {0} (now: {1}, expires: {2})", offset, utcNow, apiResponse.Expires);
                await Task.Delay(offset);
            }

            SteamApi.Reset();
        }

        private void UpdateGameState(SteamGameStatModel[] stats)
        {
            if (_gameStats == null)
            {
                for (var i = 0; i < stats.Length; i++)
                {
                    var stat = stats[i];
                    Logger.Debug("[{0}] {1} = {2}", i, stat.Name, stat.Value);
                }

                Logger.Debug("Ignoring first game stats response");
                _gameStats = stats;
                return;
            }

            for (var i = 0; i < stats.Length; i++)
            {
                var stat = stats[i];
                var oldStat = _gameStats[i];
                Debug.Assert(
                    stat.Name.Equals(oldStat.Name, StringComparison.Ordinal),
                    "stat[i].Name != oldStat[i].Name"
                );

                if (Math.Abs(stat.Value - oldStat.Value) <= 0)
                    continue;

                Logger.Trace("Checking stat {0}", stat.Name);
                if (!FindGameState(stat.Name, out var definition))
                    continue;

                SetGameState(definition);
                break;
            }

            _gameStats = stats;
        }

        private bool FindGameState(string name, out GameState gameState)
        {
            foreach (var state in _gameStates)
            {
                if (!state.Triggers.Any(trigger => trigger.Equals(name, StringComparison.Ordinal)))
                    continue;

                gameState = state;
                return true;
            }

            gameState = default;
            return false;
        }

        private void SetGameState(GameState gameState)
        {
            _dirty = true;
            _gameState = gameState;
        }

        private Uri BuildUri()
        {
            var builder = new UriBuilder("https://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v2/");

            var queryCollection = HttpUtility.ParseQueryString(builder.Query);
            queryCollection["key"] = Environment.GetEnvironmentVariable("STEAM_KEY") ??
                                     throw new ArgumentException("Missing STEAM_KEY environment variable");
            queryCollection["appid"] = DeadByDaylightAppId.ToString(NumberFormatInfo.InvariantInfo);
            queryCollection["steamid"] = _activeUser.ToString(NumberFormatInfo.InvariantInfo);
            builder.Query = queryCollection.ToString() ?? throw new InvalidOperationException();

            return builder.Uri;
        }

        public void Close()
        {
            _running = false;
        }
    }
}