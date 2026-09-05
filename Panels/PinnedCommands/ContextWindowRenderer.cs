using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Theming;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using Icon = System.Drawing.Icon;

namespace SideKicker.Panels.PinnedCommands
{
    internal class ContextWindowRenderer
    {
        private const int PanelTextSize = ContextWindow.PanelTextSize;
        private const string ReorderButtonId = "pinnedcommands.reorderbutton";
        private const string EditButtonId = "pinnedcommands.editbutton";

        private readonly RenderTarget2D _windowTexture;
        private readonly Point _windowSize;
        private readonly Dictionary<string, Texture2D> _fileIcons;
        private readonly Dictionary<string, Texture2D> _fileTypeIcons;

        public ContextWindowRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);
            _fileIcons = new Dictionary<string, Texture2D>();
            _fileTypeIcons = new Dictionary<string, Texture2D>();
        }

        #region Icons
        [ModuleInitializer]
        public static void InitializeIcons()
        {
            IconRegistry.Register(new IconDefinition
            {
                Id = ReorderButtonId,
                DefaultPath = @"panels\ReorderButton.png"
            });
        }

        private static Texture2D GetIconTexture(MemoryStream stream, Icon icon)
        {
            stream.SetLength(0);

            icon.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);

            return Texture2D.FromStream(DrawHelper.Device, stream);
        }

        public void RefreshIcons(IReadOnlyList<SavedCommand> savedCommands)
        {
            using MemoryStream stream = new MemoryStream();

            foreach (var command in savedCommands)
            {
                string fileExtension = Path.GetExtension(command.FileName);

                // if an icon was already set for this file
                if (_fileIcons.ContainsKey(command.FileName))
                {
                    continue;
                }

                // if no icon was set for this file, but was set for same extension.
                // ".exe" can't exist here as they are only added to _fileIcons dictionary
                if (_fileTypeIcons.TryGetValue(fileExtension, out Texture2D value))
                {
                    _fileIcons.Add(command.FileName, value);
                    continue;
                }

                string absolutePath = Path.IsPathRooted(command.FileName) ? command.FileName : Path.Combine(command.PathToRunCommand, command.FileName);

                // special case for directories: use explorer icon
                if (Directory.Exists(absolutePath))
                {
                    Icon explorerIcon = Icon.ExtractAssociatedIcon(@"C:\Windows\explorer.exe");
                    if (explorerIcon == null) continue;

                    Texture2D explorerIconTexture = GetIconTexture(stream, explorerIcon);

                    _fileIcons.Add(command.FileName, explorerIconTexture);
                }
                else
                {
                    Icon icon = Icon.ExtractAssociatedIcon(absolutePath);
                    if (icon == null) continue;

                    Texture2D iconTexture = GetIconTexture(stream, icon);

                    // special case for executables: they have different icons with same .exe extension,
                    // so don't save them for type icons
                    if (fileExtension != ".exe")
                    {
                        _fileTypeIcons[fileExtension] = iconTexture;
                    }

                    _fileIcons.Add(command.FileName, iconTexture);
                }
            }
        }
        #endregion

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

                if (_fileIcons.TryGetValue(savedCommands[i].FileName, out Texture2D icon))
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
