using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Surface;
using SideKicker.Panels;
using Microsoft.Xna.Framework;
using System;

namespace SideKicker.PanelSystem
{
    internal class PanelWindow : PopWindow
    {
        private readonly PanelToggle[] _panelToggles;
        private readonly ControllerRenderer _renderer;

        public event Action<ContextPanel> ContextPanelToggled;

        public PanelWindow(ContextPanel[] panels)
        {
            _panelToggles = new PanelToggle[panels.Length];
            InitializeToggles(panels);

            Elements.AddRange(_panelToggles);

            Boundaries = new Rectangle(default, ControllerLayoutCalculator.GetSidePanelSize(panels.Length));
            _renderer = new ControllerRenderer(WindowTexture);
        }

        private void InitializeToggles(ContextPanel[] panels)
        {
            SelectablePacket selectPacket = new SelectablePacket();
            HoldablePacket holdPacket = new HoldablePacket();

            ActionCommander<ToggleButton> toggleCommand = new ActionCommander<ToggleButton>(button =>
            {
                ContextPanelToggled?.Invoke(((PanelToggle)button).Panel);
            });

            for (int i = 0; i < panels.Length; i++)
            {
                _panelToggles[i] = new PanelToggle
                {
                    Panel = panels[i],

                    Boundaries = ControllerLayoutCalculator.GetContextPanelIconBounds(i),
                    ToggleCommand = toggleCommand,

                    SelectablePacket = selectPacket,
                    HoldPacket = holdPacket
                };
            }
        }

        protected override void RenderUpdate()
        {
            _renderer.Render(_panelToggles);
        }

        public void OpenPanelUpdated(ContextPanel openedPanel)
        {
            foreach (var toggle in _panelToggles)
            {
                toggle.SetStatusWithoutCommander(setToChecked: toggle.Panel == openedPanel);
            }
        }

        public void PanelOrderSwapped(int firstPanelIndex, int secondPanelIndex)
        {
            // swap toggle's order
            (_panelToggles[firstPanelIndex], _panelToggles[secondPanelIndex]) = (_panelToggles[secondPanelIndex], _panelToggles[firstPanelIndex]);

            // adjust boundaries of swapped toggles
            _panelToggles[firstPanelIndex].Boundaries = ControllerLayoutCalculator.GetContextPanelIconBounds(firstPanelIndex);
            _panelToggles[secondPanelIndex].Boundaries = ControllerLayoutCalculator.GetContextPanelIconBounds(secondPanelIndex);
        }
    }
}
