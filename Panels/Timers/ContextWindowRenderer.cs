using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
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
