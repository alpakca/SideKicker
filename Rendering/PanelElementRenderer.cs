using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using SideKicker.Theming;
using System;

namespace SideKicker.Rendering
{
    internal static class PanelElementRenderer
    {
        private const int CaretWidth = 1;

        public static void RenderBackground(Point size)
        {
            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, size), RenderData.Background, RenderData.Surface, 1);
        }

        public static void RenderTextPlaque(params ReadOnlySpan<TextPlaque> plaques)
        {
            foreach (TextPlaque plaque in plaques)
            {
                FontService.Regular.DrawText(plaque.TextSize, plaque.Text, plaque.Position, RenderData.Text);
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

                FontService.Regular.DrawCenteredText(button.TextSize, button.Text, button.Boundaries, RenderData.Text);

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

        public static void RenderLinearTextBox(LinearTextBox textbox)
        {
            DrawHelper.Device.ScissorRectangle = textbox.Boundaries;

            DrawHelper.DrawBorderedBackground(textbox.SelectableArea, RenderData.BorderAccent, RenderData.Background, 1);

            FontService.Regular.DrawVerticallyCenteredText(textbox.TextSize, textbox.Text,
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
                int caretHeight = FontService.Regular.GetLineSpacing(textbox.TextSize);

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

            int lineSpacing = FontService.Regular.GetLineSpacing(textbox.TextSize);

            Vector2 currentPosition = textbox.Boundaries.Location.ToVector2() - new Vector2(0, textbox.ScrollOffset);
            foreach (string line in textbox.Lines)
            {
                FontService.Regular.DrawText(textbox.TextSize, line, currentPosition, RenderData.Text);
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

        public static void DrawScrollableButtonListHoverOverlay<TRegion>(ScrollableButtonList list, ButtonRegionLayout<TRegion> layout) 
            where TRegion : struct, Enum
        {
            Rectangle overlayLocal = layout.TryHitLocalBounds(list.ButtonSize, list.RelativeMouseToHovered, out _, out Rectangle regionBounds)
                                   ? regionBounds
                                   : new Rectangle(Point.Zero, list.ButtonSize);

            overlayLocal.Location += list.Boundaries.Location + list.GetButtonLocation(list.HoveredButtonIndex); // to screen bounds

            DrawHelper.DrawBackground(overlayLocal, list.IsHoveredButtonHeld ? RenderData.HeldOverlay : RenderData.HoverOverlay);
        }

        public static void DrawScrollableButtonListRegions<TRegion>(ScrollableButtonList list, int buttonIndex, ButtonRegionLayout<TRegion> layout, 
                                                                    ButtonRegionRendering<TRegion> rendering, params ReadOnlySpan<TRegion> regions)
            where TRegion : struct, Enum
        {
            foreach (TRegion region in regions)
            {
                Rectangle local = layout.GetLocalBounds(region, list.ButtonSize);
                local.Location += list.Boundaries.Location + list.GetButtonLocation(buttonIndex);

                rendering.GetRenderer(region).Invoke(local);
            }
        }
    }
}
