using Microsoft.Xna.Framework;
using SideKicker.Native;
using System;

namespace SideKicker.PanelSystem
{
    internal class WindowMovementManager
    {
        private const int TriggeredWindowOffset = 40;

        private int TriggeredOpenAnimationTarget => _origin.X - TriggeredWindowOffset;
        private int ClosingAnimationTarget => _origin.X;
        private int OpenAnimationTarget => _origin.X - _sidePanelBounds.Width - _shownPanelBounds.Width;
       
        private Rectangle TriggerVisibleArea
        {
            get
            {
                Point size = new Point(TriggeredWindowOffset, _shownPanelBounds.Height);
                return new Rectangle(_origin - size, size);
            }
        }
        private Rectangle OpenStateMinBoundaryBox
        {
            get
            {
                Point maxSize = new Point(_sidePanelBounds.Width + _shownPanelBounds.Width, Math.Max(_sidePanelBounds.Height, _shownPanelBounds.Height));
                return new Rectangle(_origin - maxSize, maxSize);
            }
        }

        private readonly IControllerState _controllerState;
        private readonly PanelOpenCloseAnimator _animator;

        private Point _origin;
        private Rectangle _sidePanelBounds;
        private Rectangle _shownPanelBounds;

        public Point Origin => _origin;
        public bool LocationUpdating => _animator.IsRunning;
        public Rectangle MinBoundaryBox => Rectangle.Union(_sidePanelBounds, _shownPanelBounds);

        public WindowMovementManager(IControllerState state)
        {
            _controllerState = state;
            _animator = new PanelOpenCloseAnimator();
            _animator.AnimationDone += Animator_AnimationDone;
        }

        public void Update()
        {
            // re-run the trigger animation until trigger removed
            if (_controllerState.IsTriggered && !_animator.IsRunning)
            {
                int newTarget = _animator.TargetX != TriggeredOpenAnimationTarget ? TriggeredOpenAnimationTarget : ClosingAnimationTarget;
                _animator.AnimateTo(newTarget);
            }

            if (_animator.IsRunning)
            {
                _animator.Update();
                UpdateLocations();
            }
        }

        public void SetOrigin(Point value)
        {
            _origin = value;
            SetAnimator();
            UpdateLocations();
            SetRGN();
        }

        public void SidePanelSizeChanged(Point size)
        {
            _sidePanelBounds = new Rectangle(Point.Zero, size);
            SetAnimator();
            UpdateLocations();
            SetRGN();
        }

        public void ShowedPanelSizeChanged(Point size)
        {
            _shownPanelBounds = new Rectangle(Point.Zero, size);
            SetAnimator();
            UpdateLocations();
            SetRGN();
        }

        public void RunOpenAnimation()
        {
            // restore being top-most incase it lost
            GlobalHitRegionManager.RestoreTopMost();

            // reserve the min boundary area for sliding effect drawing
            GlobalHitRegionManager.SetArea(OpenStateMinBoundaryBox);
            _animator.AnimateTo(OpenAnimationTarget);
        }

        public void RunCloseAnimation()
        {
            // reserve the min boundary area for sliding effect drawing
            GlobalHitRegionManager.SetArea(OpenStateMinBoundaryBox);
            _animator.AnimateTo(ClosingAnimationTarget);
        }

        public void StartTriggerAnimation()
        {
            GlobalHitRegionManager.SetArea(TriggerVisibleArea);
            _animator.AnimateTo(TriggeredOpenAnimationTarget);
        }

        private void SetAnimator()
        {
            if (_controllerState.IsOpen)
            {
                _animator.SetFromValue(OpenAnimationTarget);
            }
            else
            {
                _animator.SetFromValue(ClosingAnimationTarget);
            }
        }

        private void SetRGN()
        {
            if (_controllerState.IsOpen)
            {
                GlobalHitRegionManager.SetAreas(_sidePanelBounds, _shownPanelBounds);
            }
            else if (_controllerState.IsTriggered)
            {
                GlobalHitRegionManager.SetArea(TriggerVisibleArea);
            }
            else
            {
                GlobalHitRegionManager.SetArea(Rectangle.Empty);
            }
        }

        private void UpdateLocations()
        {
            _sidePanelBounds = GetSidePanelBounds(_animator.CurrentX);
            _shownPanelBounds = GetShownPanelBounds(_animator.CurrentX);

            _controllerState.SetSidePanelBounds(_sidePanelBounds);
            _controllerState.SetShownPanelBounds(_shownPanelBounds);
        }

        private void Animator_AnimationDone()
        {
            UpdateLocations();
            SetRGN();
        }

        private Rectangle GetSidePanelBounds(int animatorCurrent)
        {
            return new Rectangle(animatorCurrent + _shownPanelBounds.Width, _origin.Y - _sidePanelBounds.Height, 
                                 _sidePanelBounds.Width, _sidePanelBounds.Height);
        }

        private Rectangle GetShownPanelBounds(int animatorCurrent)
        {
            return new Rectangle(animatorCurrent, _origin.Y - _shownPanelBounds.Height, 
                                 _shownPanelBounds.Width, _shownPanelBounds.Height);
        }
    }
}
