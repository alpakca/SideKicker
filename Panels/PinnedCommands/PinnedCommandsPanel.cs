using PopUpUI.Toolkit;
using SideKicker.PanelSystem.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using CommandSerializer = SideKicker.Panels.PanelContentSerializer<SideKicker.Panels.PinnedCommands.SavedCommand>;

namespace SideKicker.Panels.PinnedCommands
{
    internal class PinnedCommandsPanel : ContextPanel
    {
        static PinnedCommandsPanel()
        {
            CommandSerializer.SaveFileName = "pinnedcommands.json";
        }

        private const int SavedCommandsListDirtyCheckTime = 1_200_000; // in milliseconds, equals 20 minutes

        private bool _commandListDirty;
        private float _elapsedSinceLastDirtyCheck;
        private SavedCommand _editorShownCommand;

        private readonly ContextWindow _contextWindow;
        private readonly List<SavedCommand> _savedCommands;

        public override string IconId => PanelIconValues.PinnedCommands_IconId;
        public override string IconPath => PanelIconValues.PinnedCommands_IconPath;
        public override ContextPanelType Type => ContextPanelType.PinnedCommands;
        public override MessageSupporterWindow Window => _contextWindow;

        public PinnedCommandsPanel()
        {
            _savedCommands = new List<SavedCommand>(CommandSerializer.Load());
            _contextWindow = new ContextWindow(_savedCommands);
            _contextWindow.SetSavedCommandCount(_savedCommands.Count);

            _contextWindow.EditorButtonPressed += Window_EditorButtonPressed;
            _contextWindow.CommandListButtonBodyPressed += Window_CommandListButtonBodyPressed;
            _contextWindow.CommandListButtonEditPressed += Window_CommandListButtonEditPressed;
            _contextWindow.CommandListButtonsSwaped += Window_CommandListButtonsSwaped;

            _editorShownCommand = null;
            _contextWindow.ShowCommand(null);
        }

        public override void Update()
        {
            _elapsedSinceLastDirtyCheck += Time.DeltaTime;
            if (_elapsedSinceLastDirtyCheck >= SavedCommandsListDirtyCheckTime)
            {
                _elapsedSinceLastDirtyCheck = 0;

                if (_commandListDirty)
                {
                    _commandListDirty = false;
                    _ = CommandSerializer.SaveAsync(_savedCommands);
                }
            }
        }

        public override void OnExiting()
        {
            if (_commandListDirty)
            {
                _commandListDirty = false;
                CommandSerializer.Save(_savedCommands);
            }
        }

        private void Window_CommandListButtonBodyPressed(int buttonIndex)
        {
            SavedCommand command = _savedCommands[buttonIndex];
            try
            {
                using Process proc = Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = command.PathToRunCommand,
                    FileName = command.FileName,
                    UseShellExecute = true,
                });
            }
            catch (Win32Exception)
            {
                _contextWindow.ShowMessageWindow(MessageWindow.Create("Couldn't start the file at given path.", MessageWindowButton.Ok));
            }
            catch (InvalidOperationException)
            {
                _contextWindow.ShowMessageWindow(MessageWindow.Create("Couldn't start the file at given path.", MessageWindowButton.Ok));
            }
        }

        private void Window_CommandListButtonEditPressed(int buttonIndex)
        {
            SetEditorShowCommand(_savedCommands[buttonIndex]);
        }

        private void Window_CommandListButtonsSwaped(int firstButtonIndex, int secondButtonIndex)
        {
            (_savedCommands[firstButtonIndex], _savedCommands[secondButtonIndex]) = (_savedCommands[secondButtonIndex], _savedCommands[firstButtonIndex]);
            _commandListDirty = true;
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
                        _contextWindow.ShowMessageWindow(MessageWindow.Create("File path and visible name can't be empty.", MessageWindowButton.Ok));
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

            _contextWindow.SetSavedCommandCount(_savedCommands.Count);
            _commandListDirty = true;
        }

        private void SetEditorShowCommand(SavedCommand command)
        {
            _editorShownCommand = command;
            _contextWindow.ShowCommand(command);
        }
    }
}
