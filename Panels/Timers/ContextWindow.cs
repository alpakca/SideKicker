using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;

namespace SideKicker.Panels.Timers
{
    internal class ContextWindow : PopWindow
    {
        private readonly CalendarShowcase _calendaryShowcase;

        public ContextWindow()
        {
            _calendaryShowcase = new CalendarShowcase { Location = new Point(20, 20), ButtonHeight = 30, ButtonMargin = 8 };

            this.Boundaries = new Rectangle(0, 0, 400, 400);
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            _calendaryShowcase.MouseStateChangeUpdate(e);
            base.OnMouseButtonStateChange(e);
        }
    }
}
