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
        private Point _origin;

        public ContextPanelController()
        {
            _allPanels = [new Test(), new Test(), new Test()]; // replace with creation of context panel instances
            _panelWindow = new PanelWindow(this, _allPanels);

            InitializeTheme();

            _origin = new Point(1920, 920); // replace with a preference setting reading
            OpenPanelAt(0); // replace with a preference setting reading

            PopWindowManager.ActiveWindows.Add(_panelWindow);
        }

        public void OpenPanelAt(int index)
        {
            if (_allPanels.IndexOf(_openPanel) == index)
                return;

            PopWindowManager.ActiveWindows.Remove(_openPanel?.Window);

            _openPanel = _allPanels[index];
            _panelWindow.OpenPanelUpdated(index);

            PopWindowManager.ActiveWindows.Add(_openPanel.Window);

            UpdateWindowLocations();
        }

        public void UpdateWindowLocations()
        {
            Point minimumWindowSize = new Point(_openPanel.Size.X + _panelWindow.Boundaries.Width,
                                                Math.Max(_panelWindow.Boundaries.Height, _openPanel.Window.Boundaries.Height));

            WindowOptions.Position = _origin - minimumWindowSize;
            WindowOptions.Size = minimumWindowSize;

            _openPanel.Window.Boundaries = new Rectangle(default, _openPanel.Size);
            _panelWindow.Boundaries = new Rectangle(minimumWindowSize - _panelWindow.Boundaries.Size, _panelWindow.Boundaries.Size);

            WindowTransparency.SetClickableRegions([_openPanel.Window.Boundaries, _panelWindow.Boundaries]);
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

            public override Point Size { get; } = new Point(200, 200);

            public override PopWindow Window { get; } = new PopWindow();
        }
    }
}
