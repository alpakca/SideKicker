using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Rendering;
using System.Collections.Generic;

namespace SideKicker.Panels.PinnedCommands
{
    internal class ContextWindowRenderer
    {
        private readonly Point _windowSize;
        private readonly RenderTarget2D _windowTexture;
        private readonly ButtonListRendering<SavedCommand, CommandsListButtonRegion> _commandsListRendering;

        public ContextWindowRenderer(RenderTarget2D windowTexture)
        {
            _windowTexture = windowTexture;
            _windowSize = new Point(windowTexture.Width, windowTexture.Height);

            _commandsListRendering = new ButtonListRendering<SavedCommand, CommandsListButtonRegion>(
                cachedFileIconGatherer: (_, source) => source.FileName,
                slidingTextGatherer: (_, source) => source.Name,
                commonIconPredicate: (_, _) => true);

            _commandsListRendering.AddRegionCachedFileIcon(CommandsListButtonRegion.FileIcon);
            _commandsListRendering.AddRegionSlidingText(CommandsListButtonRegion.SavedName, CommandListLayoutCalculator.PanelTextSize);
            _commandsListRendering.AddRegionCommonIcon(CommandsListButtonRegion.ReorderButton, IconType.ReorderButton);
            _commandsListRendering.AddRegionCommonIcon(CommandsListButtonRegion.EditButton, IconType.EditButton);
        }

        public void Render(TextPlaque[] editorTextPlaques, TextedButton[] editorButtons, IEnumerable<LinearTextBox> textboxes, 
                           ScrollableButtonList commandsList, IReadOnlyList<SavedCommand> savedCommands)
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

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderScrollableButtonListBackground(commandsList, savedCommands, _commandsListRendering, CommandListLayoutCalculator.Instance);
            DrawHelper.EndBatch();

            for (int i = 0; i < savedCommands.Count; i++)
            {
                foreach (CommandsListButtonRegion slidingTextRegion in _commandsListRendering.SlidingTextRegions)
                {
                    DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
                    PanelElementRenderer.RenderScrollableButtonListSlidingText(commandsList, slidingTextRegion, i, 
                                                                               savedCommands[i], _commandsListRendering, CommandListLayoutCalculator.Instance);
                    DrawHelper.EndBatch();
                }
            }

            DrawHelper.SetBeginBatch(_windowTexture, DrawHelper.AlphaScissoredDrawOptions);
            PanelElementRenderer.RenderScrollableButtonListHoverOverlay(commandsList, _commandsListRendering, CommandListLayoutCalculator.Instance);
            DrawHelper.EndBatch();
        }
    }
}
