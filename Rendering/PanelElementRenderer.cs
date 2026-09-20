using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using System;
using System.Collections.Generic;

namespace SideKicker.Rendering
{
    internal static class PanelElementRenderer
    {
        private const int CaretWidth = 1;
        private const int SlidingTextPauseStart = 1; // seconds
        private const int SlidingTextPauseEnd = 1; // seconds
        private const int SlidingTextSpeed = 40; // pixels/second

        public static void RenderBackground(Point size)
        {
            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, size), RenderData.Border, RenderData.Surface, 1);
        }

        public static void RenderBackground(Rectangle bounds)
        {
            DrawHelper.DrawBorderedBackground(bounds, RenderData.Border, RenderData.Surface, 1);
        }

        public static void RenderTextPlaque(params ReadOnlySpan<TextPlaque> plaques)
        {
            foreach (TextPlaque plaque in plaques)
            {
                FontService.DrawText(plaque.TextSize, plaque.Text, plaque.Position, RenderData.Text);
            }
        }

        public static void RenderIconedButton(params ReadOnlySpan<IconedButton> buttons)
        {
            foreach (IconedButton button in buttons)
            {
                DrawHelper.Batch.Draw(RenderData.GetIcon(button.IconType), button.Boundaries, Color.White);

                if (button.IsHovered)
                {
                    DrawHelper.DrawBackground(button.Boundaries, button.IsHeld ? RenderData.HeldOverlay : RenderData.HoverOverlay);
                }
            }
        }

        public static void RenderTextedButton(params ReadOnlySpan<TextedButton> buttons)
        {
            foreach (TextedButton button in buttons)
            {
                DrawHelper.DrawBorderedBackground(button.Boundaries, RenderData.BorderAccent, RenderData.Background, 1);

                FontService.DrawCenteredText(button.TextSize, button.Text, button.Boundaries, RenderData.Text);

                if (button.IsHovered)
                {
                    DrawHelper.DrawBackground(button.Boundaries, button.IsHeld ? RenderData.HeldOverlay : RenderData.HoverOverlay);
                }
            }
        }

        public static void RenderToggleButton(params ReadOnlySpan<ToggleButton> buttons)
        {
            foreach (ToggleButton button in buttons)
            {
                DrawHelper.DrawBorderedBackground(button.Boundaries, RenderData.BorderAccent, RenderData.Background, 1);

                if (button.IsChecked)
                {
                    DrawHelper.Batch.Draw(RenderData.GetIcon(IconType.CheckedToggleButton), button.Boundaries, Color.White);
                }
            }
        }

        public static void RenderToggleButton(IEnumerable<ToggleButton> buttons)
        {
            foreach (ToggleButton button in buttons)
            {
                DrawHelper.DrawBorderedBackground(button.Boundaries, RenderData.BorderAccent, RenderData.Background, 1);

                if (button.IsChecked)
                {
                    DrawHelper.Batch.Draw(RenderData.GetIcon(IconType.CheckedToggleButton), button.Boundaries, Color.White);
                }
            }
        }

        public static void RenderLinearTextBox(LinearTextBox textbox)
        {
            DrawHelper.Device.ScissorRectangle = textbox.Boundaries;

            DrawHelper.DrawBorderedBackground(textbox.SelectableArea, RenderData.BorderAccent, RenderData.Background, 1);

            FontService.DrawVerticallyCenteredText(textbox.TextSize, textbox.Text,
                                                           new Vector2(textbox.Boundaries.X + textbox.TextOffset, textbox.Boundaries.Y),
                                                           textbox.Boundaries.Height,
                                                           RenderData.Text);

            Rectangle selectionBox = textbox.SelectionBox;
            DrawHelper.DrawBackground(new Rectangle(textbox.Boundaries.X + textbox.TextOffset + selectionBox.X,
                                                    textbox.Boundaries.Y + selectionBox.Y,
                                                    selectionBox.Width,
                                                    selectionBox.Height),
                                      RenderData.SelectionBox);

            if (textbox.ReceivingInput)
            {
                int caretHeight = FontService.GetLineSpacing(textbox.TextSize);

                DrawHelper.DrawBackground(new Rectangle(textbox.Boundaries.X + textbox.TextOffset + textbox.CaretX,
                                                        textbox.Boundaries.Y + (textbox.Boundaries.Height - caretHeight) / 2,
                                                        CaretWidth,
                                                        caretHeight),
                                          RenderData.Text);
            }
        }

        public static void RenderMultiLineTextBox(MultiLineTextBox textbox)
        {
            DrawHelper.Device.ScissorRectangle = textbox.Boundaries;

            DrawHelper.DrawBorderedBackground(textbox.SelectableArea, RenderData.BorderAccent, RenderData.Background, 1);

            int lineSpacing = FontService.GetLineSpacing(textbox.TextSize);

            Vector2 currentPosition = textbox.Boundaries.Location.ToVector2() - new Vector2(0, textbox.ScrollOffset);
            foreach (string line in textbox.Lines)
            {
                FontService.DrawText(textbox.TextSize, line, currentPosition, RenderData.Text);
                currentPosition.Y += lineSpacing;
            }

            foreach (Rectangle selectionBox in textbox.SelectionBoxes)
            {
                DrawHelper.DrawBackground(new Rectangle(textbox.Boundaries.X + selectionBox.X,
                                                        textbox.Boundaries.Y + selectionBox.Y - textbox.ScrollOffset,
                                                        selectionBox.Width,
                                                        selectionBox.Height),
                                          RenderData.SelectionBox);
            }

            if (textbox.ReceivingInput)
            {
                DrawHelper.DrawBackground(new Rectangle(textbox.Boundaries.Location + textbox.CaretPosition,
                                                        new Point(CaretWidth, lineSpacing)),
                                          RenderData.Text);
            }
        }

        public static void RenderScrollableButtonListBackground<TSource, TRegion>(ScrollableButtonList list, IReadOnlyList<TSource> source, 
                                                                                  ButtonListRendering<TSource, TRegion> rendering, 
                                                                                  IButtonListLayoutCalculator<TRegion> layoutCalculator)
            where TRegion : struct, Enum
        {
            DrawHelper.Device.ScissorRectangle = list.Boundaries;

            for (int i = 0; i < source.Count; i++)
            {
                Rectangle localButtonBounds = list.GetButtonBounds(i);
                localButtonBounds.Location += list.Boundaries.Location;

                foreach (TRegion cachedFileRegion in rendering.CachedFileIconRegions)
                {
                    if (FileIconCache.TryGetIcon(rendering.GetCachedFileIconKey(cachedFileRegion, source[i]), out Texture2D icon) && icon != null)
                    {
                        DrawHelper.Batch.Draw(icon, layoutCalculator.ToCachedFileIconRegion(cachedFileRegion, localButtonBounds), Color.White);
                    }
                }

                foreach (TRegion commonIconRegion in rendering.CommonIconRegions)
                {
                    if (rendering.TryGetRegionCommonIcon(commonIconRegion, i, out IconType commonIcon))
                    {
                        DrawHelper.Batch.Draw(RenderData.GetIcon(commonIcon), layoutCalculator.ToCommonIconRegion(commonIconRegion, localButtonBounds), Color.White);
                    }
                }
            }
        }

        public static void RenderScrollableButtonListSlidingText<TSource, TRegion>(ScrollableButtonList list, 
                                                                                   TRegion slidingTextRegion, int buttonIndex, TSource source,
                                                                                   ButtonListRendering<TSource, TRegion> rendering,
                                                                                   IButtonListLayoutCalculator<TRegion> layoutCalculator)
        {
            Rectangle localButtonBounds = list.GetButtonBounds(buttonIndex);
            localButtonBounds.Location += list.Boundaries.Location;

            Rectangle slidingTextBounds = layoutCalculator.ToSlidingTextRegion(slidingTextRegion, localButtonBounds);
            DrawHelper.Device.ScissorRectangle = Rectangle.Intersect(list.Boundaries, slidingTextBounds);

            Vector2 textPosition = slidingTextBounds.Location.ToVector2();
            string textResult = rendering.GetSlidingTextResult(slidingTextRegion, source);
            int textSize = layoutCalculator.GetSlidingTextRegionTextSize(slidingTextRegion);

            if (list.HoveredButtonIndex == buttonIndex)
            {
                textPosition.X += GetTextSlideOffset(FontService.MeasureWidth(textSize, textResult), slidingTextBounds.Width);
            }

            FontService.DrawText(textSize, textResult, textPosition, RenderData.Text);
        }

        public static void RenderScrollableButtonListHoverOverlay<TSource, TRegion>(ScrollableButtonList list, ButtonListRendering<TSource, TRegion> rendering,
                                                                                    IButtonListLayoutCalculator<TRegion> layoutCalculator)
        {
            Rectangle hoveredRegionBounds = list.GetButtonBounds(list.HoveredButtonIndex);
            Rectangle overlayBounds = hoveredRegionBounds;

            if (layoutCalculator.TryGetHoveredCommonIconRegion(list.RelativeMouseToHovered, hoveredRegionBounds.Size, out TRegion hoveredRegion) &&
                rendering.IsCommonIconRegionEligible(hoveredRegion, list.HoveredButtonIndex))
            {
                overlayBounds = layoutCalculator.ToCommonIconRegion(hoveredRegion, hoveredRegionBounds);
            }

            overlayBounds.Location += list.Boundaries.Location;

            DrawHelper.Device.ScissorRectangle = Rectangle.Intersect(list.Boundaries, overlayBounds);
            DrawHelper.DrawBackground(overlayBounds, list.IsHoveredButtonHeld ? RenderData.HeldOverlay : RenderData.HoverOverlay);
        }

        private static float GetTextSlideOffset(float totalTextWidth, int textLayoutWidth)
        {
            float scrollDistance = totalTextWidth - textLayoutWidth;
            if (scrollDistance <= 0)
            {
                return 0;
            }

            float scrollTime = scrollDistance / SlidingTextSpeed;
            float cycleTime = SlidingTextPauseStart + scrollTime + SlidingTextPauseEnd;

            float currentCyleTimePosition = (float)(Time.ElapsedGameTime.TotalSeconds % cycleTime);

            if (currentCyleTimePosition < SlidingTextPauseStart) // start pause state
            {
                return 0;
            }

            if (currentCyleTimePosition < SlidingTextPauseStart + scrollTime) // sliding
            {
                return -(currentCyleTimePosition - SlidingTextPauseStart) * SlidingTextSpeed;
            }

            return -scrollDistance; // end pause state
        }
    }
}
