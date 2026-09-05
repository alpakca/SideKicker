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
        public abstract string IconPath { get; }

        public abstract PopWindow Window { get; }

        public abstract void Update();
        public abstract void OnExiting();
    }
}
