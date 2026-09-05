using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using Microsoft.Xna.Framework;
using System;

namespace SideKicker.PanelSystem
{
    internal class PanelWindow : PopWindow
    {
        private readonly PanelToggle[] _panelToggles;
        private readonly ControllerRenderer _renderer;

        public event Action<int> ContextPanelToggled;

        public PanelWindow(ContextPanel[] panels)
        {
            Boundaries = new Rectangle(default, ControllerLayoutCalculator.GetSidePanelSize(panels.Length));

            _panelToggles = new PanelToggle[panels.Length];
            InitializeToggles(panels);
            _renderer = new ControllerRenderer(_panelToggles);

            Elements.AddRange(_panelToggles);
        }

        private void InitializeToggles(ContextPanel[] panels)
        {
            SelectablePacket selectPacket = new SelectablePacket();
            HoldablePacket holdPacket = new HoldablePacket();

            ActionCommander<ToggleButton> toggleCommand = new ActionCommander<ToggleButton>(button =>
            {
                ContextPanelToggled?.Invoke(((PanelToggle)button).PanelIndex);
            });

            for (int i = 0; i < panels.Length; i++)
            {
                _panelToggles[i] = new PanelToggle
                {
                    Panel = panels[i],
                    PanelIndex = i,

                    Boundaries = ControllerLayoutCalculator.GetContextPanelIconBounds(i),
                    ToggleCommand = toggleCommand,

                    SelectablePacket = selectPacket,
                    HoldPacket = holdPacket
                };
            }
        }

        protected override void RenderUpdate()
        {
            DrawHelper.SetBeginBatch(WindowTexture, DrawHelper.AlphaBlendDrawOptions);

            _renderer.Render();

            DrawHelper.EndBatch();
        }

        public void OpenPanelUpdated(int newOpenindex)
        {
            PanelToggle newOpen = _panelToggles[newOpenindex];

            foreach (var toggle in _panelToggles) toggle.SetStatusWithoutCommander(false);
            newOpen.SetStatusWithoutCommander(true);
        }
    }
}
