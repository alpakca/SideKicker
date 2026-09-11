using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class SearchResultsSupplier
    {
        private const int SearchResultMaxCount = 10;
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        private readonly EnumerationOptions _searchOptions;
        private readonly List<SearcherResult> _lastSearchResults;

        public IReadOnlyList<SearcherResult> LastSearchResults => _lastSearchResults;

        public SearchResultsSupplier()
        {
            _lastSearchResults = new List<SearcherResult>();
            _searchOptions = new EnumerationOptions
            {
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.PlatformDefault,
                RecurseSubdirectories = true
            };
        }

        public void UpdateSearchResults(string pattern)
        {
            _lastSearchResults.Clear();

            if (pattern.Length == 0)
            {
                return;
            }

            foreach (var entry in Directory.EnumerateFileSystemEntries(DesktopPath, "*", _searchOptions))
            {
                if (IsInsidePattern(entry, pattern))
                {
                    _lastSearchResults.Add(new SearcherResult
                    {
                        FilePath = entry,
                        FileName = Path.GetFileName(entry)
                    });

                    if (_lastSearchResults.Count == SearchResultMaxCount)
                    {
                        return;
                    }
                }
            }
        }

        private static bool IsInsidePattern(string fileName, string pattern)
        {
            int end = 0;
            int patternIndex = 0;
            for (int start = 0; start < fileName.Length; start++)
            {
                if (patternIndex >= pattern.Length)
                {
                    return true;
                }

                if (pattern.Length - patternIndex > fileName.Length - end)
                {
                    return false;
                }

                if (pattern[patternIndex] == '*' || pattern[patternIndex] == fileName[end])
                {
                    patternIndex++;
                    end++;
                }
                else
                {
                    end = start + 1;
                    patternIndex = 0;
                }
            }

            return false;
        }
    }
}
