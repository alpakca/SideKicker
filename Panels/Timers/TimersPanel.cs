using SideKicker.PanelSystem;
using SideKicker.PanelSystem.Message;
using System.Collections.Generic;

using TimeInfoSerializer = SideKicker.Panels.PanelContentSerializer<SideKicker.Panels.Timers.ITimeInfo>;

namespace SideKicker.Panels.Timers
{
    internal class TimersPanel : ContextPanel
    {
        static TimersPanel()
        {
            TimeInfoSerializer.SaveFileName = "timeinfos.json";
        }

        private const int TimersListDirtyCheckTime = 1_200_000; // milliseconds, equals 20 minutes
        private const string TimerGoingOffMessage = "Timer goes off.";

        private readonly ContextWindow _contextWindow;
        private readonly List<ITimeInfo> _registeredTimers;
        private readonly IComparer<ITimeInfo> _timeInfoComparer;

        private bool _timersListDirty;
        private float _elapsedSinceLastDirtyCheck;

        private AlarmInfo _shownAlarm;
        private TimerInfo _shownTimer;

        public override ContextPanelType Type => ContextPanelType.Timers;
        public override MessageSupporterWindow Window => _contextWindow;

        public TimersPanel()
        {
            _registeredTimers = new List<ITimeInfo>(TimeInfoSerializer.Load());
            _timeInfoComparer = new TimeInfoComparer();

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

            _elapsedSinceLastDirtyCheck += TimeManager.DeltaTime;
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
            _contextWindow.OnExiting();
            if (_timersListDirty)
            {
                _timersListDirty = false;
                TimeInfoSerializer.Save(_registeredTimers);
            }
        }

        private void CheckAlarmTrigger()
        {
            if (_registeredTimers.Count == 0) return;

            ITimeInfo first = _registeredTimers.Count > 0 ? _registeredTimers[0] : null;
            if (first != null && first.HasElapsed)
            {
                if (first.IsRepeating)
                {
                    _registeredTimers.Sort(_timeInfoComparer);

                    TriggerTimeInfo(first);
                    return;
                }

                RemoveTimeInfo(first);
                if (_shownTimer == first)
                {
                    ShowTimer(null);
                }

                _contextWindow.SetTimesListCount(_registeredTimers.Count);
                TriggerTimeInfo(first);
            }
        }

        private void TriggerTimeInfo(ITimeInfo info)
        {
            RequestTriggeredWindow();

            if (info.LabelText.Length != 0)
                _contextWindow.ShowMessageWindow(MessageWindow.Create(TimerGoingOffMessage + $"\n\n{info.LabelText}", MessageWindowButton.Ok));

            else
                _contextWindow.ShowMessageWindow(MessageWindow.Create(TimerGoingOffMessage, MessageWindowButton.Ok));
        }

        private void Window_TimesListButtonEditPressed(int buttonIndex)
        {
            ITimeInfo info = _registeredTimers[buttonIndex];
            if (info is AlarmInfo alarm)
            {
                ShowAlarm(alarm);
            }
            else if (info is TimerInfo timer)
            {
                ShowTimer(timer);
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

                    TimerInfo info = new TimerInfo();
                    _contextWindow.UpdateTimerWithUI(info);
                    AddTimeInfo(info);

                    ShowTimer(null);
                    _contextWindow.SetTimesListCount(_registeredTimers.Count);

                    break;

                case TimerButtonType.Discard:

                    if (_shownTimer == null) return;

                    RemoveTimeInfo(_shownTimer);
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
                    _contextWindow.UpdateTimerWithUI(_shownTimer);
                    _registeredTimers.Sort(_timeInfoComparer);
                    _timersListDirty = true;

                    ShowTimer(null);

                    break;
            }
        }

        private void Window_AlarmButtonPressed(AlarmButtonType buttonType)
        {
            switch (buttonType)
            {
                case AlarmButtonType.Add:

                    if (_contextWindow.IsSelectedAlarmAlreadyPassed())
                    {
                        return;
                    }

                    AlarmInfo info = new AlarmInfo();
                    _contextWindow.UpdateAlarmWithUI(info);

                    AddTimeInfo(info);

                    break;

                case AlarmButtonType.Delete:

                    if (_shownAlarm == null) return;

                    RemoveTimeInfo(_shownAlarm);

                    break;

                case AlarmButtonType.Save:

                    if (_shownAlarm == null) return;

                    _contextWindow.UpdateAlarmWithUI(_shownAlarm);
                    _registeredTimers.Sort(_timeInfoComparer);
                    _timersListDirty = true;

                    break;
            }

            ShowAlarm(null);
            _contextWindow.SetTimesListCount(_registeredTimers.Count);
        }

        private void ShowAlarm(AlarmInfo info)
        {
            _shownAlarm = info;

            if (info == null)
                _contextWindow.ResetAlarm();
            else
                _contextWindow.InspectAlarm(info);
        }

        private void ShowTimer(TimerInfo info)
        {
            _shownTimer = info;

            if (info == null)
                _contextWindow.ResetTimer();
            else
                _contextWindow.InspectTimer(info);
        }

        private void AddTimeInfo(ITimeInfo info)
        {
            _registeredTimers.Add(info);
            _registeredTimers.Sort(_timeInfoComparer);
            _timersListDirty = true;
        }

        private bool RemoveTimeInfo(ITimeInfo info)
        {
            if (_registeredTimers.Remove(info))
            {
                _timersListDirty = true;
                return true;
            }

            return false;
        }
    }
}
