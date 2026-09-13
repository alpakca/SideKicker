using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
using SideKicker.Theming;
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

        public void BeginRendering()
        {
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);
        }

        public void EndRendering()
        {
            DrawHelper.EndBatch();
        }

        public void RenderMainElements(TextPlaque[] plaques, IconedButton closeButton)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);

            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, _windowSize), theme.GetColor(ColorRegistry.BorderId),
                                              theme.GetColor(ColorRegistry.SurfaceId), 1);

            foreach (var plaque in plaques)
            {
                FontService.Regular.DrawText(plaque.TextSize, plaque.Text, plaque.Position, textColor);
            }

            DrawHelper.Batch.Draw(theme.GetIcon(closeButton.IconId), closeButton.Boundaries, Color.White);
            if (closeButton.IsHovered)
            {
                DrawHelper.DrawBackground(closeButton.Boundaries, closeButton.IsHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                                     : theme.GetColor(ColorRegistry.HoverOverlayId));
            }
        }

        public void RenderSettingToggles(IEnumerable<ToggleButton> toggles)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;

            Color background = theme.GetColor(ColorRegistry.BackgroundId);
            Texture2D checkedIcon = theme.GetIcon(IconRegistry.CheckedToggleButtonIcon);

            foreach (var toggle in toggles)
            {
                DrawHelper.DrawBackground(toggle.Boundaries, background);
               
                if (toggle.IsChecked)
                {
                    DrawHelper.Batch.Draw(checkedIcon, toggle.Boundaries, Color.White);
                }
            }
        }

        public void RenderSectionSwapper(SectionSwaper swaper, ContextPanelType[] panelOrder)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;

            for (int i = 0; i < panelOrder.Length; i++)
            {
                Rectangle relativeBounds = swaper.GetSectionBounds(i);
                Rectangle screenBounds = new Rectangle(swaper.Location + relativeBounds.Location, relativeBounds.Size);

                DrawHelper.Batch.Draw(theme.GetIcon(PanelIconValues.GetIconId(panelOrder[i])), screenBounds, Color.White);
            }
        }

        public void RenderOriginMover(OriginMover mover)
        {
            DrawHelper.DrawBackground(mover.Boundaries, Color.DarkKhaki);
        }
    }
}
