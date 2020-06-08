using System;
using System.Threading.Tasks;

namespace Surveillance.App.RichPresence
{
    public interface IRichPresence : IDisposable
    {
        int UpdateRate { get; }

        Task Init(SurveillanceApp app);
        void PollEvents();

        void UpdateGameState(GameState gameState);
    }
}