using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace Surveillance.RichPresence.Tray
{
    public class TrayRichPresence : IRichPresence
    {
        public int UpdateRate => 0;

        private IApplication _application;
        private TrayIcon _icon;

        public void Init(IApplication application)
        {
            _application = application;

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
            _application.Close();
        }

        public void PollEvents()
        {
        }

        public void UpdateActivity(string character, string item, string details)
        {
            _icon.SetText($"{character} - {item}");
            _icon.SetIcon(details);
        }

        public void Dispose()
        {
            _icon.Dispose();
            Application.Exit();
        }
    }
}