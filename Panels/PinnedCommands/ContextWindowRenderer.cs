using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Rendering;
using System.Collections.Generic;

namespace SideKicker.Panels.PinnedCommands
{
    internal class ContextWindowRenderer
    {
        private const int PanelTextSize = ContextWindow.PanelTextSize;

        private readonly RenderTarget2D _windowTexture;
        private readonly Point _windowSize;

        public ContextWindowRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);
        }

        public void Render(TextPlaque[] editorTextPlaques, TextedButton[] editorButtons, IEnumerable<LinearTextBox> textboxes)
        {
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);
            PanelElementRenderer.RenderBackground(_windowSize);
            PanelElementRenderer.RenderTextPlaque(editorTextPlaques);
            PanelElementRenderer.RenderTextedButton(editorButtons);
            DrawHelper.EndBatch();

            foreach (var textbox in textboxes)
            {
                DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
                PanelElementRenderer.RenderLinearTextBox(textbox);
                DrawHelper.EndBatch();
            }
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

            FontService.Regular.DrawText(PanelTextSize, buttonName, textPosition, RenderData.Text);

            DrawHelper.EndBatch();
        }

        public void RenderCommandListBackgrounds(ScrollableButtonList commandsList, IReadOnlyList<SavedCommand> savedCommands)
        {
            DrawHelper.Device.ScissorRectangle = commandsList.Boundaries;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            for (int i = 0; i < savedCommands.Count; i++)
            {
                Rectangle relativeButtonBounds = commandsList.GetButtonBounds(i);
                Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + commandsList.Boundaries.Location, relativeButtonBounds.Size);

                DrawHelper.Batch.Draw(RenderData.GetIcon(IconType.ReorderButton), CommandListLayoutCalculator.ReorderButtonBounds(buttonScreenBounds), Color.White);

                if (FileIconCache.TryGetIcon(savedCommands[i].FileName, out Texture2D icon) && icon != null)
                {
                    DrawHelper.Batch.Draw(icon, CommandListLayoutCalculator.ButtonFileIconBounds(buttonScreenBounds), Color.White);
                }

                DrawHelper.Batch.Draw(RenderData.GetIcon(IconType.EditButton), CommandListLayoutCalculator.EditButtonBounds(buttonScreenBounds), Color.White);

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

                    DrawHelper.DrawBackground(overlayBounds, commandsList.IsHoveredButtonHeld ? RenderData.HeldOverlay : RenderData.HoverOverlay);
                }
            }

            DrawHelper.EndBatch();
        }
    }
}
