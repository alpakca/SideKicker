using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using SideKicker.Theming;

namespace SideKicker.Panels.Timers
{
    internal class ContextWindow : PopWindow
    {
        private readonly ScrollableNumberSelector _selector;
        private readonly CalendarShowcase _calendaryShowcase;

        public ContextWindow()
        {
            _calendaryShowcase = new CalendarShowcase { Location = new Point(20, 20), ButtonHeight = 30, ButtonMargin = 8 };
            _selector = new ScrollableNumberSelector(0, 23)
            {
                DistanceToChangeNumber = FontService.Regular.GetLineSpacing(16) + 4,
                Boundaries = new Rectangle(200, 200, 40, 200)
            };

            this.Elements.Add(_selector);
            this.Boundaries = new Rectangle(0, 0, 400, 400);
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            _calendaryShowcase.MouseStateChangeUpdate(e);
            base.OnMouseButtonStateChange(e);
        }
    }
}
