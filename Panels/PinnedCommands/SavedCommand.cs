using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.PinnedCommands
{
    internal class SavedCommand
    {
        public string PathToRunCommand { get; init; }
        public string Command { get; init; }
        public string Name { get; init; }
    }
}
