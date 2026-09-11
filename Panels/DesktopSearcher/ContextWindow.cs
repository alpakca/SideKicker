using PopUpUI.Toolkit;
using SideKicker.PanelSystem.Message;
using System;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class ContextWindow : MessageSupporterWindow
    {
        private const int SearchBoxTextSize = 16;
        private const int HeaderTextSize = 20;
        private const int EnumerateFilesThreshold = 400;

        private readonly ISearcherResultCollection _searcherResultCollection;

        private readonly LinearTextBox _searchBox;
        private readonly ScrollableButtonList _resultsList;

        private readonly TextPlaque _emptySearchBoxPlaque;
        private readonly TextPlaque _nonEmptySearchBoxPlaque;

        private bool _isSearchBoxEmpty;
        private float _remainingTextWaitingTime;

        public bool IsSearchBoxEmpty => _isSearchBoxEmpty;

        public event Action<string> SearchForResult;
        public event Action<SearcherResult> ResultSelected;

        public ContextWindow(ISearcherResultCollection searchResults)
        {
            _searcherResultCollection = searchResults;

            _emptySearchBoxPlaque = new TextPlaque
            {
                Text = "Last",
                TextSize = HeaderTextSize
            };

            _nonEmptySearchBoxPlaque = new TextPlaque
            {
                Text = "Results",
                TextSize = HeaderTextSize
            };

            _searchBox = new LinearTextBox
            {
                TextSize = SearchBoxTextSize
            };
            _searchBox.TextValueChanged += SearchBox_TextValueChanged;

            _resultsList = new ScrollableButtonList
            {

            };
            _resultsList.ButtonPressed += ResultsList_ButtonPressed;
        }

        private void ResultsList_ButtonPressed(int buttonIndex)
        {
            ResultSelected?.Invoke(_searcherResultCollection.Results[buttonIndex]);
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
                _remainingTextWaitingTime -= Time.DeltaTime;
                if (_remainingTextWaitingTime <= 0)
                {
                    _isSearchBoxEmpty = _searchBox.Text == string.Empty;

                    if (!_isSearchBoxEmpty)
                    {
                        SearchForResult?.Invoke(_searchBox.Text);
                    }
                }
            }

            base.Update();
        }
    }
}
