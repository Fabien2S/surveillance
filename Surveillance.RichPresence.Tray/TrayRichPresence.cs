using System;
using System.Collections.Generic;
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

        private readonly Dictionary<string, ToolStripMenuItem> _stateItem = new Dictionary<string, ToolStripMenuItem>();
        private ToolStripMenuItem _activeStateItem;
        
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

            BuildGameStateItem();

            var closeItem = _icon.AddItem("Close", HandleCloseButton);
            closeItem.Text = _app.I18N("app.close");

            Application.Run();
        }

        private void BuildGameStateItem()
        {
            var stateItem = _icon.AddItem("game_states");
            stateItem.Text = _app.I18N("app.select_state");

            var categoryItems = new Dictionary<string, ToolStripMenuItem>();
            foreach (var gameState in _app.GameStates)
            {
                var character = gameState.Character;
                var category = character.Type;

                if (!categoryItems.TryGetValue(category, out var categoryItem))
                {
                    categoryItem = new ToolStripMenuItem(category)
                    {
                        Text = _app.I18N("character.role." + character.Type)
                    };

                    stateItem.DropDownItems.Add(categoryItem);
                    categoryItems[category] = categoryItem;
                }

                var path = StateToPath(gameState);
                var item = new ToolStripMenuItem(path)
                {
                    Text = character.DisplayName + " (" + gameState.Action.DisplayName + ")"
                };
                item.Click += (sender, args) => _app.SetGameState(gameState);

                categoryItem.DropDownItems.Add(item);
                _stateItem[path] = item;
            }
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
            var character = gameState.Character;
            _icon.SetText(character.DisplayName + " (" + gameState.Details + ")");

            _icon.SetIcon("character." + character.Type + "." + character.Name);

            if (_activeStateItem != null)
            {
                _activeStateItem.Checked = false;
                _activeStateItem = null;
            }

            var path = StateToPath(gameState);
            if (!_stateItem.TryGetValue(path, out var btn))
                return;
            btn.Checked = true;
            _activeStateItem = btn;
        }

        public void Dispose()
        {
            _icon.Dispose();
            Application.Exit();
        }

        private static string StateToPath(GameState state)
        {
            var character = state.Character;
            var action = state.Action;
            return character.Type + "." + character.Name + "." + action.Type + "." + action.Name;
        }
    }
}