using System;

namespace Surveillance.RichPresence
{
    public interface IRichPresence : IDisposable
    {
        int UpdateRate { get; }

        void Init(IApplication application);
        void PollEvents();

        void UpdateActivity(string character, string item, string details);
    }
}