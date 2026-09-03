using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
using SideKicker.Panels.PinnedCommands;
using SideKicker.Theming;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SideKicker.Panels.Timers
{
    internal class ContextWindowRenderer
    {
        public const string AlarmCalendarPreviousMonthButtonId = "AlarmCalendarPreviousMonthButton";
        public const string AlarmCalendarNextMonthButtonId = "AlarmCalendarNextMonthButtonId";
        public const string TimerAddButton = "TimerAddButton";
        public const string TimerDiscardButton = "TimerDiscardButton";
        public const string TimerStopButton = "TimerStopButton";
        public const string TimerSaveButton = "TimerSaveButton";

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

        [ModuleInitializer]
        public static void InitializeIcons()
        {
            IconRegistry.Register(new IconDefinition
            {
                Id = AlarmCalendarNextMonthButtonId,
                DefaultPath = @"panels\AlarmCalendarNextMonthButton.png"
            });

            IconRegistry.Register(new IconDefinition
            {
                Id = AlarmCalendarPreviousMonthButtonId,
                DefaultPath = @"panels\AlarmCalendarPreviousMonthButton.png",
            });

            IconRegistry.Register(new IconDefinition
            {
                Id = TimerAddButton,
                DefaultPath = @"panels\TimerAddButton.png"
            });

            IconRegistry.Register(new IconDefinition
            {
                Id = TimerDiscardButton,
                DefaultPath = @"panels\TimerDiscardButton.png"
            });

            IconRegistry.Register(new IconDefinition
            {
                Id = TimerStopButton,
                DefaultPath = @"panels\TimerStopButton.png"
            });

            IconRegistry.Register(new IconDefinition
            {
                Id = TimerSaveButton,
                DefaultPath = @"panels\TimerSaveButton.png"
            });
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
            FontService.Regular.DrawText(12, calendar.ShownDateText, new Vector2(20, 45), textColor);

            int dayIndex = 0;
            foreach (var dayName in CalendarShowcase.WeekDayNames)
            {
                Rectangle weekDayBounds = TimesListLayoutCalculator.CalendarWeekDayBounds(dayIndex);
                FontService.Regular.DrawCenteredText(12, dayName, new Rectangle(new Point(20, 70) + weekDayBounds.Location, weekDayBounds.Size), textColor);
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

        public void RenderIconedButtons(params ReadOnlySpan<IconedButton> buttons)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            foreach (var button in buttons)
            {
                DrawHelper.Batch.Draw(theme.GetIcon(button.IconId), button.Boundaries, Color.White);

                if (button.IsHovered)
                {
                    DrawHelper.DrawBackground(button.Boundaries, button.IsHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                               : theme.GetColor(ColorRegistry.HoverOverlayId));
                }
            }
        }

        public void RenderTextedButtons(params ReadOnlySpan<TextedButton> buttons)
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
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);
            TimeSpan elapsed = stopwatch.Elapsed;

            const int LeftMostStartX = 290;
            const int NumbersMargin = 10;
            const int NumberWidth = 20;
            const int NumberTotalWidth = NumberWidth + NumbersMargin;
            const int NumberY = 220;

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
            ResolvedTheme theme = ThemeService.CurrentTheme;

            DrawHelper.Device.ScissorRectangle = timesList.Boundaries;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            for (int i = 0; i < timesList.ButtonCount; i++)
            {
                Rectangle relativeButtonBounds = timesList.GetButtonBounds(i);
                Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + timesList.Boundaries.Location, relativeButtonBounds.Size);

                DrawHelper.Batch.Draw(theme.GetIcon(IconRegistry.EditButtonId), TimesListLayoutCalculator.EditButtonBounds(buttonScreenBounds), Color.White);

                if (i == timesList.HoveredButtonIndex)
                {
                    bool hitEditButton = TimesListLayoutCalculator.HitEditButton(buttonScreenBounds.Size, timesList.RelativeMouseToHovered);
                    Rectangle overlayBounds = hitEditButton ? TimesListLayoutCalculator.EditButtonBounds(buttonScreenBounds) : buttonScreenBounds;

                    DrawHelper.DrawBackground(overlayBounds, timesList.IsHoveredButtonHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                                           : theme.GetColor(ColorRegistry.HoverOverlayId));
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

            FontService.Regular.DrawText(TimesListTextSize, buttonName, textPosition, ThemeService.CurrentTheme.GetColor(ColorRegistry.TextColorId));

            DrawHelper.EndBatch();
        }
    }
}
