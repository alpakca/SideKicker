using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;

using TimeInfoSerializer = SideKicker.Panels.PanelContentSerializer<SideKicker.Panels.Timers.TimeInfo>;

namespace SideKicker.Panels.Timers
{
    internal class TimersPanel : ContextPanel
    {
        static TimersPanel()
        {
            TimeInfoSerializer.SaveFileName = "timers.json";
        }

        private const int TimersListDirtyCheckTime = 5000; // milliseconds

        private readonly ContextWindow _contextWindow;
        private readonly List<TimeInfo> _registeredTimers;
        private readonly Comparison<TimeInfo> _timerComparison;

        private bool _timersListDirty;
        private float _elapsedSinceLastDirtyCheck;

        private TimeInfo _shownAlarm;
        private TimeInfo _shownTimer;

        public override string IconId { get; } = "Timers";
        public override string IconPath { get; } = @"panels\TimersPanelIcon.png";

        public override PopWindow Window => _contextWindow;

        public TimersPanel()
        {
            _registeredTimers = new List<TimeInfo>(TimeInfoSerializer.Load());
            _timerComparison = (f, s) => f.DueTime.CompareTo(s.DueTime);

            _contextWindow = new ContextWindow(_registeredTimers);
            _contextWindow.SetTimesListCount(_registeredTimers.Count);

            _contextWindow.AlarmButtonPressed += Window_AlarmButtonPressed;
            _contextWindow.TimerButtonPressed += Window_TimerButtonPressed;
            _contextWindow.TimesListButtonEditPressed += Window_TimesListButtonEditPressed;

            ShowAlarm(null);
            ShowTimer(null);
        }

        public override void Update()
        {
            CheckAlarmTrigger();

            _elapsedSinceLastDirtyCheck += Time.DeltaTime;
            if (_elapsedSinceLastDirtyCheck >= TimersListDirtyCheckTime)
            {
                _elapsedSinceLastDirtyCheck = 0;

                if (_timersListDirty)
                {
                    _timersListDirty = false;
                    _ = TimeInfoSerializer.SaveAsync(_registeredTimers);
                }
            }
        }

        public override void OnExiting()
        {
            if (_timersListDirty)
            {
                _timersListDirty = false;
                TimeInfoSerializer.Save(_registeredTimers);
            }
        }

        private void CheckAlarmTrigger()
        {
            if (_registeredTimers.Count == 0) return;

            TimeInfo first = _registeredTimers.Count > 0 ? _registeredTimers[0] : null;
            if (first != null && first.HasElapsed)
            {
                RemoveTimeInfo(first);
                if (_shownTimer == first)
                {
                    ShowTimer(null);
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
            }
        }

        private void Window_TimerButtonPressed(TimerButtonType buttonType)
        {
            switch (buttonType)
            {
                case TimerButtonType.Add:

                    if (_contextWindow.IsTimerSelectedTimeZero())
                    {
                        return;
                    }

                    TimeInfo info = _contextWindow.GetTimer(DateTime.Now);
                    AddTimeInfo(info);

                    ShowTimer(null);
                    _contextWindow.SetTimesListCount(_registeredTimers.Count);

                    break;

                case TimerButtonType.Discard:

                    if (!RemoveTimeInfo(_shownTimer))
                    {
                        return;
                    }

                    ShowTimer(null);
                    _contextWindow.SetTimesListCount(_registeredTimers.Count);

                    break;

                case TimerButtonType.Stop:

                    if (_shownTimer == null) return;

                    _shownTimer.Pause();
                    ShowTimer(_shownTimer); // re-show timer to update UI buttons as they are stick to TimeInfo.IsRunning, which is changed by .Pause()
                    _timersListDirty = true; // explicit set to true since we need to update list to save timer as paused

                    break;

                case TimerButtonType.Continue:

                    if (_shownTimer == null) return;

                    _shownTimer.Resume();
                    ShowTimer(_shownTimer); // re-show timer to update UI buttons as they are stick to TimeInfo.IsRunning, which is changed by .Resume()
                    _timersListDirty = true; // explicit set to true since we need to update list to save timer as running

                    break;

                case TimerButtonType.Save:

                    if (_shownTimer == null) return;

                    // replace old timer with new one, quit inspecting
                    RemoveTimeInfo(_shownTimer);
                    TimeInfo saved = _contextWindow.GetTimer(DateTime.Now);

                    // TimeInfo's are running by default
                    if (!_shownTimer.IsRunning)
                    {
                        saved.Pause();
                    }

                    AddTimeInfo(saved);
                    ShowTimer(null);

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
            _timersListDirty = true;
        }

        private bool RemoveTimeInfo(TimeInfo info)
        {
            if (_registeredTimers.Remove(info))
            {
                _registeredTimers.Sort(_timerComparison);
                _timersListDirty = true;
                return true;
            }

            return false;
        }
    }
}
