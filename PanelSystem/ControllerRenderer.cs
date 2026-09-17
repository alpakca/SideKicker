using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using SideKicker.Rendering;

namespace SideKicker.PanelSystem
{
    internal class ControllerRenderer
    {
        private readonly Point _windowSize;
        private readonly RenderTarget2D _windowTexture;

        public ControllerRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);
        }

        public void Render(PanelToggle[] toggles)
        {
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);
            PanelElementRenderer.RenderBackground(_windowSize);

            for (int i = 0; i < toggles.Length; i++)
            {
                PanelToggle toggle = toggles[i];
                Rectangle iconBounds = ControllerLayoutCalculator.GetContextPanelIconBounds(i);

                DrawHelper.DrawTextureCenteralized(iconBounds, RenderData.GetPanelIcon(toggle.Panel.Type));

                // replace with a vertical bar that has animation when switched to another panel, also shows the current open panel
                if (toggle.IsChecked)
                {
                    DrawHelper.DrawBackground(ControllerLayoutCalculator.GetOpenPanelBarBounds(i), RenderData.VerticalBar);
                }
            }

            DrawHelper.EndBatch();
        }
    }
}
