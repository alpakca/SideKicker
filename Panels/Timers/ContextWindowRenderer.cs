using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
using SideKicker.Theming;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.Timers
{
    internal class ContextWindowRenderer
    {
        private readonly RenderTarget2D _windowTexture;
        private readonly Point _windowSize;

        public ContextWindowRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);
        }

        public void StartMainBatch()
        {
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);
        }

        public void EndMainBatch()
        {
            DrawHelper.EndBatch();
        }

        public void RenderMainElements(TextPlaque[] textPlaques, CalendarShowcase calendar)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);

            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, _windowSize), theme.GetColor(ColorRegistry.BorderId), theme.GetColor(ColorRegistry.SurfaceId), 1);

            // plaques
            foreach (var plaque in textPlaques)
            {
                FontService.Regular.DrawText(plaque.TextSize, plaque.Text, plaque.Position, textColor);
            }

            // calendar
            FontService.Regular.DrawText(12, calendar.ShownDateText, calendar.ShownMonthDisplayBounds().Location.ToVector2(), textColor);

            DrawHelper.DrawBackground(calendar.PreviousMonthButtonBounds(), Color.Red); // replace with icon draw
            DrawHelper.DrawBackground(calendar.NextMonthButtonBounds(), Color.Blue); // replace with icon draw

            int dayIndex = 0;
            foreach (var dayName in CalendarShowcase.WeekDayNames)
            {
                FontService.Regular.DrawCenteredText(12, dayName, calendar.GetWeekDayBounds(dayIndex), textColor);
                dayIndex++;
            }

            for (int i = 1; i <= calendar.MaxDays; i++)
            {
                (int dayColumn, int weekRow) = calendar.GetDayIndexPosition(i);
                Rectangle buttonRelativeBounds = calendar.GetDayButtonBounds(weekRow, dayColumn);
                Rectangle buttonScreenBounds = new Rectangle(calendar.Location + buttonRelativeBounds.Location, buttonRelativeBounds.Size);

                FontService.Regular.DrawCenteredText(16, i.ToString(), buttonScreenBounds, textColor);

                if (i == calendar.SelectedDay)
                {
                    DrawHelper.DrawBackground(buttonScreenBounds, theme.GetColor(ColorRegistry.HoverOverlayId));
                }
            }
        }

        public void RenderButtons(params ReadOnlySpan<Clickable> buttons)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            foreach (var button in buttons)
            {
                DrawHelper.DrawBorderedBackground(button.Boundaries, theme.GetColor(ColorRegistry.BorderAccentId), theme.GetColor(ColorRegistry.BackgroundId), 1);
            }
        }

        public void RenderButtons(params ReadOnlySpan<TextedButton> buttons)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            foreach (var button in buttons)
            {
                DrawHelper.DrawBackground(button.Boundaries, theme.GetColor(ColorRegistry.BackgroundId));
                FontService.Regular.DrawCenteredText(12, button.Text, button.Boundaries, theme.GetColor(ColorRegistry.TextColorId));

                if (button.IsHovered)
                {
                    DrawHelper.DrawBackground(button.Boundaries, button.IsHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                               : theme.GetColor(ColorRegistry.HoverOverlayId));
                }
            }
        }

        public void RenderTextBox(MultiLineTextBox textbox)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);
            int lineSpacing = FontService.Regular.GetLineSpacing(textbox.TextSize);

            DrawHelper.Device.ScissorRectangle = textbox.SelectableArea;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            DrawHelper.DrawBackground(textbox.SelectableArea, theme.GetColor(ColorRegistry.BackgroundId));

            Vector2 position = textbox.Boundaries.Location.ToVector2() - new Vector2(0, textbox.ScrollOffset);
            foreach (var line in textbox.Lines)
            {
                FontService.Regular.DrawText(textbox.TextSize, line, position, textColor);
                position.Y += lineSpacing;
            }

            if (textbox.ReceivingInput)
            {
                const int CaretWidth = 1;
                DrawHelper.DrawBackground(new Rectangle(textbox.Boundaries.Location + textbox.CaretPosition, new Point(CaretWidth, lineSpacing)), textColor);
            }

            DrawHelper.EndBatch();
        }

        public void RenderNumberSelectors(IList<ScrollableNumberSelector> selectorGroup)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);
            int lineSpacing = FontService.Regular.GetLineSpacing(16);

            int index = 0;
            foreach (var selector in selectorGroup)
            {
                int marginBetween = (selector.Boundaries.Height - 3 * lineSpacing) / 2;
                int startX = selector.Boundaries.X;
                int startY = selector.Boundaries.Y;

                FontService.Regular.DrawText(16, selector.PreviousNumberFormatted, new Vector2(startX, startY), textColor * 0.5f);
                FontService.Regular.DrawText(16, selector.CurrentNumberFormatted, new Vector2(startX, startY + marginBetween + lineSpacing), textColor);
                FontService.Regular.DrawText(16, selector.NextNumberFormatted, new Vector2(startX, startY + 2 * (marginBetween + lineSpacing)), textColor * 0.5f);

                // if not on last selector
                if (index != selectorGroup.Count - 1)
                {
                    int xMiddle = selector.Boundaries.Right + (selectorGroup[index + 1].Boundaries.X - selector.Boundaries.Right) / 2;
                    int yMiddle = startY + marginBetween + lineSpacing + lineSpacing / 2;

                    DrawHelper.DrawBackground(new Rectangle(xMiddle, yMiddle, 2, 2), textColor);
                    DrawHelper.DrawBackground(new Rectangle(xMiddle, yMiddle + 6, 2, 2), textColor);

                    index++;
                }
            }
        }

        public void RenderTimesListBackgrounds(ScrollableButtonList timesList)
        {

        }

        public void RenderTimesListText(ScrollableButtonList timesList, int buttonIndex, string buttonName)
        {

        }
    }
}
