using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using SideKicker.Theming;
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
            _allPanels = [new Test(), new Test(), new Test()]; // replace with creation of context panel instances
            _panelWindow = new PanelWindow(_allPanels);

            InitializeTheme();
            OpenPanelAt(0); // replace with a preference setting reading

            PopWindowManager.ActiveWindows.Add(_panelWindow);
        }

        public void OpenPanelAt(int index)
        {
            _openPanel = _allPanels[index];
            _panelWindow.OpenPanelUpdated(index);
        }

        private void InitializeTheme()
        {
            foreach (var panel in _allPanels)
            {
                IconRegistry.Register(new IconDefinition { Id = panel.IconId, DefaultPath = panel.IconPath });
            }

            ThemeService.Refresh();
            ThemeService.SetTheme("dark"); // replace with a preference setting reading

        }

        private class Test : ContextPanel
        {
            public override string IconId => "test";

            public override string IconPath => "panels\\test.png";

            public override Point Size => throw new NotImplementedException();

            public override PopWindow Window => throw new NotImplementedException();
        }
    }
}
