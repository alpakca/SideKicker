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
        private const int TimersNumberTextSize = 16;

        private readonly Point _windowSize;
        private readonly RenderTarget2D _windowTexture;
        private readonly ButtonListRendering<ITimeInfo, TimesListRegion> _timesListRendering;

        private DateTime _timesListDateNow; // for rendering display strings of ITimeInfo(s)

        public ContextWindowRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);

            _timesListRendering = new ButtonListRendering<ITimeInfo, TimesListRegion>(
                cachedFileIconGatherer: null,
                slidingTextGatherer: (_, timeInfo) => timeInfo.DisplayString(_timesListDateNow),
                commonIconPredicate: (_, _) => true);

            _timesListRendering.AddRegionSlidingText(TimesListRegion.TimeInfoDescription, TimesListLayoutCalculator.TimesListTextSize);
            _timesListRendering.AddRegionCommonIcon(TimesListRegion.EditButton, IconType.EditButton);
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
            FontService.DrawText(AlarmTextSize, calendar.ShownDateText, new Vector2(20, 45), RenderData.Text);

            // alarm days
            for (int i = 1; i <= calendar.MaxDays; i++)
            {
                (int dayColumn, int weekRow) = calendar.GetDayIndexPosition(i);
                Rectangle buttonRelativeBounds = calendar.GetDayButtonBounds(weekRow, dayColumn);
                Rectangle buttonScreenBounds = new Rectangle(calendar.Location + buttonRelativeBounds.Location, buttonRelativeBounds.Size);

                FontService.DrawCenteredText(AlarmTextSize, i.ToString(), buttonScreenBounds, RenderData.Text);

                if (!anyWeekdaysSelected && i == calendar.SelectedDay)
                {
                    DrawHelper.DrawBackground(buttonScreenBounds, RenderData.HoverOverlay);
                }
            }

            // alarm weekdays
            for (int i = 0; i < weekdaysButtons.Length; i++)
            {
                FontService.DrawCenteredText(12, CalendarShowcase.WeekDayNames[i], weekdaysButtons[i].Boundaries, RenderData.Text);

                if (anyWeekdaysSelected && weekdaysButtons[i].IsChecked)
                {
                    DrawHelper.DrawBackground(weekdaysButtons[i].Boundaries, RenderData.HeldOverlay);
                }
            }
        }

        public void RenderTextBox(MultiLineTextBox textbox)
        {
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderMultiLineTextBox(textbox);
            DrawHelper.EndBatch();
        }

        public void RenderScrollableButtonList(ScrollableButtonList timesList, IReadOnlyList<ITimeInfo> registeredTimers)
        {
            _timesListDateNow = DateTime.Now;

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderScrollableButtonListBackground(timesList, registeredTimers, _timesListRendering, TimesListLayoutCalculator.Instance);
            DrawHelper.EndBatch();

            for (int i = 0; i < registeredTimers.Count; i++)
            {
                foreach (TimesListRegion slidingTextRegion in _timesListRendering.SlidingTextRegions)
                {
                    DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
                    PanelElementRenderer.RenderScrollableButtonListSlidingText(timesList, slidingTextRegion, i, registeredTimers[i],
                                                                               _timesListRendering, TimesListLayoutCalculator.Instance);
                    DrawHelper.EndBatch();
                }
            }

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderScrollableButtonListHoverOverlay(timesList, _timesListRendering, TimesListLayoutCalculator.Instance);
            DrawHelper.EndBatch();
        }

        public void RenderNumberSelectors(IList<ScrollableNumberSelector> selectorGroup)
        {
            Color textColor = RenderData.Text;
            int lineSpacing = FontService.GetLineSpacing(TimersNumberTextSize);

            int index = 0;
            foreach (var selector in selectorGroup)
            {
                int marginBetween = (selector.Boundaries.Height - 3 * lineSpacing) / 2;
                int startX = selector.Boundaries.X;
                int startY = selector.Boundaries.Y;

                FontService.DrawText(TimersNumberTextSize, selector.PreviousNumberFormatted, new Vector2(startX, startY), textColor * 0.5f);
                FontService.DrawText(TimersNumberTextSize, selector.CurrentNumberFormatted, new Vector2(startX, startY + marginBetween + lineSpacing), textColor);
                FontService.DrawText(TimersNumberTextSize, selector.NextNumberFormatted, new Vector2(startX, startY + 2 * (marginBetween + lineSpacing)), textColor * 0.5f);

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

            FontService.DrawText(TimersNumberTextSize, elapsed.Hours.ToString("00"), new Vector2(LeftMostStartX, NumberY), textColor);
            FontService.DrawText(TimersNumberTextSize, elapsed.Minutes.ToString("00"), new Vector2(LeftMostStartX + NumberTotalWidth, NumberY), textColor);
            FontService.DrawText(TimersNumberTextSize, elapsed.Seconds.ToString("00"), new Vector2(LeftMostStartX + 2 * NumberTotalWidth, NumberY), textColor); 
            FontService.DrawText(TimersNumberTextSize, elapsed.Milliseconds.ToString("000"), new Vector2(LeftMostStartX + 3 * NumberTotalWidth, NumberY), textColor);

            // draw middle points
            int yMiddle = NumberY + FontService.GetLineSpacing(TimersNumberTextSize) / 2;
            for (int i = 0; i < 3; i++)
            {
                int xMiddle = LeftMostStartX + NumberWidth + i * NumberTotalWidth + (NumbersMargin / 2);

                DrawHelper.DrawBackground(new Rectangle(xMiddle, yMiddle, 2, 2), textColor);
                DrawHelper.DrawBackground(new Rectangle(xMiddle, yMiddle + 6, 2, 2), textColor);
            }
        }
    }
}
