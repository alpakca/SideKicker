using PopUpUI.Surface;
using System;

namespace SideKicker.Panels
{
    internal abstract class ContextPanel
    {
        public abstract string IconId { get; }
        public abstract string IconPath { get; }

        public abstract PopWindow Window { get; }

        public event Action<ContextPanel> TriggeredWindowRequested;

        public abstract void Update();
        public abstract void OnExiting();

        protected void RequestTriggeredWindow()
        {
            TriggeredWindowRequested?.Invoke(this);
        }
    }
}
