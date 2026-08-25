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
        private readonly PanelToggle[] _toggles;

        public ControllerRenderer(PanelToggle[] toggles)
        {
            _toggles = toggles;
        }

        public void Render()
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;

            DrawHelper.DrawBorderedBackground(new Rectangle(default, ControllerLayoutCalculator.GetSidePanelSize(_toggles.Length)), 
                                              theme.GetColor(ColorRegistry.BorderId), theme.GetColor(ColorRegistry.SurfaceId), 1);

            for (int i = 0; i < _toggles.Length; i++)
            {
                PanelToggle toggle = _toggles[i];
                Rectangle iconBounds = ControllerLayoutCalculator.GetContextPanelIconBounds(i);

                DrawHelper.DrawTextureCenteralized(iconBounds, theme.GetIcon(toggle.Panel.IconId));

                // replace with a vertical bar that has animation when switched to another panel, also shows the current open panel
                if (toggle.IsChecked)
                {
                    DrawHelper.DrawBackground(ControllerLayoutCalculator.GetOpenPanelBarBounds(i), theme.GetColor(ColorRegistry.VerticalBarColorId));
                }
            }
        }
    }
}
