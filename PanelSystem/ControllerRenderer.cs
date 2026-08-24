using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal class ControllerRenderer
    {
        private readonly ControllerLayoutCalculator _layoutCalculator;

        public ControllerRenderer(ControllerLayoutCalculator layoutCalculator)
        {
            _layoutCalculator = layoutCalculator;
        }
    }
}
