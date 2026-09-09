using PopUpUI.Toolkit;
using System;

namespace SideKicker.PanelSystem.Settings
{
    internal class SettingsService
    {
        public Preferences Preferences { get; }

        public event Action OriginUpdated;

        public SettingsService()
        {
            Preferences = PreferenceSerializer.Load();
        }

        public void RaiseOriginUpdated() => OriginUpdated?.Invoke();

        public void Save()
        {
            PreferenceSerializer.Save(Preferences);
        }
    }
}
