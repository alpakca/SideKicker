using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using SideKicker.Panels.PinnedCommands;
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
            _allPanels = [new PinnedCommandsPanel(), new Test(), new Test()]; // replace with creation of context panel instances
            _panelWindow = new PanelWindow(this, _allPanels);

            InitializeTheme();

            _origin = new Point(1920, 920); // replace with a preference setting reading
            OpenPanelAt(0); // replace with a preference setting reading

            PopWindowManager.ActiveWindows.Add(_panelWindow);
        }

        public void OnExiting()
        {
            foreach (var panel in _allPanels) panel.OnExiting();
        }

        public void OpenPanelAt(int index)
        {
            PopWindowManager.ActiveWindows.Remove(_openPanel?.Window);

            _openPanel = _allPanels[index];
            _panelWindow.OpenPanelUpdated(index);

            PopWindowManager.ActiveWindows.Add(_openPanel.Window);

            UpdateWindowLocations();
        }

        public void UpdateWindowLocations()
        {
            Point minimumWindowSize = new Point(_openPanel.Window.Boundaries.Width + _panelWindow.Boundaries.Width,
                                                Math.Max(_panelWindow.Boundaries.Height, _openPanel.Window.Boundaries.Height));

            Point openPanelPosition = _origin - minimumWindowSize;

            _openPanel.Window.Boundaries = new Rectangle(openPanelPosition, _openPanel.Window.Boundaries.Size);
            _panelWindow.Boundaries = new Rectangle(openPanelPosition + minimumWindowSize - _panelWindow.Boundaries.Size, _panelWindow.Boundaries.Size);

            OverlayManager.SetAreas(_openPanel.Window.Boundaries, _panelWindow.Boundaries);
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

            public override PopWindow Window { get; } = new PopWindow { Boundaries = new Rectangle(0, 0, 1, 1) };

            public override void OnExiting()
            {
            }
        }
    }
}
