using PopUpUI.Elements;
using SideKicker.Panels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal class PanelToggle : ToggleButton
    {
        public ContextPanel Panel { get; init; }
    }
}
