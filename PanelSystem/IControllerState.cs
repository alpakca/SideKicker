using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.PanelSystem
{
    internal interface IControllerState
    {
        public bool IsOpen { get; }
        public bool IsOpenByHover { get; }
        public bool IsTriggered { get; }

        public Rectangle MinBoundaryBox { get; }

        public void SetWindowState(bool isOpen, bool openedByHover = true);
        public void SetSidePanelBounds(Rectangle value);
        public void SetShownPanelBounds(Rectangle value);
    }
}
