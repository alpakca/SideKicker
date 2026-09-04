using Microsoft.Xna.Framework;
using PopUpUI.Toolkit;
using SideKicker.Native;
using SideKicker.Panels;
using SideKicker.Panels.PinnedCommands;
using SideKicker.Theming;
using System;

namespace SideKicker.PanelSystem
{
    internal class ContextPanelController
    {
        private readonly ContextPanel[] _allPanels;
        private readonly PanelWindow _panelWindow;

        private ContextPanel _openPanel;

        private Point _origin;
        private bool _isOpen;

        public ContextPanelController()
        {
            _allPanels = [new PinnedCommandsPanel()];
            _panelWindow = new PanelWindow(this, _allPanels);
            _panelWindow.ContextPanelToggled += OpenPanelAt;

            InitializeTheme();

            _origin = new Point(1920, 920); // replace with a preference setting reading
            OpenPanelAt(0); // replace with a preference setting reading

            PopWindowManager.ActiveWindows.Add(_panelWindow);
        }

        public void OnExiting()
        {
            foreach (var panel in _allPanels) panel.OnExiting();
        }

        public void TogglePanel()
        {
            _isOpen = !_isOpen;
            if (_isOpen)
            {
                UpdateWindowLocations();
            }
            else
            {
                GlobalHitRegionManager.SetArea(new Rectangle());
            }
        }

        private void OpenPanelAt(int index)
        {
            PopWindowManager.ActiveWindows.Remove(_openPanel?.Window);

            _openPanel = _allPanels[index];
            _panelWindow.OpenPanelUpdated(index);

            PopWindowManager.ActiveWindows.Add(_openPanel.Window);

            UpdateWindowLocations();
        }

        private void UpdateWindowLocations()
        {
            Point minimumWindowSize = new Point(_openPanel.Window.Boundaries.Width + _panelWindow.Boundaries.Width,
                                                Math.Max(_panelWindow.Boundaries.Height, _openPanel.Window.Boundaries.Height));

            Point openPanelPosition = _origin - minimumWindowSize;

            _openPanel.Window.Boundaries = new Rectangle(openPanelPosition, _openPanel.Window.Boundaries.Size);
            _panelWindow.Boundaries = new Rectangle(openPanelPosition + minimumWindowSize - _panelWindow.Boundaries.Size, _panelWindow.Boundaries.Size);

            GlobalHitRegionManager.SetAreas(_openPanel.Window.Boundaries, _panelWindow.Boundaries);
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
    }
}
