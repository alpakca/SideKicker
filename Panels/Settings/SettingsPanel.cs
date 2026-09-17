using Microsoft.Xna.Framework;
using SideKicker.PanelSystem.Message;
using SideKicker.PanelSystem.Settings;
using System;

namespace SideKicker.Panels.Settings
{
    internal class SettingsPanel : ContextPanel
    {
        private readonly Preferences _preferences;
        private readonly ContextWindow _window;

        public override ContextPanelType Type => ContextPanelType.Settings;
        public override MessageSupporterWindow Window => _window;

        public event Action AppCloseRequested;

        public SettingsPanel(Preferences preferences)
        {
            _preferences = preferences;

            _window = new ContextWindow(preferences.PanelOrder);
            _window.CloseApp += Window_CloseApp;
            _window.PanelSwaped += Window_PanelSwaped;
            _window.OriginYOffseted += Window_OriginYOffseted;
            _window.ToggleStatusChanged += Window_ToggleStatusChanged;

            _window.SetUIValues(preferences);
        }

        private void Window_ToggleStatusChanged(SettingToggleType type, bool isEnabled)
        {
            switch (type)
            {
                case SettingToggleType.EnableHoverOpenClose:
                    _preferences.EnableHoverOpenClose = isEnabled;
                    break;
            }
        }

        private void Window_OriginYOffseted(int yDifference)
        {
            _preferences.ChangeOrigin(new Point(_preferences.OriginX, _preferences.OriginY + yDifference));
        }

        private void Window_PanelSwaped(int firstPanelIndex, int secondPanelIndex)
        {
            _preferences.ReorderPanels(firstPanelIndex, secondPanelIndex);
        }

        private void Window_CloseApp()
        {
            AppCloseRequested?.Invoke();
        }

        public override void Update()
        {
        }

        public override void OnExiting()
        {
        }
    }
}
