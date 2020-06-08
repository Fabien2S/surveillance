using System.Threading.Tasks;
using DiscordGameSDK;
using Surveillance.App;
using Surveillance.App.RichPresence;

namespace Surveillance.RichPresence.Discord
{
    public class DiscordRichPresence : IRichPresence
    {
        private const long ClientId = 569843143495647242;
        
        public int UpdateRate { get; } = 4_000;

        private DiscordGameSDK.Discord _discord;

        public Task Init(SurveillanceApp app)
        {
            _discord = new DiscordGameSDK.Discord(ClientId, (ulong) CreateFlags.NoRequireDiscord);

            var completionSource = new TaskCompletionSource<bool>();
            var userManager = _discord.GetUserManager();

            void HandleUserUpdate()
            {
                userManager.OnCurrentUserUpdate -= HandleUserUpdate;
                completionSource.SetResult(true);
            }

            userManager.OnCurrentUserUpdate += HandleUserUpdate;
            return completionSource.Task;
        }

        public void PollEvents()
        {
            _discord.RunCallbacks();
        }

        public void UpdateGameState(GameState gameState)
        {
            var gameCharacter = gameState.Character;
            var gameAction = gameState.Action;

            var activityManager = _discord.GetActivityManager();
            activityManager.UpdateActivity(new Activity
            {
                Instance = true,
                Type = ActivityType.Playing,
                Details = gameState.Details,
                State = gameState.State,
                Assets = new ActivityAssets
                {
                    LargeText = gameCharacter.DisplayName,
                    LargeImage = "character_" + gameCharacter.Type + "_" + gameCharacter.Name,
                    SmallText = gameAction.DisplayName,
                    SmallImage = "action_" + gameAction.Type + "_" + gameAction.Name
                }
            }, result => {});
        }

        public void Dispose()
        {
            _discord.Dispose();
            _discord = null;
        }
    }
}