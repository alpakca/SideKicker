using SideKicker.PanelSystem.Message;
using System.Collections.Generic;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class DesktopSearcherPanel : ContextPanel
    {
        private readonly ContextWindow _window;
        private readonly List<SearcherResult> _lastUsedResults;
        private readonly SearchResultsSupplier _searchResultSupplier;

        public override string IconId { get; } = "DesktopSearcher";
        public override string IconPath { get; } = @"panels\TimersPanelIcon.png";

        public override MessageSupporterWindow Window => _window;

        public DesktopSearcherPanel()
        {
            _lastUsedResults = new List<SearcherResult>();
            _searchResultSupplier = new SearchResultsSupplier();

            _window = new ContextWindow();
            _window.SearchForResult += Window_SearchForResult;
            _window.ResultSelected += Window_ResultSelected;

            _window.SetSearcherResults(_lastUsedResults);
        }

        private void Window_SearchForResult(string pattern)
        {
            if (pattern == string.Empty)
            {
                _window.SetSearcherResults(_lastUsedResults);
            }
            else
            {
                _searchResultSupplier.UpdateSearchResults(pattern);
                _window.SetSearcherResults(_searchResultSupplier.LastSearchResults);
            }
        }

        public override void Update()
        {
        }

        public override void OnExiting()
        {
        }

        private void Window_ResultSelected(SearcherResult result)
        {
        }
    }
}
