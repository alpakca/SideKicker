using SideKicker.PanelSystem.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels.DesktopSearcher
{
    internal class DesktopSearcherPanel : ContextPanel
    {
        private readonly ContextWindow _window;

        public override string IconId { get; }
        public override string IconPath { get; }

        public override MessageSupporterWindow Window => _window;

        public DesktopSearcherPanel()
        {
            _window = new ContextWindow();
        }

        public override void Update()
        {
        }

        public override void OnExiting()
        {
        }
    }
}
