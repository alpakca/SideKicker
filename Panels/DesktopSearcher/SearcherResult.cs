using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class SearcherResult
    {
        public string FileName { get; init; }
        public string FileAbsolutePath { get; init; }
        public string FileRelativePath { get; init; }
    }
}
