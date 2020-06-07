using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Surveillance.App;

namespace Surveillance.RichPresence.Tray
{
    public class TrayRichPresence : IRichPresence
    {
        public int UpdateRate => 0;

        private ISurveillanceApp _app;
        private TrayIcon _icon;


        public void Init(ISurveillanceApp app)
        {
            _app = app;

            new Thread(Run)
            {
                Name = "UI",
                IsBackground = true,
                CurrentCulture = CultureInfo.InvariantCulture,
                CurrentUICulture = CultureInfo.InvariantCulture
            }.Start();
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

        public void UpdateActivity(GameState gameState)
        {
            _icon.SetText($"{gameState.Character} - {gameState.Action}");
            _icon.SetIcon(gameState.CharacterIcon);
        }

        public void Dispose()
        {
            _icon.Dispose();
            Application.Exit();
        }
    }
}