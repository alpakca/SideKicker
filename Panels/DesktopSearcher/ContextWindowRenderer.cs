using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Rendering;
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

        public void Render(TextPlaque plaque, LinearTextBox searchBox)
        {
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaBlendDrawOptions);
            PanelElementRenderer.RenderBackground(_windowSize);
            PanelElementRenderer.RenderTextPlaque(plaque);
            DrawHelper.EndBatch();

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderLinearTextBox(searchBox);
            DrawHelper.EndBatch();
        }

        public void RenderResultsListBackgrounds(ScrollableButtonList resultsList, IReadOnlyList<SearcherResult> searcherResults, bool showClosingOption)
        {
            DrawHelper.Device.ScissorRectangle = resultsList.Boundaries;
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            for (int i = 0; i < resultsList.ButtonCount; i++)
            {
                Rectangle relativeButtonBounds = resultsList.GetButtonBounds(i);
                Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + resultsList.Boundaries.Location, relativeButtonBounds.Size);

                if (FileIconCache.TryGetIcon(searcherResults[i].FileName, out Texture2D icon) && icon != null)
                {
                    DrawHelper.Batch.Draw(icon, ResultsListLayoutCalculator.ButtonIconBounds(buttonScreenBounds), Color.White);
                }

                if (i == resultsList.HoveredButtonIndex)
                {
                    Rectangle overlayBounds = buttonScreenBounds;

                    if (showClosingOption)
                    {
                        Rectangle closeButtonBounds = ResultsListLayoutCalculator.ButtonCloseBounds(buttonScreenBounds);
                        DrawHelper.Batch.Draw(RenderData.GetIcon(IconType.CloseButton), closeButtonBounds, Color.White);

                        if (ResultsListLayoutCalculator.HitCloseBounds(buttonScreenBounds.Size, resultsList.RelativeMouseToHovered))
                        {
                            overlayBounds = closeButtonBounds;
                        }
                    }

                    DrawHelper.DrawBackground(overlayBounds, resultsList.IsHoveredButtonHeld ? RenderData.HeldOverlay : RenderData.HoverOverlay);
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

            Color textColor = RenderData.Text;

            // we need intersect here since button may have been cut through if because of scrolling, so text should be cut too
            DrawHelper.Device.ScissorRectangle = Rectangle.Intersect(resultsList.Boundaries, buttonNameScreenBounds);
            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);

            FontService.Regular.DrawText(ResultListFileNameTextSize, result.FileName, fileNameTextPosition, textColor);
            FontService.Regular.DrawText(ResultListFilePathTextSize, result.FileRelativePath, filePathTextPosition, textColor);

            DrawHelper.EndBatch();
        }
    }
}
