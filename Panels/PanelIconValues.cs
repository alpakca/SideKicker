using SideKicker.Theming;
using System;
using System.Runtime.CompilerServices;

namespace SideKicker.Panels
{
    internal static class PanelIconValues
    {
        public const string PinnedCommands_IconId = "PinnedCommands";
        public const string PinnedCommands_IconPath = @"panels\PinnedActionsPanelIcon.png";

        public const string Timers_IconId = "Timers";
        public const string Timers_IconPath = @"panels\TimersPanelIcon.png";

        public const string DesktopSearcher_IconId = "DesktopSearcher";
        public const string DesktopSearcher_IconPath = @"panels\DesktopSearcherPanelIcon.png";

        public const string Settings_IconId = "Settings";
        public const string Settings_IconPath = @"panels\SettingsPanelIcon.png";

        [ModuleInitializer]
        public static void InitializeIcons()
        {
            IconRegistry.Register(new IconDefinition
            {
                Id = PinnedCommands_IconId,
                DefaultPath = PinnedCommands_IconPath,
            });

            IconRegistry.Register(new IconDefinition
            {
                Id = Timers_IconId,
                DefaultPath = Timers_IconPath
            });

            IconRegistry.Register(new IconDefinition
            {
                Id = DesktopSearcher_IconId,
                DefaultPath = DesktopSearcher_IconPath
            });

            IconRegistry.Register(new IconDefinition
            {
                Id = Settings_IconId,
                DefaultPath = Settings_IconPath
            });
        }

        public static string GetIconId(ContextPanelType type)
        {
            return type switch
            {
                ContextPanelType.PinnedCommands => PinnedCommands_IconId,
                ContextPanelType.Timers => Timers_IconId,
                ContextPanelType.DesktopSearcher => DesktopSearcher_IconId,
                ContextPanelType.Settings => Settings_IconId,

                _ => throw new ArgumentException()
            };
        }
    }
}
