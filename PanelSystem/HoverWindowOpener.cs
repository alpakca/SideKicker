using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace SideKicker.PanelSystem
{
    internal static class HoverWindowOpener
    {
        private const int HoverOpenDelay = 250;
        private const int HoverCloseDelay = 400;

        private const int HoveredAreaWidth = 50;
        private const int HoveredAreaHeight = 250;

        private static float _hoverOpenElapsed;
        private static float _hoverCloseElapsed;
        private static Rectangle _hoverOpenArea;
        private static IControllerState _controllerState;

        public static event Action OpenControllerWindow;
        public static event Action CloseControllerWindow;

        public static void WindowClosed()
        {
            _hoverOpenElapsed = 0;
            _hoverCloseElapsed = 0;
        }

        public static void Initialize(IControllerState state)
        {
            _controllerState = state;
        }

        public static void OriginUpdated(Point value)
        {
            _hoverOpenArea = new Rectangle(value.X - HoveredAreaWidth, value.Y - HoveredAreaHeight, HoveredAreaWidth, HoveredAreaHeight);
        }

        public static void Update()
        {
            Point mousePosition = Mouse.GetState().Position;

            if (!_controllerState.IsOpen)
            {
                if (_hoverOpenArea.Contains(mousePosition))
                {
                    _hoverOpenElapsed += TimeManager.DeltaTime;
                    if (_hoverOpenElapsed >= HoverOpenDelay)
                    {
                        OpenControllerWindow?.Invoke();
                        _hoverOpenElapsed = 0f;
                    }
                }
                else
                {
                    _hoverOpenElapsed = 0f;
                }
            }
            else
            {
                _hoverOpenElapsed = 0f;
            }

            // hover to close, only possible if window was opened by hover (not by hotkey)
            if (_controllerState.IsOpen && _controllerState.IsOpenByHover)
            {
                if (!_controllerState.MinBoundaryBox.Contains(mousePosition))
                {
                    _hoverCloseElapsed += TimeManager.DeltaTime;
                    if (_hoverCloseElapsed >= HoverCloseDelay)
                    {
                        CloseControllerWindow?.Invoke();
                        _hoverCloseElapsed = 0f;
                    }
                }
                else
                {
                    _hoverCloseElapsed = 0f;
                }
            }
            else
            {
                _hoverCloseElapsed = 0f;
            }
        }
    }
}
