using Microsoft.Xna.Framework;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using SideKicker.Panels.DesktopSearcher;
using SideKicker.Panels.PinnedCommands;
using SideKicker.Panels.Settings;
using SideKicker.Panels.Timers;
using SideKicker.PanelSystem.Settings;
using SideKicker.Theming;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SideKicker.PanelSystem
{
    internal class ContextPanelController : IControllerState
    {
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
        public Preferences Preferences { get; }

        public ContextPanelController()
        {
            Preferences = PreferenceSerializer.Load();
            Preferences.OriginUpdated += SetOrigin;
            Preferences.PanelsReordered += SwapPanelOrder;

            _allPanels = [new PinnedCommandsPanel(), new TimersPanel(), new DesktopSearcherPanel(), new SettingsPanel(Preferences)];
            foreach (var panel in _allPanels) panel.TriggeredWindowRequested += TriggerWindowShowcase;

            // adjust order of panels using order from preference
            Array.Sort(_allPanels, (a, b) => Preferences.PanelOrder.IndexOf(a.Type).CompareTo(Preferences.PanelOrder.IndexOf(b.Type)));

            _panelWindow = new PanelWindow(_allPanels);
            _panelWindow.ContextPanelToggled += ShowPanel;

            PopWindowManager.ActiveWindows.Add(_panelWindow, PopWindowZIndex.ControllerPanel);
            InputManager.MouseButtonStateChange += InputManager_MouseButtonStateChange;

            _windowMovement = new WindowMovementManager(this);
            _windowMovement.SidePanelSizeChanged(_panelWindow.Boundaries.Size);

            SetOrigin(Preferences.Origin);
            ShowPanel(_allPanels[Preferences.ShownPanelIndex]);

            ThemeService.Refresh();
            ThemeService.SetTheme(Preferences.ThemeId);
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

            PreferenceSerializer.Save(Preferences);
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

        public void SetSidePanelBounds(Rectangle value)
        {
            _panelWindow?.Boundaries = value;
        }

        public void SetShownPanelBounds(Rectangle value)
        {
            _openPanel?.Window.Boundaries = value;
        }

        private void SetOrigin(Point value)
        {
            _windowMovement.SetOrigin(value);
        }

        private void SwapPanelOrder(int firstPanelIndex, int secondPanelIndex)
        {
            (_allPanels[firstPanelIndex], _allPanels[secondPanelIndex]) = (_allPanels[secondPanelIndex], _allPanels[firstPanelIndex]);

            _panelWindow.PanelOrderSwapped(firstPanelIndex, secondPanelIndex);
        }
    }
}
