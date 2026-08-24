using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using Microsoft.Xna.Framework;

namespace SideKicker.PanelSystem
{
    internal class PanelWindow : PopWindow
    {
        private readonly PanelToggle[] _panelToggles;
        private readonly ControllerLayoutCalculator _layoutCalculator;
        private readonly ControllerRenderer _renderer;

        public PanelWindow(ContextPanel[] panels) : base(new BatchDrawOptions { BlendState = Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend })
        {
            _layoutCalculator = new ControllerLayoutCalculator();
            _renderer = new ControllerRenderer(_layoutCalculator);
            Boundaries = new Rectangle(default, _layoutCalculator.GetSidePanelSize(panels.Length));

            _panelToggles = new PanelToggle[panels.Length];
            InitializeToggles(panels);

            Elements.AddRange(_panelToggles);
        }

        private void InitializeToggles(ContextPanel[] panels)
        {
            SelectablePacket selectPacket = new SelectablePacket();
            HoldablePacket holdPacket = new HoldablePacket();

            ActionCommander<ToggleButton> toggleCommand = new ActionCommander<ToggleButton>(button =>
            {
                foreach (var toggle in _panelToggles) toggle.SetStatusWithoutCommander(false);
                button.SetStatusWithoutCommander(true);
            });

            for (int i = 0; i < panels.Length; i++)
            {
                _panelToggles[i] = new PanelToggle
                {
                    Panel = panels[i],
                    Boundaries = _layoutCalculator.GetContextPanelIconBounds(i),
                    ToggleCommand = toggleCommand,

                    SelectablePacket = selectPacket,
                    HoldPacket = holdPacket
                };
            }
        }

        protected override void RenderUpdate()
        {
            DrawHelper.SetBeginBatch(WindowTexture, DrawOptions);

            _renderer.Render(_panelToggles, this.Boundaries.Location);

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
