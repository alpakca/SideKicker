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
        private readonly List<SavedCommand> _savedCommands;

        public override string IconId => "PinnedActions";
        public override string IconPath => @"panels\PinnedActionsPanelIcon.png";

        public override PopWindow Window => _contextWindow;

        public PinnedACommandsPanel()
        {
            _savedCommands = new List<SavedCommand>();
            _contextWindow = new ContextWindow(_savedCommands);

            _contextWindow.EditorButtonPressed += Window_EditorButtonPressed;
            _contextWindow.CommandListButtonBodyPressed += Window_CommandListButtonBodyPressed;
            _contextWindow.CommandListButtonEditPressed += Window_CommandListButtonEditPressed;
            _contextWindow.CommandListButtonsSwaped += Window_CommandListButtonsSwaped;

            _contextWindow.ShowCommand(null);
        }

        private void Window_CommandListButtonBodyPressed(int buttonIndex)
        {
            // run command
        }

        private void Window_CommandListButtonEditPressed(int buttonIndex)
        {
            // change state of editor to "edit"
        }

        private void Window_CommandListButtonsSwaped(int firstButtonIndex, int secondButtonIndex)
        {
            // swap two commands in the list
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
    }
}
