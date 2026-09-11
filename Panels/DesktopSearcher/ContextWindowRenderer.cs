using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Theming;
using System.Collections.Generic;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class ContextWindowRenderer
    {
        private const int ResultListFileNameTextSize = 16;
        private const int ResultListFilePathTextSize = 12;

        private static readonly int ResultListFileNameTextLineSpacing = FontService.Regular.GetLineSpacing(ResultListFileNameTextSize);
        private static readonly int ResultListTextLineSpacing = FontService.Regular.GetLineSpacing(ResultListFileNameTextSize)
                                                              + FontService.Regular.GetLineSpacing(ResultListFilePathTextSize);

        private readonly Point _windowSize;
        private readonly RenderTarget2D _windowTexture;

        public ContextWindowRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);
        }

        public void RenderMainElements(TextPlaque plaque)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);

            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, _windowSize), theme.GetColor(ColorRegistry.BorderId),
                                              theme.GetColor(ColorRegistry.SurfaceId), 1);

            FontService.Regular.DrawText(plaque.TextSize, plaque.Text, plaque.Position, textColor);

            DrawHelper.EndBatch();
        }

        public void RenderSearchBox(LinearTextBox searchBox)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);

            DrawHelper.Device.ScissorRectangle = searchBox.Boundaries;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            DrawHelper.DrawBorderedBackground(searchBox.SelectableArea, theme.GetColor(ColorRegistry.BorderAccentId),
                                              theme.GetColor(ColorRegistry.BackgroundId), 1);

            FontService.Regular.DrawVerticallyCenteredText(searchBox.TextSize, searchBox.Text,
                new Vector2(searchBox.Boundaries.X + searchBox.TextOffset, searchBox.Boundaries.Y), searchBox.Boundaries.Height,
                textColor);

            if (searchBox.ReceivingInput)
            {
                const int CaretWidth = 1;

                int caretHeight = FontService.Regular.GetLineSpacing(searchBox.TextSize);
                DrawHelper.DrawBackground(new Rectangle(searchBox.Boundaries.X + searchBox.CaretX + searchBox.TextOffset,
                                                        searchBox.Boundaries.Y + (searchBox.Boundaries.Height - caretHeight) / 2,
                                                        CaretWidth,
                                                        caretHeight),
                                          textColor);
            }

            DrawHelper.EndBatch();
        }

        public void RenderResultsListBackgrounds(ScrollableButtonList resultsList, IReadOnlyList<SearcherResult> searcherResults)
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;

            DrawHelper.Device.ScissorRectangle = resultsList.Boundaries;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            for (int i = 0; i < resultsList.ButtonCount; i++)
            {
                Rectangle relativeButtonBounds = resultsList.GetButtonBounds(i);
                Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + resultsList.Boundaries.Location, relativeButtonBounds.Size);

                if (FileIconCache.TryGetIcon(searcherResults[i].FileName, out Texture2D icon))
                {
                    DrawHelper.Batch.Draw(icon, ResultsListLayoutCalculator.ButtonIconBounds(buttonScreenBounds), Color.White);
                }

                if (i == resultsList.HoveredButtonIndex)
                {
                    DrawHelper.DrawBackground(buttonScreenBounds, resultsList.IsHoveredButtonHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                                                  : theme.GetColor(ColorRegistry.HoverOverlayId));
                }
            }

            DrawHelper.EndBatch();
        }

        public void RenderResultsListText(ScrollableButtonList resultsList, SearcherResult result, int buttonIndex)
        {
            Rectangle relativeButtonBounds = resultsList.GetButtonBounds(buttonIndex);
            Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + resultsList.Boundaries.Location, relativeButtonBounds.Size);
            Rectangle buttonNameScreenBounds = ResultsListLayoutCalculator.ButtonNameBounds(buttonScreenBounds);

            int verticalCenterOffset = (resultsList.ButtonHeight - ResultListTextLineSpacing) / 2;
            Vector2 fileNameTextPosition = buttonNameScreenBounds.Location.ToVector2() + new Vector2(0, verticalCenterOffset);
            Vector2 filePathTextPosition = buttonNameScreenBounds.Location.ToVector2() + new Vector2(0, verticalCenterOffset + ResultListFileNameTextLineSpacing);

            // apply sliding offset
            if (buttonIndex == resultsList.HoveredButtonIndex)
            {
                float fileNameTextWidth = FontService.Regular.MeasureWidth(ResultListFileNameTextSize, result.FileName);
                fileNameTextPosition.X += ResultsListLayoutCalculator.GetTextSlideOffset(fileNameTextWidth, buttonScreenBounds.Width);

                float filePathTextWidth = FontService.Regular.MeasureWidth(ResultListFilePathTextSize, result.FileRelativePath);
                filePathTextPosition.X += ResultsListLayoutCalculator.GetTextSlideOffset(filePathTextWidth, buttonScreenBounds.Width);
            }

            Color textColor = ThemeService.CurrentTheme.GetColor(ColorRegistry.TextColorId);

            // we need intersect here since button may have been cut through if because of scrolling, so text should be cut too
            DrawHelper.Device.ScissorRectangle = Rectangle.Intersect(resultsList.Boundaries, buttonNameScreenBounds);
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            FontService.Regular.DrawText(ResultListFileNameTextSize, result.FileName, fileNameTextPosition, textColor);
            FontService.Regular.DrawText(ResultListFilePathTextSize, result.FileRelativePath, filePathTextPosition, textColor);

            DrawHelper.EndBatch();
        }
    }
}
