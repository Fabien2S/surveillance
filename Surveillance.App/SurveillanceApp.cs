using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Surveillance.App.Json;
using Surveillance.App.RichPresence;
using Surveillance.Steam;
using Surveillance.Steam.Models;
using Surveillance.Steam.Response;

namespace Surveillance.App
{
    public class SurveillanceApp
    {
        public const uint DeadByDaylightAppId = 381210;
        private const int UpdateInterval = 1_000;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        
        private readonly Dictionary<string, GameState> _gameStates = new Dictionary<string, GameState>();
        private readonly Dictionary<string, double> _stats = new Dictionary<string, double>();
        
        private readonly ResourceManager _resourceManager;
        private readonly IRichPresence[] _richPresences;
        private readonly int _updateRate;

        private bool _running;

        private bool _dirty;
        private GameState _gameState;

        public SurveillanceApp(IRichPresence[] richPresences)
        {
            _resourceManager = new ResourceManager("Surveillance.App.Resources.Strings", typeof(SurveillanceApp).Assembly);
            _richPresences = richPresences;
            _updateRate = richPresences.Select(rp => rp.UpdateRate).Max();
        }

        public void Run()
        {
            Logger.Info("Starting Surveillance");

            _running = true;

            LoadGameStates();
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
        
        private async void LoadGameStates()
        {
            _stats.Clear();
            
            var type = typeof(SurveillanceApp);
            var path = type.Namespace + ".Resources.GameStates.json";
            var resourceStream = type.Assembly.GetManifestResourceStream(path);
            var gameStates = await JsonSerializer.DeserializeAsync<GameState[]>(resourceStream, DefaultJsonOptions.Instance);
            
            foreach (var gameState in gameStates)
            foreach (var trigger in gameState.Triggers)
                _gameStates[trigger] = gameState;
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
                var apiResponse = await SteamApi.Request<SteamUserStatsForGameResponse>(requestUri, DefaultJsonOptions.Instance);
                var apiResponseContent = apiResponse.Content;
                var playerStats = apiResponseContent.PlayerStats;

                Logger.Info("Received stats of player {0} for {1}", playerStats.SteamId, playerStats.GameName);

                UpdateGameState(playerStats.Stats);

                var utcNow = DateTimeOffset.UtcNow;
                var offset = apiResponse.Expires.Subtract(utcNow);
                Logger.Trace("Next steam request in {0} (now: {1}, expires: {2})", offset, utcNow, apiResponse.Expires);
                await Task.Delay(offset);
            }

            SteamApi.Reset();
        }

        private void UpdateGameState(IEnumerable<SteamGameStatModel> stats)
        {
            foreach (var stat in stats)
            {
                var statName = stat.Name;
                if (!_gameStates.TryGetValue(statName, out var gameState))
                    continue;

                if (_stats.TryGetValue(statName, out var oldStat))
                {
                    if (Math.Abs(oldStat - stat.Value) <= 0)
                        continue;

                    SetGameState(gameState);
                    _stats[statName] = stat.Value;
                }
                else
                    _stats[statName] = stat.Value;
            }
        }

        private void SetGameState(GameState gameState)
        {
            _dirty = true;
            _gameState = gameState;

            var gameCharacter = gameState.Character;
            gameCharacter.DisplayName = I18N("character." + gameCharacter.Type + "." + gameCharacter.Name);
            _gameState.Character = gameCharacter;
            
            _gameState.CharacterString = I18N("character.info.playing_as", gameCharacter.DisplayName);

            var gameAction = gameState.Action;
            gameAction.DisplayName = I18N("action." + gameAction.Type + "." + gameAction.Name);
            _gameState.Action = gameAction;

            var values = gameState.Triggers.Select(trigger => _stats[trigger]).Cast<object>().ToArray();
            _gameState.ActionString = I18N("action." + gameAction.Type + "." + gameAction.Name + ".details", values);
        }

        private static Uri BuildUri()
        {
            var builder = new UriBuilder("https://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v2/");

            var queryCollection = HttpUtility.ParseQueryString(builder.Query);
            queryCollection["key"] = Environment.GetEnvironmentVariable("STEAM_KEY") ?? throw new ArgumentException("Missing STEAM_KEY");
            queryCollection["appid"] = DeadByDaylightAppId.ToString(NumberFormatInfo.InvariantInfo);
            queryCollection["steamid"] = Environment.GetEnvironmentVariable("STEAM_ID") ?? throw new ArgumentException("Missing STEAM_ID");
            builder.Query = queryCollection.ToString() ?? throw new InvalidOperationException();

            return builder.Uri;
        }

        private string I18N(string key, params object[] args)
        {
            var str = _resourceManager.GetString(key) ?? throw new KeyNotFoundException("Missing " + key);
            return args.Length == 0 ? str : string.Format(str, args);
        }

        public void Close()
        {
            _running = false;
        }
    }
}