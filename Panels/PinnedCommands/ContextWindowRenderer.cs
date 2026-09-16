using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Theming;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SideKicker.Panels.PinnedCommands
{
    internal class ContextWindowRenderer
    {
        private const int PanelTextSize = ContextWindow.PanelTextSize;
        private const string ReorderButtonId = "pinnedcommands.reorderbutton";

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
                Id = ReorderButtonId,
                DefaultPath = @"panels\ReorderButton.png"
            });
        }

        public void RenderMainElements(TextPlaque[] editorTextPlaques, TextedButton[] editorButtons)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);

            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, _windowSize), theme.GetColor(ColorRegistry.BorderId),
                                              theme.GetColor(ColorRegistry.SurfaceId), 1);

            foreach (var textPlaque in editorTextPlaques)
            {
                FontService.Regular.DrawText(PanelTextSize, textPlaque.Text, textPlaque.Position, textColor);
            }

            foreach (var button in editorButtons)
            {
                DrawHelper.DrawBackground(button.Boundaries, theme.GetColor(ColorRegistry.BackgroundId));
                FontService.Regular.DrawCenteredText(PanelTextSize, button.Text, button.Boundaries, textColor);

                if (button.IsHovered)
                {
                    DrawHelper.DrawBackground(button.Boundaries, button.IsHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                               : theme.GetColor(ColorRegistry.HoverOverlayId));
                }
            }

            // reagent between editor and command list
            DrawHelper.DrawBackground(new Rectangle(250, 20, 1, _windowSize.Y - 2 * 10), theme.GetColor(ColorRegistry.BorderId));

            DrawHelper.EndBatch();
        }

        public void RenderTextBox(LinearTextBox textBox)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);

            DrawHelper.Device.ScissorRectangle = textBox.Boundaries;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            DrawHelper.DrawBorderedBackground(textBox.SelectableArea, theme.GetColor(ColorRegistry.BorderAccentId),
                                              theme.GetColor(ColorRegistry.BackgroundId), 1);

            FontService.Regular.DrawVerticallyCenteredText(PanelTextSize, textBox.Text,
                new Vector2(textBox.Boundaries.X + textBox.TextOffset, textBox.Boundaries.Y), textBox.Boundaries.Height,
                textColor);

            Rectangle selectionBox = textBox.SelectionBox;
            DrawHelper.DrawBackground(new Rectangle(textBox.Boundaries.X + selectionBox.X + textBox.TextOffset,
                                                    textBox.Boundaries.Y + selectionBox.Y,
                                                    selectionBox.Width, selectionBox.Height),
                                      theme.GetColor(ColorRegistry.SelectionBoxOverlayId));

            if (textBox.ReceivingInput)
            {
                const int CaretWidth = 2;

                int caretHeight = FontService.Regular.GetLineSpacing(PanelTextSize);
                DrawHelper.DrawBackground(new Rectangle(textBox.Boundaries.X + textBox.CaretX + textBox.TextOffset, 
                                                        textBox.Boundaries.Y + (textBox.Boundaries.Height - caretHeight) / 2,
                                                        CaretWidth, 
                                                        caretHeight),
                                          textColor);
            }

            DrawHelper.EndBatch();
        }

        public void RenderCommandListButtonText(ScrollableButtonList commandsList, int buttonIndex, string buttonName)
        {
            Rectangle relativeButtonBounds = commandsList.GetButtonBounds(buttonIndex);
            Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + commandsList.Boundaries.Location, relativeButtonBounds.Size);
            Rectangle buttonNameScreenBounds = CommandListLayoutCalculator.ButtonNameBounds(buttonScreenBounds);

            int verticalCenterOffset = (commandsList.ButtonHeight - FontService.Regular.GetLineSpacing(PanelTextSize)) / 2;
            Vector2 textPosition = buttonNameScreenBounds.Location.ToVector2() + new Vector2(0, verticalCenterOffset);

            // apply sliding offset
            if (buttonIndex == commandsList.HoveredButtonIndex)
            {
                float textWidth = FontService.Regular.MeasureWidth(PanelTextSize, buttonName);
                textPosition.X += CommandListLayoutCalculator.GetCommandListTextSlideOffset(textWidth, buttonScreenBounds.Width);
            }

            // we need intersect here since button may have been cut through if because of scrolling, so text should be cut too
            DrawHelper.Device.ScissorRectangle = Rectangle.Intersect(commandsList.Boundaries, buttonNameScreenBounds);
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            FontService.Regular.DrawText(PanelTextSize, buttonName, textPosition, ThemeService.CurrentTheme.GetColor(ColorRegistry.TextColorId));

            DrawHelper.EndBatch();
        }

        public void RenderCommandListBackgrounds(ScrollableButtonList commandsList, IReadOnlyList<SavedCommand> savedCommands)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;

            DrawHelper.Device.ScissorRectangle = commandsList.Boundaries;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            for (int i = 0; i < savedCommands.Count; i++)
            {
                Rectangle relativeButtonBounds = commandsList.GetButtonBounds(i);
                Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + commandsList.Boundaries.Location, relativeButtonBounds.Size);

                DrawHelper.Batch.Draw(theme.GetIcon(ReorderButtonId), CommandListLayoutCalculator.ReorderButtonBounds(buttonScreenBounds), Color.White);

                if (FileIconCache.TryGetIcon(savedCommands[i].FileName, out Texture2D icon) && icon != null)
                {
                    DrawHelper.Batch.Draw(icon, CommandListLayoutCalculator.ButtonFileIconBounds(buttonScreenBounds), Color.White);
                }

                DrawHelper.Batch.Draw(theme.GetIcon(IconRegistry.EditButtonId), CommandListLayoutCalculator.EditButtonBounds(buttonScreenBounds), Color.White);

                if (i == commandsList.HoveredButtonIndex)
                {
                    CommandButtonLayout layout = CommandListLayoutCalculator.GetLayout(commandsList.GetButtonBounds(i).Size,
                                                                                       commandsList.RelativeMouseToHovered);

                    Rectangle overlayBounds = buttonScreenBounds;
                    if (layout == CommandButtonLayout.ReorderButton)
                    {
                        overlayBounds = CommandListLayoutCalculator.ReorderButtonBounds(buttonScreenBounds);
                    }
                    else if (layout == CommandButtonLayout.EditButton)
                    {
                        overlayBounds = CommandListLayoutCalculator.EditButtonBounds(buttonScreenBounds);
                    }

                    DrawHelper.DrawBackground(overlayBounds, commandsList.IsHoveredButtonHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                                              : theme.GetColor(ColorRegistry.HoverOverlayId));
                }
            }

            DrawHelper.EndBatch();
        }
    }
}
