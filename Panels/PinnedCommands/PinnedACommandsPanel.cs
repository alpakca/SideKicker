using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.PinnedCommands
{
    internal class PinnedACommandsPanel : ContextPanel
    {
        public override string IconId => "PinnedActions";
        public override string IconPath => @"panels\PinnedActionsPanelIcon.png";

        public override PopWindow Window { get; } = new ContextWindow();
    }
}
