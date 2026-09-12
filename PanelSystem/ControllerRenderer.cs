using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using SideKicker.Theming;
using System;
using System.Collections.Generic;
using System.Text;

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
            ResolvedTheme theme = ThemeService.CurrentTheme;

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);

            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, _windowSize), 
                                              theme.GetColor(ColorRegistry.BorderId), theme.GetColor(ColorRegistry.SurfaceId), 1);

            for (int i = 0; i < toggles.Length; i++)
            {
                PanelToggle toggle = toggles[i];
                Rectangle iconBounds = ControllerLayoutCalculator.GetContextPanelIconBounds(i);

                DrawHelper.DrawTextureCenteralized(iconBounds, theme.GetIcon(toggle.Panel.IconId));

                // replace with a vertical bar that has animation when switched to another panel, also shows the current open panel
                if (toggle.IsChecked)
                {
                    DrawHelper.DrawBackground(ControllerLayoutCalculator.GetOpenPanelBarBounds(i), theme.GetColor(ColorRegistry.VerticalBarColorId));
                }
            }

            DrawHelper.EndBatch();
        }
    }
}
