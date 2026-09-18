using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Rendering;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class ContextWindowRenderer
    {
        private readonly Point _windowSize;
        private readonly RenderTarget2D _windowTexture;
        private readonly ButtonListRendering<SearcherResult, ResultsListButtonRegion> _resultsListRendering;

        // captured by results list renderer's common icon predicate, for drawing closing button
        private int _resultsListHoveredButtonIndex;
        private bool _resultsListShowClosingOption;

        public ContextWindowRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);

            _resultsListRendering = new ButtonListRendering<SearcherResult, ResultsListButtonRegion>(
                cachedFileIconGatherer: (region, searchResult) => searchResult.FileName,
                slidingTextGatherer: (region, searchResult) => region switch
                {
                    ResultsListButtonRegion.FileName => searchResult.FileName,
                    ResultsListButtonRegion.FileRelativePath => searchResult.FileRelativePath,

                    _ => throw new ArgumentException("Region not supported " + region)
                },
                commonIconPredicate: (region, buttonIndex) => _resultsListShowClosingOption && buttonIndex == _resultsListHoveredButtonIndex);

            _resultsListRendering.AddRegionCachedFileIcon(ResultsListButtonRegion.FileIcon);
            _resultsListRendering.AddRegionSlidingText(ResultsListButtonRegion.FileName, ResultsListLayoutCalculator.FileNameTextSize);
            _resultsListRendering.AddRegionSlidingText(ResultsListButtonRegion.FileRelativePath, ResultsListLayoutCalculator.FilePathTextSize);
            _resultsListRendering.AddRegionCommonIcon(ResultsListButtonRegion.CloseButton, IconType.CloseButton);
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

        public void RenderScrollableButtonList(ScrollableButtonList resultsList, IReadOnlyList<SearcherResult> searcherResults, bool showClosingOption)
        {
            _resultsListHoveredButtonIndex = resultsList.HoveredButtonIndex;
            _resultsListShowClosingOption = showClosingOption;

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderScrollableButtonListBackground(resultsList, searcherResults, _resultsListRendering, ResultsListLayoutCalculator.Instance);
            DrawHelper.EndBatch();

            for (int i = 0; i < searcherResults.Count; i++)
            {
                foreach (ResultsListButtonRegion region in _resultsListRendering.SlidingTextRegions)
                {
                    DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
                    PanelElementRenderer.RenderScrollableButtonListSlidingText(resultsList, region, i, searcherResults[i], 
                                                                               _resultsListRendering, ResultsListLayoutCalculator.Instance);
                    DrawHelper.EndBatch();
                }
            }

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderScrollableButtonListHoverOverlay(resultsList, _resultsListRendering, ResultsListLayoutCalculator.Instance);
            DrawHelper.EndBatch();
        }
    }
}
