using PopUpUI.Surface;
using System.Collections.ObjectModel;

namespace SideKicker.Panels.PinnedCommands
{
    internal class PinnedCommandsPanel : ContextPanel
    {
        private SavedCommand _editorShownCommand;

        private readonly ContextWindow _contextWindow;
        private readonly ObservableCollection<SavedCommand> _savedCommands;

        public override string IconId => "PinnedActions";
        public override string IconPath => @"panels\PinnedActionsPanelIcon.png";

        public override PopWindow Window => _contextWindow;

        public PinnedCommandsPanel()
        {
            _savedCommands = new ObservableCollection<SavedCommand>();
            _contextWindow = new ContextWindow(_savedCommands);

            _contextWindow.EditorButtonPressed += Window_EditorButtonPressed;
            _contextWindow.CommandListButtonBodyPressed += Window_CommandListButtonBodyPressed;
            _contextWindow.CommandListButtonEditPressed += Window_CommandListButtonEditPressed;
            _contextWindow.CommandListButtonsSwaped += Window_CommandListButtonsSwaped;

            _editorShownCommand = null;
            _contextWindow.ShowCommand(null);

            _savedCommands.Add(new SavedCommand { Name = "SideKicker Git", Command = "", PathToRunCommand = "" });
        }

        private void Window_CommandListButtonBodyPressed(int buttonIndex)
        {
            // run command
        }

        private void Window_CommandListButtonEditPressed(int buttonIndex)
        {
            SetEditorShowCommand(_savedCommands[buttonIndex]);
        }

        private void Window_CommandListButtonsSwaped(int firstButtonIndex, int secondButtonIndex)
        {
            (_savedCommands[firstButtonIndex], _savedCommands[secondButtonIndex]) = (_savedCommands[secondButtonIndex], _savedCommands[firstButtonIndex]);
        }

        private void Window_EditorButtonPressed(EditorButton buttonType)
        {
            switch (buttonType)
            {
                case EditorButton.Add:

                    _savedCommands.Add(_contextWindow.CreateCommand());
                    SetEditorShowCommand(null);
                    break;

                case EditorButton.Save:

                    int indexOfSave = _savedCommands.IndexOf(_editorShownCommand);
                    if (indexOfSave == -1) return;

                    SavedCommand newCommand = _contextWindow.CreateCommand();

                    _savedCommands[indexOfSave] = newCommand;
                    SetEditorShowCommand(null);
                    break;

                case EditorButton.Delete:

                    int indexOfDelete = _savedCommands.IndexOf(_editorShownCommand);
                    if (indexOfDelete == -1) return;

                    _savedCommands.RemoveAt(indexOfDelete);
                    SetEditorShowCommand(null);
                    break;
            }
        }

        private void SetEditorShowCommand(SavedCommand command)
        {
            _editorShownCommand = command;
            _contextWindow.ShowCommand(command);
        }
    }
}
