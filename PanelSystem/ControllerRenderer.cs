using Microsoft.Xna.Framework;
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
        private readonly ControllerLayoutCalculator _layoutCalculator;

        public ControllerRenderer(ControllerLayoutCalculator layoutCalculator)
        {
            _layoutCalculator = layoutCalculator;
        }

        public void Render(PanelToggle[] toggles, Point windowLocation)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;

            DrawHelper.DrawBorderedBackground(new Rectangle(windowLocation, _layoutCalculator.GetSidePanelSize(toggles.Length)), 
                                              theme.GetColor(ColorRegistry.BorderId), theme.GetColor(ColorRegistry.SurfaceId), 1);

            for (int i = 0; i < toggles.Length; i++)
            {
                PanelToggle toggle = toggles[i];
                Rectangle iconBounds = _layoutCalculator.GetContextPanelIconBounds(i);

                DrawHelper.DrawTextureCenteralized(iconBounds, theme.GetIcon(toggle.Panel.IconId));

                // replace with a vertical bar that has animation when switched to another panel, also shows the current open panel
                if (toggle.IsChecked)
                {
                    DrawHelper.DrawBackground(_layoutCalculator.GetOpenPanelBarBounds(i), theme.GetColor(ColorRegistry.VerticalBarColorId));
                }
            }
        }
    }
}
