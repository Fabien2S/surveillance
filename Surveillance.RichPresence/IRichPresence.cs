using System;
using System.Threading.Tasks;
using Surveillance.App;

namespace Surveillance.RichPresence
{
    public interface IRichPresence : IDisposable
    {
        int UpdateRate { get; }

        Task Init(ISurveillanceApp app);
        void PollEvents();

        void UpdateGameState(GameState gameState);
    }
}