using SideKicker.PanelSystem.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class ContextWindow : MessageSupporterWindow
    {
        private readonly LinearTextBox _searchBox;
        private readonly ScrollableButtonList _resultsList;

        private readonly TextPlaque _emptySearchBoxPlaque;
        private readonly TextPlaque _nonEmptySearchBoxPlaque;

        public ContextWindow()
        {

        }
    }
}
