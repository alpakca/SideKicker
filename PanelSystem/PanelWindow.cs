using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SideKicker.PanelSystem
{
    internal class PanelWindow : PopWindow
    {
        private readonly ContextPanelController _controller;
        private readonly PanelToggle[] _panelToggles;
        private readonly ControllerRenderer _renderer;

        public PanelWindow(ContextPanelController controller, ContextPanel[] panels) 
            : base(new BatchDrawOptions { BlendState = BlendState.AlphaBlend })
        {
            _controller = controller;
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
                _controller.OpenPanelAt(((PanelToggle)button).PanelIndex);
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
            DrawHelper.SetBeginBatch(WindowTexture, DrawOptions);

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
