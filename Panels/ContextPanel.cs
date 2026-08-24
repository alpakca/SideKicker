using Microsoft.Xna.Framework;
using PopUpUI.Surface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels
{
    internal abstract class ContextPanel
    {
        public abstract string IconId { get; }
        public abstract Point Size { get; }
        public abstract PopWindow Window { get; }
    }
}
