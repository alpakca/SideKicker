using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
using SideKicker.Rendering;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.Settings
{
    internal class ContextWindowRenderer
    {
        private readonly Point _windowSize;
        private readonly RenderTarget2D _windowTexture;

        public ContextWindowRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);
        }

        public void Render(TextPlaque[] plaques, IconedButton closeButton, IEnumerable<ToggleButton> toggles, SectionSwaper swaper, 
                           ContextPanelType[] panelOrder, OriginMover mover)
        {
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);
            PanelElementRenderer.RenderBackground(_windowSize);
            PanelElementRenderer.RenderTextPlaque(plaques);
            PanelElementRenderer.RenderIconedButton(closeButton);
            PanelElementRenderer.RenderToggleButton(toggles);
            RenderSectionSwapper(swaper, panelOrder);
            RenderOriginMover(mover);
            DrawHelper.EndBatch();
        }

        private void RenderSectionSwapper(SectionSwaper swaper, ContextPanelType[] panelOrder)
        {
            for (int i = 0; i < panelOrder.Length; i++)
            {
                Rectangle relativeBounds = swaper.GetSectionBounds(i);
                Rectangle screenBounds = new Rectangle(swaper.Location + relativeBounds.Location, relativeBounds.Size);

                DrawHelper.Batch.Draw(RenderData.GetPanelIcon(panelOrder[i]), screenBounds, Color.White);
            }
        }

        private void RenderOriginMover(OriginMover mover)
        {
            DrawHelper.DrawBackground(mover.Boundaries, Color.DarkKhaki);
        }
    }
}
