using PopUpUI.Toolkit;
using SideKicker.PanelSystem.Message;
using System.Diagnostics;
using System.IO;

using SearcherSerializer = SideKicker.Panels.PanelContentSerializer<SideKicker.Panels.DesktopSearcher.SearcherResult>;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class DesktopSearcherPanel : ContextPanel
    {
        private const int LastUsedResultsDirtyCheckTime = 1_200_000; // 20 minutes

        static DesktopSearcherPanel()
        {
            SearcherSerializer.SaveFileName = "lastsearchedresults.json";
        }

        private readonly ContextWindow _window;
        private readonly LastSearcherResultList _lastUsedResults;
        private readonly SearchResultsSupplier _searchResultSupplier;

        private bool _lastUsedResultsDirty;
        private float _elapsedSinceLastDirtyCheck;

        public override string IconId => PanelIconValues.DesktopSearcher_IconId;
        public override string IconPath => PanelIconValues.DesktopSearcher_IconPath;
        public override ContextPanelType Type => ContextPanelType.DesktopSearcher;
        public override MessageSupporterWindow Window => _window;

        public DesktopSearcherPanel()
        {
            _lastUsedResults = new LastSearcherResultList(SearcherSerializer.Load());
            _searchResultSupplier = new SearchResultsSupplier();

            _window = new ContextWindow();
            _window.SearchForResult += ShowSearcherResult;
            _window.ResultSelected += Window_ResultSelected;
            _window.ResultCloseSelected += Window_ResultCloseSelected;

            ShowSearcherResult(string.Empty);
        }

        private void ShowSearcherResult(string pattern)
        {
            if (pattern == string.Empty)
            {
                _window.SetSearcherResults(_lastUsedResults, showClosingOption: true);
            }
            else
            {
                _searchResultSupplier.UpdateSearchResults(pattern);
                _window.SetSearcherResults(_searchResultSupplier.LastSearchResults, showClosingOption: false);
            }
        }

        public override void Update()
        {
            _elapsedSinceLastDirtyCheck += Time.DeltaTime;
            if (_elapsedSinceLastDirtyCheck >= LastUsedResultsDirtyCheckTime)
            {
                _elapsedSinceLastDirtyCheck = 0;

                if (_lastUsedResultsDirty)
                {
                    _lastUsedResultsDirty = false;
                    _ = SearcherSerializer.SaveAsync(_lastUsedResults);
                }
            }
        }

        public override void OnExiting()
        {
            if (_lastUsedResultsDirty)
            {
                _lastUsedResultsDirty = false;
                SearcherSerializer.Save(_lastUsedResults);
            }
        }

        private void Window_ResultSelected(SearcherResult result)
        {
            _lastUsedResults.Enqueue(result);
            _lastUsedResultsDirty = true;

            using Process proc = Process.Start("explorer.exe", $"\"{Path.GetDirectoryName(result.FileAbsolutePath)}\"");
        }

        private void Window_ResultCloseSelected(SearcherResult result)
        {
            _lastUsedResults.Remove(result);
            _lastUsedResultsDirty = true;

            _window.SetSearcherResults(_lastUsedResults, showClosingOption: true);
        }
    }
}
