using PopUpUI.Surface;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.Timers
{
    internal class TimersPanel : ContextPanel
    {
        private readonly ContextWindow _window;
        private readonly SortedSet<TimeInfo> _registeredTimers;

        public override string IconId { get; } = "Timers";
        public override string IconPath { get; } = @"panels\TimersPanelIcon.png";

        public override PopWindow Window => _window;

        public TimersPanel()
        {
            _window = new ContextWindow();
            _registeredTimers = new SortedSet<TimeInfo>(Comparer<TimeInfo>.Create((f, s) => DateTime.Compare(f.DueTime, s.DueTime)));

            _window.AlarmButtonPressed += Window_AlarmButtonPressed;
            _window.TimerButtonPressed += Window_TimerButtonPressed;
            _window.TimesListButtonBodyPressed += Window_TimesListButtonBodyPressed;
            _window.TimesListButtonEditPressed += Window_TimesListButtonEditPressed;
        }

        private void Window_TimesListButtonEditPressed(int buttonIndex)
        {

        }

        private void Window_TimesListButtonBodyPressed(int buttonIndex)
        {

        }

        private void Window_TimerButtonPressed(ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.Start:
                    break;

                case ButtonType.Stop:
                    break;
            }
        }

        private void Window_AlarmButtonPressed(ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.Add:
                    break;

                case ButtonType.Delete:
                    break;

                case ButtonType.Save:
                    break;
            }
        }

        public override void OnExiting()
        {

        }
    }
}
