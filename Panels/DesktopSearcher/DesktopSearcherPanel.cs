using SideKicker.PanelSystem.Message;
using System.Collections.Generic;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class DesktopSearcherPanel : ContextPanel, ISearcherResultCollection
    {
        private readonly ContextWindow _window;
        private readonly List<SearcherResult> _lastUsedResults;
        private readonly SearchResultsSupplier _searchResultSupplier;

        public override string IconId { get; }
        public override string IconPath { get; }

        public override MessageSupporterWindow Window => _window;
        public IReadOnlyList<SearcherResult> Results => _window.IsSearchBoxEmpty ? _lastUsedResults : _searchResultSupplier.LastSearchResults;

        public DesktopSearcherPanel()
        {
            _searchResultSupplier = new SearchResultsSupplier();

            _window = new ContextWindow(this);
            _window.SearchForResult += _searchResultSupplier.UpdateSearchResults;
            _window.ResultSelected += Window_ResultSelected;
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
