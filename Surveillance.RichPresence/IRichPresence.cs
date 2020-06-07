using System;
using Surveillance.App;

namespace Surveillance.RichPresence
{
    public interface IRichPresence : IDisposable
    {
        int UpdateRate { get; }

        void Init(ISurveillanceApp app);
        void PollEvents();

        void UpdateActivity(GameState gameState);
    }
}