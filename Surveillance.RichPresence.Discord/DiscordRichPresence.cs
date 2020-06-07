using DiscordGameSDK;
using Surveillance.App;

namespace Surveillance.RichPresence.Discord
{
    public class DiscordRichPresence : IRichPresence
    {
        private const long ClientId = 569843143495647242;
        
        public int UpdateRate { get; } = 4_000;

        private DiscordGameSDK.Discord _discord;

        public void Init(ISurveillanceApp app)
        {
            _discord = new DiscordGameSDK.Discord(ClientId, (ulong) CreateFlags.NoRequireDiscord);
        }

        public void PollEvents()
        {
            _discord.RunCallbacks();
        }

        public void UpdateActivity(GameState gameState)
        {
            var activityManager = _discord.GetActivityManager();
            activityManager.UpdateActivity(new Activity
            {
                Instance = true,
                Name = "Surveillance",
                Type = ActivityType.Watching,
                Assets = new ActivityAssets
                {
                    LargeText = gameState.Character,
                    LargeImage = gameState.CharacterIcon,
                    SmallText = gameState.Action,
                    SmallImage = gameState.ActionIcon
                }
            }, result => { });
        }

        public void Dispose()
        {
            _discord.Dispose();
            _discord = null;
        }
    }
}