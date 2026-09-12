using SideKicker.PanelSystem.Message;
using SideKicker.PanelSystem.Settings;

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

        public SettingsPanel(Preferences preferences)
        {
            _preferences = preferences;
        }

        public override void Update()
        {
        }

        public override void OnExiting()
        {
        }
    }
}
