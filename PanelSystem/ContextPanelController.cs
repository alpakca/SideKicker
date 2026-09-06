using Microsoft.Xna.Framework;
using PopUpUI.Toolkit;
using SideKicker.Native;
using SideKicker.Panels;
using SideKicker.Panels.PinnedCommands;
using SideKicker.Panels.Timers;
using SideKicker.Theming;
using System;

namespace SideKicker.PanelSystem
{
    internal class ContextPanelController
    {
        private const int TriggeredWindowOffset = 50;
        private static readonly int TriggeredWindowMinPosition = WindowOptions.Width - TriggeredWindowOffset;
        private static readonly int ScreenWidth = WindowOptions.Width;

        private readonly ContextPanel[] _allPanels;
        private readonly PanelWindow _panelWindow;
        private readonly PanelOpenCloseAnimator _openCloseAnimator;
        private readonly HoverWindowOpener _hoverWindowOpener;

        private ContextPanel _openPanel;

        private Point _origin;
        private bool _isOpen = false;
        private bool _openedByHover = false;
        private bool _isTriggered = false;

        public bool IsOpen => _isOpen;
        public bool IsOpenByHover => _openedByHover;
        public Rectangle PanelsUnionArea => Rectangle.Union(_panelWindow.Boundaries, _openPanel.Window.Boundaries);

        public ContextPanelController()
        {
            _allPanels = [new PinnedCommandsPanel(), new TimersPanel()];
            foreach (var panel in _allPanels) panel.TriggeredWindowRequested += TriggerWindowShowcase;

            _panelWindow = new PanelWindow(_allPanels);
            _hoverWindowOpener = new HoverWindowOpener(this);

            PopWindowManager.ActiveWindows.Add(_panelWindow);

            _panelWindow.ContextPanelToggled += OpenPanel;
            InputManager.MouseButtonStateChange += InputManager_MouseButtonStateChange;

            InitializeTheme();

            _openCloseAnimator = new PanelOpenCloseAnimator(ScreenWidth);
            _openCloseAnimator.AnimationDone += OpenCloseAnimator_AnimationDone;
            _origin = new Point(ScreenWidth, 920); // replace with a preference setting reading
            OpenPanel(_allPanels[0]); // replace with a preference setting reading, as index?

            GlobalHitRegionManager.SetArea(new Rectangle()); // make controller is closed by default
        }

        private void InputManager_MouseButtonStateChange(PopUpUI.Input.PopWindowMouseEventArgs e)
        {
            // close the controller if did not hit any of open panel bounds
            // we check union here since we don't want window to be closed-opened continuesly
            // when mouse click happens in space boundary between Union area & actual window bounds
            if (e.FirstTimeButtons.HasLeft && !PanelsUnionArea.Contains(e.MousePosition))
            {
                SetWindowState(isOpen: false);
            }
        }

        public void Update()
        {
            foreach (var panel in _allPanels) panel.Update();
            _hoverWindowOpener.Update();

            // re-run the trigger animation until trigger removed
            if (_isTriggered && !_openCloseAnimator.IsRunning)
            {
                _openCloseAnimator.AnimateTo(_openCloseAnimator.CurrentX == TriggeredWindowMinPosition ? ScreenWidth : TriggeredWindowMinPosition);
            }

            if (_openCloseAnimator.IsRunning)
            {
                _openCloseAnimator.Update();
                UpdateWindowsLocations();
            }
        }

        public void OnExiting()
        {
            foreach (var panel in _allPanels) panel.OnExiting();
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

                int totalPanelsMaxWidth = _openPanel.Window.Boundaries.Width + _panelWindow.Boundaries.Width;
                int totalPanelsMaxHeight = Math.Max(_openPanel.Window.Boundaries.Height, _panelWindow.Boundaries.Height);
                Point maxSize = new Point(totalPanelsMaxWidth, totalPanelsMaxHeight);

                // restore being top-most incase it lost
                GlobalHitRegionManager.RestoreTopMost();

                // before starting to open, set RGN to maximum space to reserve the area for drawing while animating
                GlobalHitRegionManager.SetArea(new Rectangle(_origin - maxSize, maxSize));
                _openCloseAnimator.AnimateTo(_origin.X - maxSize.X);
            }
            else
            {
                _hoverWindowOpener.WindowClosed();

                // before starting to close, set RGN to maximum space to there won't be holes that are not drawn while animating
                GlobalHitRegionManager.SetArea(Rectangle.Union(_openPanel.Window.Boundaries, _panelWindow.Boundaries));
                _openCloseAnimator.AnimateTo(_origin.X);
            }

            UpdateWindowsLocations();
        }

        /// <summary>Creates a visual trigger effect until window is opened.</summary>
        private void TriggerWindowShowcase(ContextPanel panel)
        {
            if (_isOpen) return;

            _isTriggered = true;
            OpenPanel(panel);

            UpdateRGNArea();
            _openCloseAnimator.AnimateTo(TriggeredWindowMinPosition);
        }

        private void OpenPanel(ContextPanel panel)
        {
            PopWindowManager.ActiveWindows.Remove(_openPanel?.Window);

            _openPanel = panel;
            _panelWindow.OpenPanelUpdated(panel);

            PopWindowManager.ActiveWindows.Add(_openPanel.Window);

            OriginUpdated();
            UpdateRGNArea();
        }

        #region Animator Positions
        private void OpenCloseAnimator_AnimationDone()
        {
            // animation ended - call it to correctly update locations to last positions
            UpdateWindowsLocations();
            UpdateRGNArea();
        }

        private void UpdateRGNArea()
        {
            if (_isOpen)
            {
                GlobalHitRegionManager.SetAreas(_openPanel.Window.Boundaries, _panelWindow.Boundaries);
            }
            else if (_isTriggered) // window only may be triggered if window is closed
            {
                int openPanelHeight = _openPanel.Window.Boundaries.Height;
                GlobalHitRegionManager.SetArea(new Rectangle(_origin.X - TriggeredWindowOffset, _origin.Y - openPanelHeight,
                                               TriggeredWindowOffset, openPanelHeight));
            }
            else
            {
                GlobalHitRegionManager.SetArea(new Rectangle());
            }
        }

        private void UpdateWindowsLocations()
        {
            Rectangle openPanelBounds = _openPanel.Window.Boundaries;
            _openPanel.Window.Boundaries = new Rectangle(_openCloseAnimator.CurrentX, openPanelBounds.Y,
                                                         openPanelBounds.Width, openPanelBounds.Height);

            Rectangle controllerPanelBounds = _panelWindow.Boundaries;
            _panelWindow.Boundaries = new Rectangle(openPanelBounds.Right, controllerPanelBounds.Y,
                                                    controllerPanelBounds.Width, controllerPanelBounds.Height);
        }

        private void OriginUpdated()
        {
            Rectangle openPanelBounds = _openPanel.Window.Boundaries;
            _openPanel.Window.Boundaries = new Rectangle(_origin.X - openPanelBounds.Width - _panelWindow.Boundaries.Width, _origin.Y - openPanelBounds.Height,
                                                         openPanelBounds.Width, openPanelBounds.Height);

            Rectangle controllerPanelBounds = _panelWindow.Boundaries;
            _panelWindow.Boundaries = new Rectangle(_origin.X - controllerPanelBounds.Width, _origin.Y - controllerPanelBounds.Height,
                                                    controllerPanelBounds.Width, controllerPanelBounds.Height);

            _hoverWindowOpener.OriginUpdated(_origin);
        }
        #endregion

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
