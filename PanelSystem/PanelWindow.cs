using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal class PanelWindow : PopWindow
    {
        private readonly ContextPanel[] _panels;
        private readonly ControllerLayoutCalculator _layoutCalculator;
        private readonly ControllerRenderer _renderer;

        public PanelWindow(ContextPanel[] panels)
        {
            _panels = panels;
            _layoutCalculator = new ControllerLayoutCalculator();
            _renderer = new ControllerRenderer(_layoutCalculator);
        }
    }
}
