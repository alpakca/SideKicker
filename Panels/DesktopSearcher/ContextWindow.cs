using Microsoft.Xna.Framework;
using PopUpUI.Input;
using SideKicker.PanelSystem;
using SideKicker.PanelSystem.Message;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class ContextWindow : MessageSupporterWindow
    {
        private const int SearchBoxTextSize = 16;
        private const int HeaderTextSize = 20;
        private const int EnumerateFilesThreshold = 400;

        private IReadOnlyList<SearcherResult> _searcherResults;
        private readonly ContextWindowRenderer _renderer;

        private readonly LinearTextBox _searchBox;
        private readonly ScrollableButtonList _resultsList;

        private readonly TextPlaque _emptySearchBoxPlaque;
        private readonly TextPlaque _nonEmptySearchBoxPlaque;

        private bool _showClosingOption;
        private float _remainingTextWaitingTime;

        public event Action<string> SearchForResult;
        public event Action<SearcherResult> ResultSelected;
        public event Action<SearcherResult> ResultCloseSelected;

        public ContextWindow()
        {
            _emptySearchBoxPlaque = new TextPlaque
            {
                Text = "Last",
                TextSize = HeaderTextSize,
                Position = new Vector2(20, 10)
            };

            _nonEmptySearchBoxPlaque = new TextPlaque
            {
                Text = "Results",
                TextSize = HeaderTextSize,
                Position = new Vector2(20, 10)
            };

            _searchBox = new LinearTextBox
            {
                TextSize = SearchBoxTextSize,
                SelectableMargin = 4,
                Boundaries = new Rectangle(20, 356, 360, 30)
            };
            _searchBox.TextValueChanged += SearchBox_TextValueChanged;

            _resultsList = new ScrollableButtonList
            {
                Boundaries = new Rectangle(20, 50, 368, 300),
                ButtonHeight = ResultsListLayoutCalculator.ButtonHeight,
                ButtonMargin = 4
            };
            _resultsList.ButtonPressed += ResultsList_ButtonPressed;

            this.Elements.Add(_searchBox);
            RegisterClipboardSupporters(_searchBox);

            this.Boundaries = new Rectangle(Point.Zero, new Point(400, 400));
            _renderer = new ContextWindowRenderer(WindowTexture);
        }

        public void SetSearcherResults(IReadOnlyList<SearcherResult> searcherResults, bool showClosingOption)
        {
            _showClosingOption = showClosingOption;
            _searcherResults = searcherResults;
            _resultsList.ButtonCount = searcherResults.Count;
            _resultsList.UpdateScrolledLength();

            FileIconCache.AddIcons(_searcherResults);
        }

        private void ResultsList_ButtonPressed(int buttonIndex)
        {
            if (_showClosingOption && ResultsListLayoutCalculator.Instance.HitCloseButton(_resultsList.GetButtonBounds(buttonIndex), _resultsList.RelativeMouseToHovered))
            {
                ResultCloseSelected?.Invoke(_searcherResults[buttonIndex]);
            }
            else
            {
                ResultSelected?.Invoke(_searcherResults[buttonIndex]);
            }
        }

        private void SearchBox_TextValueChanged()
        {
            if (_remainingTextWaitingTime > 0)
            {
                return;
            }

            _remainingTextWaitingTime = EnumerateFilesThreshold;
        }

        protected override void Update()
        {
            if (_remainingTextWaitingTime > 0)
            {
                _remainingTextWaitingTime -= TimeManager.DeltaTime;
                if (_remainingTextWaitingTime <= 0)
                {
                    SearchForResult?.Invoke(_searchBox.Text);
                }
            }

            base.Update();
        }

        protected override void RenderUpdateWithoutMessages()
        {
            _renderer.Render(_searchBox.Text.Length == 0 ? _emptySearchBoxPlaque : _nonEmptySearchBoxPlaque, _searchBox);
            _renderer.RenderScrollableButtonList(_resultsList, _searcherResults, _showClosingOption);
        }

        protected override void OnMouseButtonStateChangeWithoutMessages(PopWindowMouseEventArgs e)
        {
            _resultsList.MouseStateChangeUpdate(e);
            base.OnMouseButtonStateChangeWithoutMessages(e);
        }

        protected override void OnMousePositionChangeWithoutMessages(PopWindowMouseEventArgs e)
        {
            _resultsList.MousePositionChanged(e);
            base.OnMousePositionChangeWithoutMessages(e);
        }

        protected override void OnMouseVerticalScrollChange(PopWindowMouseEventArgs e, int change)
        {
            if (_resultsList.Boundaries.Contains(e.MousePosition))
            {
                _resultsList.Scroll(e, -change / 6);
            }

            base.OnMouseVerticalScrollChange(e, change);
        }
    }
}
