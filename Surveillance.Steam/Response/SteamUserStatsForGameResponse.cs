using Surveillance.Steam.Models;

namespace Surveillance.Steam.Response
{
    public struct SteamUserStatsForGameResponse
    {
        public SteamUserGameStatsModel PlayerStats { get; set; }
    }
}