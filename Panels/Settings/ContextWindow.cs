using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using SideKicker.PanelSystem.Message;
using SideKicker.PanelSystem.Settings;
using SideKicker.Theming;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.Settings
{
    internal class ContextWindow : MessageSupporterWindow
    {
        private const int HeaderTextSize = 20;
        private const int OptionTextSize = 12;

        private readonly TextPlaque[] _plaques;
        private readonly IconedButton _closeAppButton;
        private readonly SectionSwaper _panelSwapper;
        private readonly OriginMover _originMover;
        private readonly Dictionary<SettingToggleType, ToggleButton> _optionToggles;

        private readonly ContextPanelType[] _panelOrder;
        private readonly ContextWindowRenderer _renderer;

        public event Action CloseApp;
        public event Action<SettingToggleType, bool> ToggleStatusChanged;
        public event Action<int, int> PanelSwaped;
        public event Action<int> OriginYOffseted;

        public ContextWindow(ContextPanelType[] panelOrder)
        {
            _panelOrder = panelOrder;

            _plaques = new TextPlaque[]
            {
                new TextPlaque
                {
                    Text = "Settings",
                    TextSize = HeaderTextSize,
                    Position = new Vector2(20, 10)
                },
                new TextPlaque
                {
                    Text = "Close app",
                    TextSize = OptionTextSize,
                    Position = new Vector2(60, 211)
                },
                new TextPlaque
                {
                    Text = "Enable hovered open-close",
                    TextSize = OptionTextSize,
                    Position = new Vector2(60, 57)
                },
            };

            _closeAppButton = new IconedButton
            {
                IconId = IconRegistry.CloseButtonId,
                Boundaries = new Rectangle(20, 208, 32, 32),

                ClickCommand = new ActionCommander<Clickable>(_ => CloseApp?.Invoke()),

                SelectablePacket = new SelectablePacket(),
                HoldPacket = new HoldablePacket()
            };

            _panelSwapper = new SectionSwaper
            {
                SectionLength = 32,
                SectionMargin = 4,
                Location = new Point(300, 50)
            };
            _panelSwapper.SectionsSwapped += PanelSwapper_SectionsSwapped;

            _originMover = new OriginMover
            {
                Boundaries = new Rectangle(320, 210, 20, 40)
            };
            _originMover.MouseVerticalDifferenceUpdated += OriginMover_MouseMovedVertically;

            _optionToggles = new Dictionary<SettingToggleType, ToggleButton>();
            InitializeOptionToggles();

            Elements.AddRange([_closeAppButton, _originMover, .. _optionToggles.Values]);

            this.Boundaries = new Rectangle(Point.Zero, new Point(340, 250));
            _renderer = new ContextWindowRenderer(WindowTexture);
        }

        public void SetUIValues(Preferences preferences)
        {
            _panelSwapper.SectionCount = preferences.PanelOrder.Length;
            _optionToggles[SettingToggleType.EnableHoverOpenClose].SetStatusWithoutCommander(preferences.EnableHoverOpenClose);
        }

        private void PanelSwapper_SectionsSwapped(int firstIndex, int secondIndex)
        {
            PanelSwaped?.Invoke(firstIndex, secondIndex);
        }

        private void OriginMover_MouseMovedVertically(int differenceY)
        {
            OriginYOffseted?.Invoke(differenceY);
        }

        protected override void RenderUpdateWithoutMessages()
        {
            _renderer.BeginRendering();

            _renderer.RenderMainElements(_plaques, _closeAppButton);
            _renderer.RenderSettingToggles(_optionToggles.Values);
            _renderer.RenderSectionSwapper(_panelSwapper, _panelOrder);
            _renderer.RenderOriginMover(_originMover);

            _renderer.EndRendering();
        }

        protected override void OnMousePositionChangeWithoutMessages(PopWindowMouseEventArgs e)
        {
            _panelSwapper.OnMousePositionChanged(e);
            base.OnMousePositionChangeWithoutMessages(e);
        }

        protected override void OnMouseButtonStateChangeWithoutMessages(PopWindowMouseEventArgs e)
        {
            _panelSwapper.OnMouseStateChanged(e);
            base.OnMouseButtonStateChangeWithoutMessages(e);
        }

        private void InitializeOptionToggles()
        {
            _optionToggles[SettingToggleType.EnableHoverOpenClose] = new ToggleButton
            {
                Boundaries = new Rectangle(20, 55, 32, 32),

                ToggleCommand = new ActionCommander<ToggleButton>(button => ToggleStatusChanged?.Invoke(SettingToggleType.EnableHoverOpenClose, button.IsChecked)),

                SelectablePacket = new SelectablePacket(),
                HoldPacket = new HoldablePacket()
            };
        }
    }
}
