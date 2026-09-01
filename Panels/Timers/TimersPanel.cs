using PopUpUI.Surface;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.Timers
{
    internal class TimersPanel : ContextPanel
    {
        private readonly ContextWindow _contextWindow;
        private readonly SortedList<DateTime, TimeInfo> _registeredTimers;

        private TimeInfo _shownAlarm;
        private TimeInfo _shownTimer;

        public override string IconId { get; } = "Timers";
        public override string IconPath { get; } = @"panels\TimersPanelIcon.png";

        public override PopWindow Window => _contextWindow;

        public TimersPanel()
        {
            _registeredTimers = new SortedList<DateTime, TimeInfo>();
            _contextWindow = new ContextWindow(_registeredTimers.Values);

            _contextWindow.AlarmButtonPressed += Window_AlarmButtonPressed;
            _contextWindow.TimerButtonPressed += Window_TimerButtonPressed;
            _contextWindow.TimesListButtonEditPressed += Window_TimesListButtonEditPressed;
            _contextWindow.UpdateCall += ContextWindow_UpdateCall;

            ShowAlarm(null);
            ShowTimer(null);
        }

        private void ContextWindow_UpdateCall()
        {
            if (_registeredTimers.Count == 0) return;

            TimeInfo first = _registeredTimers[_registeredTimers.Keys[0]];
            if (first.DueTime <= DateTime.Now)
            {
                _registeredTimers.Remove(first.DueTime);
                TriggerTimeInfo(first);
            }
        }

        private void TriggerTimeInfo(TimeInfo info)
        {
            // trigger window to show the notification
            // show the label of the info
        }

        private void Window_TimesListButtonEditPressed(int buttonIndex)
        {
            TimeInfo info = _registeredTimers.GetValueAtIndex(buttonIndex);
            if (info.IsAlarm)
            {
                ShowAlarm(info);
            }
            else
            {
                ShowTimer(info);
                _contextWindow.SetShownTimerStatus(true);
            }
        }

        private void Window_TimerButtonPressed(TimerButtonType buttonType)
        {
            switch (buttonType)
            {
                case TimerButtonType.Add:

                    TimeInfo info = _contextWindow.GetTimer();
                    if (info.DueTime <= DateTime.Now)
                    {
                        return;
                    }

                    _registeredTimers.Add(info.DueTime, info);

                    ShowTimer(info);
                    _contextWindow.SetShownTimerStatus(false);
                    _contextWindow.SetTimesListCount(_registeredTimers.Count);

                    break;

                case TimerButtonType.Discard:

                    if (!_registeredTimers.Remove(_shownTimer.DueTime))
                    {
                        return;
                    }

                    ShowTimer(null);
                    _contextWindow.SetShownTimerStatus(false);
                    _contextWindow.SetTimesListCount(_registeredTimers.Count);

                    break;

                case TimerButtonType.Stop:

                    if (_shownTimer == null) return;
                    _contextWindow.SetShownTimerStatus(true);

                    break;

                case TimerButtonType.Continue:

                    if (_shownTimer == null) return;
                    _contextWindow.SetShownTimerStatus(false);

                    break;
            }
        }

        private void Window_AlarmButtonPressed(AlarmButtonType buttonType)
        {
            switch (buttonType)
            {
                case AlarmButtonType.Add:

                    TimeInfo info = _contextWindow.GetAlarm();
                    _registeredTimers.Add(info.DueTime, info);

                    break;

                case AlarmButtonType.Delete:

                    if (!_registeredTimers.Remove(_shownAlarm.DueTime))
                    {
                        return;
                    }

                    break;

                case AlarmButtonType.Save:

                    if (!_registeredTimers.Remove(_shownAlarm.DueTime))
                    {
                        return;
                    }

                    TimeInfo newAlarm = _contextWindow.GetAlarm();
                    _registeredTimers.Add(newAlarm.DueTime, newAlarm);

                    break;
            }

            ShowAlarm(null);
            _contextWindow.SetTimesListCount(_registeredTimers.Count);
        }

        private void ShowAlarm(TimeInfo info)
        {
            _shownAlarm = info;
            _contextWindow.ShowAlarm(info);
        }

        private void ShowTimer(TimeInfo info)
        {
            _shownTimer = info;
            _contextWindow.ShowTimer(info);
        }

        public override void OnExiting()
        {

        }
    }
}
