using SideKicker.PanelSystem.Message;
using System.Diagnostics;
using System.IO;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class DesktopSearcherPanel : ContextPanel
    {
        private readonly ContextWindow _window;
        private readonly LastSearcherResultList _lastUsedResults;
        private readonly SearchResultsSupplier _searchResultSupplier;

        public override string IconId { get; } = "DesktopSearcher";
        public override string IconPath { get; } = @"panels\TimersPanelIcon.png";

        public override MessageSupporterWindow Window => _window;

        public DesktopSearcherPanel()
        {
            _lastUsedResults = new LastSearcherResultList();
            _searchResultSupplier = new SearchResultsSupplier();

            _window = new ContextWindow();
            _window.SearchForResult += ShowSearcherResult;
            _window.ResultSelected += Window_ResultSelected;

            ShowSearcherResult(string.Empty);
        }

        private void ShowSearcherResult(string pattern)
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
            _lastUsedResults.Enqueue(result);

            using Process proc = Process.Start("explorer.exe", $"\"{Path.GetDirectoryName(result.FileAbsolutePath)}\"");
        }
    }
}
