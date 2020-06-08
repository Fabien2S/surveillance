using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Surveillance.App;
using Surveillance.App.RichPresence;

namespace Surveillance.RichPresence.Tray
{
    public class TrayRichPresence : IRichPresence
    {
        public int UpdateRate => 0;

        private SurveillanceApp _app;
        private TrayIcon _icon;


        public Task Init(SurveillanceApp app)
        {
            _app = app;

            new Thread(Run)
            {
                Name = "UI",
                IsBackground = true,
                CurrentCulture = CultureInfo.InvariantCulture,
                CurrentUICulture = CultureInfo.InvariantCulture
            }.Start();

            return Task.CompletedTask;
        }

        private void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _icon = new TrayIcon();
            _icon.SetText("Surveillance");
            _icon.SetIcon("app");
            _icon.AddItem("Close", HandleCloseButton);

            Application.Run();
        }

        private void HandleCloseButton(object sender, EventArgs e)
        {
            _app.Close();
        }

        public void PollEvents()
        {
        }

        public void UpdateGameState(GameState gameState)
        {
            _icon.SetText(gameState.CharacterString + " (" + gameState.ActionString + ")");
            
            var gameCharacter = gameState.Character;
            _icon.SetIcon("character." + gameCharacter.Type + "." + gameCharacter.Name);
        }

        public void Dispose()
        {
            _icon.Dispose();
            Application.Exit();
        }
    }
}