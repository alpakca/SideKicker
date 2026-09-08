using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using System.Collections.Generic;

namespace SideKicker.PanelSystem.Message
{
    internal class MessageSupporterWindow : PopWindow
    {
        private readonly List<MessageWindow> _activeMessages = new List<MessageWindow>();

        public override Rectangle Boundaries 
        {
            set
            {
                base.Boundaries = value;

                foreach (var window in _activeMessages)
                {
                    window.Location = (value.Size - window.WindowSize) / 2;
                }
            }
        }

        public void ShowMessageWindow(MessageWindow window)
        {
            window.Location = (this.Boundaries.Size - window.WindowSize) / 2;

            _activeMessages.Add(window);
            window.WindowReturned += MessageWindow_WindowReturned;
        }

        private void MessageWindow_WindowReturned(MessageWindow window)
        {
            window.WindowReturned -= MessageWindow_WindowReturned;
            _activeMessages.Remove(window);
        }

        protected sealed override void RenderUpdate()
        {
            RenderUpdateWithoutMessages();

            if (_activeMessages.Count > 0)
            {
                DrawHelper.SetBeginBatch(WindowTexture, DrawHelper.AlphaBlendDrawOptions);

                foreach (var window in _activeMessages)
                {
                    window.Render();
                }

                DrawHelper.EndBatch();
            }
        }

        protected virtual void RenderUpdateWithoutMessages()
        {
        }

        protected sealed override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            if (_activeMessages.Count > 0)
            {
                _activeMessages[^1].OnMouseButtonStateChange(e);
                return;
            }

            OnMouseButtonStateChangeWithoutMessages(e);
            base.OnMouseButtonStateChange(e);
        }

        protected virtual void OnMouseButtonStateChangeWithoutMessages(PopWindowMouseEventArgs e)
        {
        }

        protected sealed override void OnMousePositionChange(PopWindowMouseEventArgs e)
        {
            if (_activeMessages.Count > 0)
            {
                _activeMessages[^1].OnMousePositionChange(e);
                return;
            }

            OnMousePositionChangeWithoutMessages(e);
            base.OnMousePositionChange(e);
        }

        protected virtual void OnMousePositionChangeWithoutMessages(PopWindowMouseEventArgs e)
        {
        }
    }
}
