using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Surveillance.App;

namespace Surveillance.RichPresence.Tray
{
    public class TrayIcon : IDisposable
    {
        private readonly NotifyIcon _icon;

        public TrayIcon()
        {
            _icon = new NotifyIcon
            {
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };
        }

        public ToolStripMenuItem AddItem(string text)
        {
            var menuItem = new ToolStripMenuItem(text);
            _icon.ContextMenuStrip.Items.Add(menuItem);
            return menuItem;
        }

        public ToolStripMenuItem AddItem(string text, EventHandler click)
        {
            var menuItem = CreateItem(text, click);
            _icon.ContextMenuStrip.Items.Add(menuItem);
            return menuItem;
        }

        public void SetText(string text)
        {
            _icon.Text = text;
        }

        public void SetIcon(string name)
        {
            var resourceStream = LoadResource(name + ".ico");
            if (resourceStream != null)
                _icon.Icon = new Icon(resourceStream);
            else
            {
                var stream = LoadResource("app.ico");
                _icon.Icon = new Icon(stream);
            }
        }

        public void Dispose()
        {
            _icon.Visible = false;
            _icon.Dispose();
        }

        private static Stream LoadResource(string name)
        {
            var type = typeof(SurveillanceApp);
            var path = type.Namespace + ".Resources." + name;
            var resourceStream = type.Assembly.GetManifestResourceStream(path);
            return resourceStream;
        }
        
        public static ToolStripMenuItem CreateItem(string text, EventHandler click)
        {
            var menuItem = new ToolStripMenuItem(text);
            menuItem.Click += click;
            return menuItem;
        }
    }
}