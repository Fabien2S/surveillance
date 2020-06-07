namespace Surveillance.Steam.Models
{
    public struct SteamUserGameStatsModel
    {
        public string SteamId { get; set; }
        public string GameName { get; set; }
        public SteamGameStatModel[] Stats { get; set; }
        public SteamGameAchievementModel[] Achievements { get; set; }
    }
}