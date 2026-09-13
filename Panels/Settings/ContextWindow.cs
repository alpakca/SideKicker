using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using SideKicker.PanelSystem.Message;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.Settings
{
    internal class ContextWindow : MessageSupporterWindow
    {
        private const int HeaderTextSize = 20;

        private readonly TextPlaque _headerText;
        private readonly Clickable _closeAppButton;
        private readonly SectionSwaper _panelSwapper;
        private readonly OriginMover _originMover;
        private readonly List<SettingsToggle> _optionToggles;

        public event Action CloseApp;
        public event Action<int, int> SwapPanels;
        public event Action<int> MoveOriginY;

        public event Action<bool> HoverOpenCloseEnableUpdated;

        public ContextWindow()
        {
            _headerText = new TextPlaque
            {
                Text = "Settings",
                TextSize = HeaderTextSize,
                Position = new Vector2(20, 10)
            };

            _closeAppButton = new Clickable
            {
                Boundaries = new Rectangle(),

                ClickCommand = new ActionCommander<Clickable>(_ => CloseApp?.Invoke()),

                SelectablePacket = new SelectablePacket(),
                HoldPacket = new HoldablePacket()
            };

            _panelSwapper = new SectionSwaper
            {
                SectionLength = 0,
                SectionMargin = 0,
                Boundaries = new Rectangle()
            };
            _panelSwapper.SectionsSwapped += PanelSwapper_SectionsSwapped;

            _originMover = new OriginMover
            {
                Boundaries = new Rectangle()
            };
            _originMover.MouseVerticalDifferenceUpdated += OriginMover_MouseMovedVertically;

            _optionToggles = new List<SettingsToggle>
            {
                new SettingsToggle
                {
                    OptionText = "Enable hovered open-close",
                    Boundaries = new Rectangle(),

                    ToggleCommand = new ActionCommander<ToggleButton>(button => HoverOpenCloseEnableUpdated?.Invoke(button.IsChecked)),

                    SelectablePacket = new SelectablePacket(),
                    HoldPacket = new HoldablePacket()
                }
            };
        }

        private void PanelSwapper_SectionsSwapped(int firstIndex, int secondIndex)
        {
            SwapPanels?.Invoke(firstIndex, secondIndex);
        }

        private void OriginMover_MouseMovedVertically(int differenceY)
        {
            MoveOriginY?.Invoke(differenceY);
        }
    }
}
