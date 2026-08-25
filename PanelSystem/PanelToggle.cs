using PopUpUI.Elements;
using SideKicker.Panels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal class PanelToggle : ToggleButton
    {
        public int PanelIndex { get; init; }
        public ContextPanel Panel { get; init; }
    }
}
