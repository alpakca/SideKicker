using Microsoft.Xna.Framework;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using SideKicker.Panels.DesktopSearcher;
using SideKicker.Panels.PinnedCommands;
using SideKicker.Panels.Timers;
using SideKicker.PanelSystem.Settings;
using SideKicker.Theming;
using System;

namespace SideKicker.PanelSystem
{
    internal class ContextPanelController : IControllerState
    {
        private readonly SettingsService _settings;
        private readonly ContextPanel[] _allPanels;
        private readonly PanelWindow _panelWindow;
        private readonly WindowMovementManager _windowMovement;

        private ContextPanel _openPanel;

        private bool _isOpen = false;
        private bool _openedByHover = false;
        private bool _isTriggered = false;

        public bool IsOpen => _isOpen;
        public bool IsOpenByHover => _openedByHover;
        public bool IsTriggered => _isTriggered;
        public Rectangle MinBoundaryBox => _windowMovement.MinBoundaryBox;

        public bool IsMoving => _windowMovement.LocationUpdating;
        public SettingsService Settings => _settings;

        public ContextPanelController()
        {
            _settings = new SettingsService();
            _settings.OriginUpdated += Settings_PreferenceValueUpdated;

            _allPanels = [new PinnedCommandsPanel(), new TimersPanel(), new DesktopSearcherPanel()];
            foreach (var panel in _allPanels) panel.TriggeredWindowRequested += TriggerWindowShowcase;

            _panelWindow = new PanelWindow(_allPanels);
            _panelWindow.ContextPanelToggled += ShowPanel;

            PopWindowManager.ActiveWindows.Add(_panelWindow, PopWindowZIndex.ControllerPanel);
            InputManager.MouseButtonStateChange += InputManager_MouseButtonStateChange;

            _windowMovement = new WindowMovementManager(this);
            _windowMovement.SidePanelSizeChanged(_panelWindow.Boundaries.Size);

            SetOrigin(new Point(_settings.Preferences.OriginX, _settings.Preferences.OriginY));
            ShowPanel(_allPanels[_settings.Preferences.ShownPanelIndex]);
            InitializeTheme();
        }

        private void Settings_PreferenceValueUpdated()
        {
            SetOrigin(_settings.Preferences.Origin);
        }

        private void InputManager_MouseButtonStateChange(PopUpUI.Input.PopWindowMouseEventArgs e)
        {
            // close the controller if did not hit any of open panel bounds
            // we check union here since we don't want window to be closed-opened continuesly
            // when mouse click happens in space boundary between Union area & actual window bounds
            if (e.FirstTimeButtons.HasLeft && !MinBoundaryBox.Contains(e.MousePosition))
            {
                SetWindowState(isOpen: false);
            }
        }

        public void Update()
        {
            foreach (var panel in _allPanels) panel.Update();
            _windowMovement.Update();
        }

        public void OnExiting()
        {
            foreach (var panel in _allPanels) panel.OnExiting();

            _settings.Preferences.ShownPanelIndex = Array.IndexOf<ContextPanel>(_allPanels, _openPanel);
            _settings.Save();
        }

        public void ToggleWindow()
        {
            SetWindowState(!_isOpen);
        }

        public void SetWindowState(bool isOpen, bool openedByHover = false)
        {
            if (_isOpen == isOpen) return;

            _isOpen = isOpen;
            _openedByHover = openedByHover;

            if (isOpen)
            {
                _isTriggered = false;
                _windowMovement.RunOpenAnimation();
            }
            else
            {
                _windowMovement.RunCloseAnimation();
            }
        }

        /// <summary>Creates a visual trigger effect until window is opened.</summary>
        private void TriggerWindowShowcase(ContextPanel panel)
        {
            ShowPanel(panel);

            if (_isOpen) return;

            _isTriggered = true;
            _windowMovement.StartTriggerAnimation();
        }

        private void ShowPanel(ContextPanel panel)
        {
            PopWindowManager.ActiveWindows.Remove(_openPanel?.Window);

            _openPanel = panel;
            _panelWindow.OpenPanelUpdated(panel);

            PopWindowManager.ActiveWindows.Add(_openPanel.Window, PopWindowZIndex.ContextPanel);

            _windowMovement.ShowedPanelSizeChanged(_openPanel.Window.Boundaries.Size);
        }

        private void InitializeTheme()
        {
            foreach (var panel in _allPanels)
            {
                IconRegistry.Register(new IconDefinition { Id = panel.IconId, DefaultPath = panel.IconPath });
            }

            ThemeService.Refresh();
            ThemeService.SetTheme(_settings.Preferences.ThemeId);
        }

        private void SetOrigin(Point value)
        {
            _windowMovement.SetOrigin(value);
        }

        public void SetSidePanelBounds(Rectangle value)
        {
            _panelWindow?.Boundaries = value;
        }

        public void SetShownPanelBounds(Rectangle value)
        {
            _openPanel?.Window.Boundaries = value;
        }
    }
}
