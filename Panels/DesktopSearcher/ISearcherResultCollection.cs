using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.DesktopSearcher
{
    internal interface ISearcherResultCollection
    {
        public IReadOnlyList<SearcherResult> Results { get; }
    }
}
