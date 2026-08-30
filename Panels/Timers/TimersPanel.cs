using PopUpUI.Surface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.Timers
{
    internal class TimersPanel : ContextPanel
    {
        private readonly ContextWindow _window;

        public override string IconId { get; } = "Timers";
        public override string IconPath { get; } = @"panels\TimersPanelIcon.png";

        public override PopWindow Window => _window;

        public TimersPanel()
        {
            _window = new ContextWindow();
        }

        public override void OnExiting()
        {

        }
    }
}
