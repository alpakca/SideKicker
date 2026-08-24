using SideKicker.Panels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal class ContextPanelController
    {
        private readonly ContextPanel[] _allPanels;
        private readonly PanelWindow _panelWindow;

        private ContextPanel _openPanel;

        public ContextPanelController()
        {
            _allPanels = []; // replace with creation of context panel instances
            _panelWindow = new PanelWindow(_allPanels);
        }

        public void OpenPanelAt(int index)
        {
            _openPanel = _allPanels[index];
        }
    }
}
