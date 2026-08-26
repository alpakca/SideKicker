using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.PinnedCommands
{
    internal class CommandEditor
    {
        private readonly LinearTextBox _nameTextBox;
        private readonly LinearTextBox _pathToRunCommandTextBox;
        private readonly LinearTextBox _commandTextBox;

        public CommandEditor(LinearTextBox nameTextBox, LinearTextBox pathToRunCommandTextBox, LinearTextBox commandTextBox)
        {
            _nameTextBox = nameTextBox;
            _pathToRunCommandTextBox = pathToRunCommandTextBox;
            _commandTextBox = commandTextBox;
        }

        public void ShowEmptyCommand()
        {
            _nameTextBox.Text = "";
            _pathToRunCommandTextBox.Text = @"C:\Users\user\Desktop";
            _commandTextBox.Text = "";
        }

        public void ShowExistingCommand(SavedCommand command)
        {
            _nameTextBox.Text = command.Name;
            _pathToRunCommandTextBox.Text = command.PathToRunCommand;
            _commandTextBox.Text = command.Command;
        }

        public SavedCommand GetCommand()
        {
            SavedCommand command = new SavedCommand
            {
                Name = _nameTextBox.Text,
                PathToRunCommand = _pathToRunCommandTextBox.Text,
                Command = _commandTextBox.Text
            };

            ShowEmptyCommand();
            return command;
        }
    }
}
