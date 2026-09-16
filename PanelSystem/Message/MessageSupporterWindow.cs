using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Native;
using SideKicker.Panels;
using System;
using System.Collections.Generic;

namespace SideKicker.PanelSystem.Message
{
    internal class MessageSupporterWindow : PopWindow
    {
        private readonly List<MessageWindow> _activeMessages = new List<MessageWindow>();
        private readonly List<IClipboardSupporter> _clipboardSupporters = new List<IClipboardSupporter>();

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

        public MessageSupporterWindow()
        {
            KeyboardShortcutManager manager = new KeyboardShortcutManager();

            KeyboardCombination copyLeft = KeyboardCombination.GetSingleUse([Keys.LeftControl, Keys.C]);
            KeyboardCombination copyRight = KeyboardCombination.GetSingleUse([Keys.RightControl, Keys.C]);
            copyLeft.FirstTriggerAction = copyRight.FirstTriggerAction = TryCopyToClipboard;

            KeyboardCombination pasteLeft = KeyboardCombination.GetSingleUse([Keys.LeftControl, Keys.V]);
            KeyboardCombination pasteRight = KeyboardCombination.GetSingleUse([Keys.RightControl, Keys.V]);
            pasteLeft.FirstTriggerAction = pasteRight.FirstTriggerAction = TryGetFromClipboard;

            manager.AddCombinations(copyLeft, copyRight, pasteLeft, pasteRight);
            this.Elements.Add(manager);
        }

        protected void RegisterClipboardSupporters(params ReadOnlySpan<IClipboardSupporter> supporters)
        {
            _clipboardSupporters.AddRange(supporters);
        }

        protected void RegisterClipboardSupporters(IEnumerable<IClipboardSupporter> supporters)
        {
            _clipboardSupporters.AddRange(supporters);
        }

        private void TryCopyToClipboard()
        {
            foreach (IClipboardSupporter supporter in _clipboardSupporters)
            {
                if (supporter.ReceivingInput)
                {
                    string? selected = supporter.GetSelectedText();

                    if (selected == null)
                    {
                        continue;
                    }

                    NativeService.Current.CopyToClipboard(selected);
                    return;
                }
            }
        }

        private void TryGetFromClipboard()
        {
            if (!NativeService.Current.TryGetClipboardText(out string text))
            {
                return;
            }

            foreach (IClipboardSupporter supporter in _clipboardSupporters)
            {
                if (supporter.ReceivingInput)
                {
                    supporter.CopyFromClipboard(text);
                    return;
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
