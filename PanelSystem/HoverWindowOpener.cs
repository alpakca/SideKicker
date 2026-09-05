using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Toolkit;

namespace SideKicker.PanelSystem
{
    internal class HoverWindowOpener
    {
        private const int HoverOpenDelay = 250;
        private const int HoverCloseDelay = 400;

        private const int HoveredAreaWidth = 50;
        private const int HoveredAreaHeight = 250;

        private readonly ContextPanelController _controller;

        private float _hoverOpenElapsed;
        private float _hoverCloseElapsed;
        private Rectangle _hoverOpenArea;

        public HoverWindowOpener(ContextPanelController controller)
        {
            _controller = controller;
        }

        public void WindowClosed()
        {
            _hoverOpenElapsed = 0;
            _hoverCloseElapsed = 0;
        }

        public void OriginUpdated(Point value)
        {
            _hoverOpenArea = new Rectangle(value.X - HoveredAreaWidth, value.Y - HoveredAreaHeight, HoveredAreaWidth, HoveredAreaHeight);
        }

        public void Update()
        {
            float deltaTime = Time.DeltaTime;
            Point mousePosition = Mouse.GetState().Position;

            if (!_controller.IsOpen)
            {
                if (_hoverOpenArea.Contains(mousePosition))
                {
                    _hoverOpenElapsed += deltaTime;
                    if (_hoverOpenElapsed >= HoverOpenDelay)
                    {
                        _controller.SetWindowState(isOpen: true, openedByHover: true);
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
            if (_controller.IsOpen && _controller.IsOpenByHover)
            {
                if (!_controller.PanelsUnionArea.Contains(mousePosition))
                {
                    _hoverCloseElapsed += deltaTime;
                    if (_hoverCloseElapsed >= HoverCloseDelay)
                    {
                        _controller.SetWindowState(isOpen: false);
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
