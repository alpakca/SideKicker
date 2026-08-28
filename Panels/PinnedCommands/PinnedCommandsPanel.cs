using PopUpUI.Surface;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

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

            _savedCommands.Add(new SavedCommand 
            {
                Name = "SideKicker Git", 
                FileName = "C:\\Program Files\\Git\\git-bash.exe", 
                PathToRunCommand = @"C:\Users\user\Desktop\C#\SideKicker\SideKicker" 
            });
        }

        private void Window_CommandListButtonBodyPressed(int buttonIndex)
        {
            SavedCommand command = _savedCommands[buttonIndex];
            try
            {
                using Process cmdWindow = Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = command.PathToRunCommand,
                    FileName = command.FileName,
                    UseShellExecute = true,
                });
            }
            catch (Win32Exception)
            {

            }
            catch (InvalidOperationException)
            {
                
            }
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

                    SavedCommand command = _contextWindow.CreateCommand();
                    if (command.FileName == "" || command.Name == "")
                    {
                        // do not allow empty names
                        // do not allow empty file names as they throw InvalidOperationException in process creation
                        return;
                    }

                    _savedCommands.Add(command);
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
