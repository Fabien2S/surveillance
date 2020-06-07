using NLog;

namespace Surveillance.App
{
    public class SurveillanceClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public void Run()
        {
            Logger.Info("Starting Surveillance");

            
            
            Logger.Info("Shutting down");
        }
    }
}