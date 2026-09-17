using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
using SideKicker.Rendering;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.Timers
{
    internal class ContextWindowRenderer
    {
        private const int AlarmTextSize = 12;
        private const int TimesListTextSize = 12;
        private const int TimersNumberTextSize = 16;
        private static readonly int TimesListTextLineSpacing = FontService.Regular.GetLineSpacing(TimesListTextSize);

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

        public void Render(TextPlaque[] textPlaques, CalendarShowcase calendar, ToggleButton[] weekdaysButtons, bool anyWeekdaysSelected)
        {
            PanelElementRenderer.RenderBackground(_windowSize);
            PanelElementRenderer.RenderTextPlaque(textPlaques);

            // calendary current month/year
            FontService.Regular.DrawText(AlarmTextSize, calendar.ShownDateText, new Vector2(20, 45), RenderData.Text);

            // alarm days
            for (int i = 1; i <= calendar.MaxDays; i++)
            {
                (int dayColumn, int weekRow) = calendar.GetDayIndexPosition(i);
                Rectangle buttonRelativeBounds = calendar.GetDayButtonBounds(weekRow, dayColumn);
                Rectangle buttonScreenBounds = new Rectangle(calendar.Location + buttonRelativeBounds.Location, buttonRelativeBounds.Size);

                FontService.Regular.DrawCenteredText(AlarmTextSize, i.ToString(), buttonScreenBounds, RenderData.Text);

                if (!anyWeekdaysSelected && i == calendar.SelectedDay)
                {
                    DrawHelper.DrawBackground(buttonScreenBounds, RenderData.HoverOverlay);
                }
            }

            // alarm weekdays
            for (int i = 0; i < weekdaysButtons.Length; i++)
            {
                FontService.Regular.DrawCenteredText(12, CalendarShowcase.WeekDayNames[i], weekdaysButtons[i].Boundaries, RenderData.Text);

                if (anyWeekdaysSelected && weekdaysButtons[i].IsChecked)
                {
                    DrawHelper.DrawBackground(weekdaysButtons[i].Boundaries, RenderData.HeldOverlay);
                }
            }
        }

        public void RenderIconedButtons(params ReadOnlySpan<IconedButton> buttons) => PanelElementRenderer.RenderIconedButton(buttons);
        public void RenderTextedButtons(params ReadOnlySpan<TextedButton> buttons) => PanelElementRenderer.RenderTextedButton(buttons);

        public void RenderTextBox(MultiLineTextBox textbox)
        {
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderMultiLineTextBox(textbox);
            DrawHelper.EndBatch();
        }

        public void RenderNumberSelectors(IList<ScrollableNumberSelector> selectorGroup)
        {
            Color textColor = RenderData.Text;
            int lineSpacing = FontService.Regular.GetLineSpacing(TimersNumberTextSize);

            int index = 0;
            foreach (var selector in selectorGroup)
            {
                int marginBetween = (selector.Boundaries.Height - 3 * lineSpacing) / 2;
                int startX = selector.Boundaries.X;
                int startY = selector.Boundaries.Y;

                FontService.Regular.DrawText(TimersNumberTextSize, selector.PreviousNumberFormatted, new Vector2(startX, startY), textColor * 0.5f);
                FontService.Regular.DrawText(TimersNumberTextSize, selector.CurrentNumberFormatted, new Vector2(startX, startY + marginBetween + lineSpacing), textColor);
                FontService.Regular.DrawText(TimersNumberTextSize, selector.NextNumberFormatted, new Vector2(startX, startY + 2 * (marginBetween + lineSpacing)), textColor * 0.5f);

                // if not on last selector, draw middle points
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

        public void RenderStopwatch(StopwatchShowcase stopwatch)
        {
            Color textColor = RenderData.Text;
            TimeSpan elapsed = stopwatch.Elapsed;

            const int LeftMostStartX = 290;
            const int NumbersMargin = 10;
            const int NumberWidth = 20;
            const int NumberTotalWidth = NumberWidth + NumbersMargin;
            const int NumberY = 260;

            FontService.Regular.DrawText(TimersNumberTextSize, elapsed.Hours.ToString("00"), new Vector2(LeftMostStartX, NumberY), textColor);
            FontService.Regular.DrawText(TimersNumberTextSize, elapsed.Minutes.ToString("00"), new Vector2(LeftMostStartX + NumberTotalWidth, NumberY), textColor);
            FontService.Regular.DrawText(TimersNumberTextSize, elapsed.Seconds.ToString("00"), new Vector2(LeftMostStartX + 2 * NumberTotalWidth, NumberY), textColor); 
            FontService.Regular.DrawText(TimersNumberTextSize, elapsed.Milliseconds.ToString("000"), new Vector2(LeftMostStartX + 3 * NumberTotalWidth, NumberY), textColor);

            // draw middle points
            int yMiddle = NumberY + FontService.Regular.GetLineSpacing(TimersNumberTextSize) / 2;
            for (int i = 0; i < 3; i++)
            {
                int xMiddle = LeftMostStartX + NumberWidth + i * NumberTotalWidth + (NumbersMargin / 2);

                DrawHelper.DrawBackground(new Rectangle(xMiddle, yMiddle, 2, 2), textColor);
                DrawHelper.DrawBackground(new Rectangle(xMiddle, yMiddle + 6, 2, 2), textColor);
            }
        }

        public void RenderTimesListBackgrounds(ScrollableButtonList timesList)
        {
            DrawHelper.Device.ScissorRectangle = timesList.Boundaries;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            for (int i = 0; i < timesList.ButtonCount; i++)
            {
                Rectangle relativeButtonBounds = timesList.GetButtonBounds(i);
                Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + timesList.Boundaries.Location, relativeButtonBounds.Size);

                DrawHelper.Batch.Draw(RenderData.GetIcon(IconType.EditButton), TimesListLayoutCalculator.EditButtonBounds(buttonScreenBounds), Color.White);

                if (i == timesList.HoveredButtonIndex)
                {
                    bool hitEditButton = TimesListLayoutCalculator.HitEditButton(buttonScreenBounds.Size, timesList.RelativeMouseToHovered);
                    Rectangle overlayBounds = hitEditButton ? TimesListLayoutCalculator.EditButtonBounds(buttonScreenBounds) : buttonScreenBounds;

                    DrawHelper.DrawBackground(overlayBounds, timesList.IsHoveredButtonHeld ? RenderData.HeldOverlay : RenderData.HoverOverlay);
                }
            }

            DrawHelper.EndBatch();
        }

        public void RenderTimesListText(ScrollableButtonList timesList, int buttonIndex, string buttonName)
        {
            Rectangle relativeButtonBounds = timesList.GetButtonBounds(buttonIndex);
            Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + timesList.Boundaries.Location, relativeButtonBounds.Size);
            Rectangle buttonNameScreenBounds = TimesListLayoutCalculator.ButtonNameBounds(buttonScreenBounds);

            int verticalCenterOffset = TimesListTextLineSpacing / 2;
            Vector2 textPosition = buttonNameScreenBounds.Location.ToVector2() + new Vector2(0, verticalCenterOffset);

            // apply sliding offset
            if (buttonIndex == timesList.HoveredButtonIndex)
            {
                float textWidth = FontService.Regular.MeasureWidth(TimesListTextSize, buttonName);
                textPosition.X += TimesListLayoutCalculator.GetTimesListTextSlideOffset(textWidth, buttonScreenBounds.Width);
            }

            // we need intersect here since button may have been cut through if because of scrolling, so text should be cut too
            DrawHelper.Device.ScissorRectangle = Rectangle.Intersect(timesList.Boundaries, buttonNameScreenBounds);
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            FontService.Regular.DrawText(TimesListTextSize, buttonName, textPosition, RenderData.Text);

            DrawHelper.EndBatch();
        }
    }
}
