using PopUpUI.Surface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SideKicker.Panels.Timers
{
    internal class TimersPanel : ContextPanel
    {
        private readonly ContextWindow _contextWindow;
        private readonly List<TimeInfo> _registeredTimers;
        private readonly Comparison<TimeInfo> _timerComparison;

        private TimeInfo _shownAlarm;
        private TimeInfo _shownTimer;

        public override string IconId { get; } = "Timers";
        public override string IconPath { get; } = @"panels\TimersPanelIcon.png";

        public override PopWindow Window => _contextWindow;

        public TimersPanel()
        {
            _registeredTimers = new List<TimeInfo>();
            _timerComparison = (f, s) => f.DueTime.CompareTo(s.DueTime);
            _contextWindow = new ContextWindow(_registeredTimers);

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

            TimeInfo? first = _registeredTimers.Count > 0 ? _registeredTimers[0] : null;
            if (first?.DueTime <= DateTime.Now)
            {
                RemoveTimeInfo(first);
                if (_shownTimer == first)
                {
                    ShowTimer(null);
                    _contextWindow.SetShownTimerStatus(false);
                }

                _contextWindow.SetTimesListCount(_registeredTimers.Count);
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
            TimeInfo info = _registeredTimers[buttonIndex];
            if (info.IsAlarm)
            {
                ShowAlarm(info);
            }
            else
            {
                ShowTimer(info);
                _contextWindow.SetShownTimerStatus(false);
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

                    AddTimeInfo(info);

                    ShowTimer(null);
                    _contextWindow.SetShownTimerStatus(false);
                    _contextWindow.SetTimesListCount(_registeredTimers.Count);

                    break;

                case TimerButtonType.Discard:

                    if (!_registeredTimers.Remove(_shownTimer))
                    {
                        return;
                    }

                    ShowTimer(null);
                    _contextWindow.SetShownTimerStatus(false);
                    _contextWindow.SetTimesListCount(_registeredTimers.Count);

                    break;

                case TimerButtonType.Stop:

                    if (_shownTimer == null) return;

                    _shownTimer.Pause();
                    _contextWindow.SetShownTimerStatus(true);

                    break;

                case TimerButtonType.Continue:

                    if (_shownTimer == null) return;

                    // replace old timer with new one
                    RemoveTimeInfo(_shownTimer);
                    _contextWindow.SetShownTimerStatus(false);

                    TimeInfo edited = _contextWindow.GetTimer();

                    if (edited.DueTime <= DateTime.Now)
                    {
                        ShowTimer(null);
                        _contextWindow.SetTimesListCount(_registeredTimers.Count);
                        return;
                    }

                    AddTimeInfo(edited);
                    ShowTimer(edited);
                    // don't need to update times list count, only replaced info

                    break;

                case TimerButtonType.Save:

                    if (_shownTimer == null) return;

                    // replace old timer with new one, quit inspecting
                    RemoveTimeInfo(_shownTimer);
                    TimeInfo saved = _contextWindow.GetTimer();

                    ShowTimer(null);
                    _contextWindow.SetShownTimerStatus(false);

                    if (saved.DueTime > DateTime.Now)
                    {
                        AddTimeInfo(saved);
                        return;
                    }

                    _contextWindow.SetTimesListCount(_registeredTimers.Count);

                    break;
            }
        }

        private void Window_AlarmButtonPressed(AlarmButtonType buttonType)
        {
            switch (buttonType)
            {
                case AlarmButtonType.Add:

                    TimeInfo info = _contextWindow.GetAlarm();
                    AddTimeInfo(info);

                    break;

                case AlarmButtonType.Delete:

                    if (!RemoveTimeInfo(_shownAlarm))
                    {
                        return;
                    }

                    break;

                case AlarmButtonType.Save:

                    if (!RemoveTimeInfo(_shownAlarm))
                    {
                        return;
                    }

                    TimeInfo newAlarm = _contextWindow.GetAlarm();
                    AddTimeInfo(newAlarm);

                    break;
            }

            ShowAlarm(null);
            _contextWindow.SetTimesListCount(_registeredTimers.Count);
        }

        private void ShowAlarm(TimeInfo info)
        {
            _shownAlarm = info;

            if (info == null)
                _contextWindow.ResetAlarm();
            else
                _contextWindow.InspectAlarm(info);
        }

        private void ShowTimer(TimeInfo info)
        {
            _shownTimer = info;

            if (info == null)
                _contextWindow.ResetTimer();
            else
                _contextWindow.InspectTimer(info);
        }

        private void AddTimeInfo(TimeInfo info)
        {
            _registeredTimers.Add(info);
            _registeredTimers.Sort(_timerComparison);
        }

        private bool RemoveTimeInfo(TimeInfo info)
        {
            if (_registeredTimers.Remove(info))
            {
                _registeredTimers.Sort(_timerComparison);
                return true;
            }

            return false;
        }

        public override void OnExiting()
        {

        }
    }
}
