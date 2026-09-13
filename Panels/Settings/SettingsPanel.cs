using SideKicker.PanelSystem.Message;
using SideKicker.PanelSystem.Settings;
using Microsoft.Xna.Framework;
using System;

namespace SideKicker.Panels.Settings
{
    internal class SettingsPanel : ContextPanel
    {
        private readonly Preferences _preferences;
        private readonly ContextWindow _window;

        public override string IconId => PanelIconValues.Settings_IconId;
        public override string IconPath => PanelIconValues.Settings_IconPath;
        public override ContextPanelType Type => ContextPanelType.Settings;
        public override MessageSupporterWindow Window => _window;

        public event Action AppCloseRequested;

        public SettingsPanel(Preferences preferences)
        {
            _preferences = preferences;

            _window = new ContextWindow();
            _window.MoveOriginY += Window_MoveOriginY;
            _window.SwapPanels += Window_SwapPanels;
            _window.CloseApp += Window_CloseApp;

            _window.HoverOpenCloseEnableUpdated += isEnabled => _preferences.EnableHoverOpenClose = isEnabled;
        }

        private void Window_CloseApp()
        {
            AppCloseRequested?.Invoke();
        }

        private void Window_SwapPanels(int firstPanelIndex, int secondPanelIndex)
        {
            _preferences.ReorderPanels(firstPanelIndex, secondPanelIndex);
        }

        private void Window_MoveOriginY(int differenceY)
        {
            _preferences.ChangeOrigin(new Point(_preferences.OriginX, _preferences.OriginY + differenceY));
        }

        public override void Update()
        {
        }

        public override void OnExiting()
        {
        }
    }
}
