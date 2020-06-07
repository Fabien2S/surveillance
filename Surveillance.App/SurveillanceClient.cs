using System;
using System.Linq;
using System.Threading;
using NLog;
using Surveillance.App.Models;
using Surveillance.RichPresence;

namespace Surveillance.App
{
    public class SurveillanceClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IRichPresence[] _richPresences;
        private readonly int _updateRate;

        private bool _running;

        private bool _dirty;
        private GameStateModel _gameState;

        public SurveillanceClient(params IRichPresence[] richPresences)
        {
            _richPresences = richPresences;
            _updateRate = richPresences.Select(rp => rp.UpdateRate).Max();
        }

        public void Run()
        {
            Logger.Info("Starting Surveillance");

            _running = true;

            try
            {
                foreach (var richPresence in _richPresences)
                    richPresence.Init();
            }
            catch (Exception e)
            {
                Logger.Error("Unable to initialize Rich Presence");
                Logger.Error(e);
                return;
            }
            
            try
            {
                while (_running)
                {
                    foreach (var richPresence in _richPresences)
                    {
                        richPresence.PollEvents();
                        if (_dirty)
                        {
                            richPresence.UpdateActivity(
                                _gameState.Character,
                                _gameState.Item,
                                _gameState.Details
                            );
                        }
                    }

                    _dirty = false;
                    Thread.Sleep(_updateRate);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            foreach (var richPresence in _richPresences)
                richPresence.Dispose();

            Logger.Info("Shutting down");
        }
    }
}