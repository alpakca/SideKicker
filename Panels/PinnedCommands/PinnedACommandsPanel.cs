using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.PinnedCommands
{
    internal class PinnedACommandsPanel : ContextPanel
    {
        private readonly ContextWindow _contextWindow;

        private bool _inEditingCommandState;

        public override string IconId => "PinnedActions";
        public override string IconPath => @"panels\PinnedActionsPanelIcon.png";

        public override PopWindow Window => _contextWindow;

        public PinnedACommandsPanel()
        {
            _contextWindow = new ContextWindow();

            _contextWindow.EditorButtonPressed += Window_EditorButtonPressed;
            _contextWindow.CommandListButtonPressed += Window_CommandListButtonPressed;
        }

        private void Window_EditorButtonPressed(EditorButton buttonType)
        {
            switch (buttonType)
            {
                case EditorButton.Add:
                    break;

                case EditorButton.Save:
                    break;

                case EditorButton.Delete:
                    break;
            }
        }

        private void Window_CommandListButtonPressed(int buttonIndex, CommandButtonLayout layout)
        {
            switch (layout)
            {
                case CommandButtonLayout.Body:
                    break;

                case CommandButtonLayout.EditButton:
                    break;

                case CommandButtonLayout.ReorderButton:
                    break;
            }
        }
    }
}
