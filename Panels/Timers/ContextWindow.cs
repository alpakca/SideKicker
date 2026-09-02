using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Surface;
using SideKicker.Theming;
using System;
using System.Collections.Generic;

namespace SideKicker.Panels.Timers
{
    internal class ContextWindow : PopWindow
    {
        private const int TextBoxTextSize = 12;
        private const int NumberSelectorTextSize = 16;
        private const int HeaderTextSize = 20;

        // alarm UI elements
        private bool _isEditingAlarm;
        private readonly MultiLineTextBox _alarmLabelTextbox;
        private readonly CalendarShowcase _alarmCalendar;
        private readonly OrderedDictionary<NumberSelectorType, ScrollableNumberSelector> _alarmNumberSelectors;
        private readonly Dictionary<AlarmButtonType, TextedButton> _alarmButtons;
        private readonly IconedButton[] _alarmMonthChangeButtons;

        // timer UI elements
        private bool _isTimerStopped;
        private TimeInfo? _inspectingTimer;
        private readonly MultiLineTextBox _timerLabelTextbox;
        private readonly OrderedDictionary<NumberSelectorType, ScrollableNumberSelector> _timerNumberSelectors;
        private readonly Dictionary<TimerButtonType, IconedButton> _timerButtons;

        private readonly TextPlaque[] _textPlaques;
        private readonly ScrollableButtonList _activeTimesList;

        private readonly ContextWindowRenderer _renderer;
        private readonly IEnumerable<TimeInfo> _registedTimes;

        public event Action<AlarmButtonType> AlarmButtonPressed;
        public event Action<TimerButtonType> TimerButtonPressed;
        public event Action<int> TimesListButtonEditPressed;
        public event Action UpdateCall;

        public ContextWindow(IEnumerable<TimeInfo> registeredTimes)
        {
            _registedTimes = registeredTimes;

            _alarmLabelTextbox = new MultiLineTextBox();
            _alarmCalendar = new CalendarShowcase();
            _alarmNumberSelectors = new OrderedDictionary<NumberSelectorType, ScrollableNumberSelector>();
            _alarmButtons = new Dictionary<AlarmButtonType, TextedButton>();
            _alarmMonthChangeButtons = new IconedButton[2];
            InitializeAlarmUI();

            _timerLabelTextbox = new MultiLineTextBox();
            _timerNumberSelectors = new OrderedDictionary<NumberSelectorType, ScrollableNumberSelector>();
            _timerButtons = new Dictionary<TimerButtonType, IconedButton>();
            InitializeTimerUI();

            _textPlaques = new TextPlaque[]
            {
                new TextPlaque { IsBold = false, Text = "Set Alarm", TextSize = HeaderTextSize, Position = new Vector2(20, 10) },
                new TextPlaque { IsBold = false, Text = "Set Timer", TextSize = HeaderTextSize, Position = new Vector2(260, 10)}
            };

            _activeTimesList = new ScrollableButtonList
            {
                Boundaries = new Rectangle(),
                ButtonHeight = 0,
                ButtonMargin = 0
            };
            _activeTimesList.ButtonPressed += ActiveTimesList_ButtonPressed;
            _activeTimesList.ButtonHeld += ActiveTimesList_ButtonHeld;

            this.Elements.AddRange([_alarmLabelTextbox, .. _alarmNumberSelectors.Values, .. _alarmButtons.Values, .. _alarmMonthChangeButtons, 
                                    _timerLabelTextbox, .. _timerNumberSelectors.Values, .. _timerButtons.Values]);

            this.Boundaries = new Rectangle(0, 0, 450, 450);
            _renderer = new ContextWindowRenderer(WindowTexture);
        }

        public void SetTimesListCount(int value)
        {
            _activeTimesList.ButtonCount = value;
            _activeTimesList.UpdateScrolledLength();
        }

        public void SetShownTimerStatus(bool isStopped)
        {
            if (isStopped)
            {
                _timerButtons[TimerButtonType.Continue].Enabled = true;
                _timerButtons[TimerButtonType.Stop].Enabled = false;
            }
            else
            {
                _timerButtons[TimerButtonType.Continue].Enabled = false;
                _timerButtons[TimerButtonType.Stop].Enabled = true;
            }

            _isTimerStopped = isStopped;
        }

        public void ShowAlarm(TimeInfo? info)
        {
            if (info == null)
            {
                DateTime now = DateTime.Now;

                _alarmLabelTextbox.SetText("");
                _alarmCalendar.SetDate(now.Year, now.Month, now.Day);
                _alarmNumberSelectors[NumberSelectorType.Hour].SetNumber(now.Hour);
                _alarmNumberSelectors[NumberSelectorType.Minute].SetNumber(now.Minute);

                _alarmButtons[AlarmButtonType.Delete].Enabled = false;
                _alarmButtons[AlarmButtonType.Save].Enabled = false;
                _alarmButtons[AlarmButtonType.Add].Enabled = true;
            }
            else
            {
                _alarmLabelTextbox.SetText(info.LabelText);

                DateTime due = info.DueTime;
                _alarmCalendar.SetDate(due.Year, due.Month, due.Day);
                _alarmNumberSelectors[NumberSelectorType.Hour].SetNumber(due.Hour);
                _alarmNumberSelectors[NumberSelectorType.Minute].SetNumber(due.Minute);

                _alarmButtons[AlarmButtonType.Add].Enabled = false;
                _alarmButtons[AlarmButtonType.Delete].Enabled = true;
                _alarmButtons[AlarmButtonType.Save].Enabled = true;
            }
        }

        public TimeInfo GetAlarm()
        {
            return new TimeInfo
            {
                IsAlarm = true,
                LabelText = _alarmLabelTextbox.Text,
                DueTime = new DateTime(_alarmCalendar.SelectedYear, _alarmCalendar.SelectedMonth, _alarmCalendar.SelectedDay,
                                       _alarmNumberSelectors[NumberSelectorType.Hour].CurrentNumber,
                                       _alarmNumberSelectors[NumberSelectorType.Minute].CurrentNumber,
                                       second: 0)
            };
        }

        public void ShowTimer(TimeInfo? info)
        {
            if (info == null)
            {
                _timerLabelTextbox.SetText("");
                _timerNumberSelectors[NumberSelectorType.Hour].SetNumber(0);
                _timerNumberSelectors[NumberSelectorType.Minute].SetNumber(0);
                _timerNumberSelectors[NumberSelectorType.Seconds].SetNumber(0);

                _timerButtons[TimerButtonType.Discard].Enabled = false;
                _timerButtons[TimerButtonType.Stop].Enabled = false;
                _timerButtons[TimerButtonType.Continue].Enabled = false;
                _timerButtons[TimerButtonType.Add].Enabled = true;
            }
            else
            {
                _timerLabelTextbox.SetText(info.LabelText);

                DateTime remaining = new DateTime((info.DueTime - DateTime.Now).Ticks);
                _timerNumberSelectors[NumberSelectorType.Hour].SetNumber(remaining.Hour);
                _timerNumberSelectors[NumberSelectorType.Minute].SetNumber(remaining.Minute);
                _timerNumberSelectors[NumberSelectorType.Seconds].SetNumber(remaining.Second);

                _timerButtons[TimerButtonType.Discard].Enabled = true;
                _timerButtons[_isTimerStopped ? TimerButtonType.Stop : TimerButtonType.Continue].Enabled = true;
                _timerButtons[TimerButtonType.Add].Enabled = false;
            }

            _inspectingTimer = info;
        }

        public TimeInfo GetTimer()
        {
            DateTime now = DateTime.Now;
            return new TimeInfo
            {
                IsAlarm = false,
                LabelText = _timerLabelTextbox.Text,
                DueTime = now.Add(new TimeSpan(_timerNumberSelectors[NumberSelectorType.Hour].CurrentNumber, 
                                               _timerNumberSelectors[NumberSelectorType.Minute].CurrentNumber, 
                                               _timerNumberSelectors[NumberSelectorType.Seconds].CurrentNumber))
            };
        }

        private void ActiveTimesList_ButtonHeld(int buttonIndex)
        {

        }

        private void ActiveTimesList_ButtonPressed(int buttonIndex)
        {

        }

        protected override void Update()
        {
            UpdateCall?.Invoke();

            if (_inspectingTimer != null)
            {
                if (_isTimerStopped)
                {
                    return;
                }

                // update the selectors to left time of inspecting timer
                DateTime remaining = new DateTime((_inspectingTimer.DueTime - DateTime.Now).Ticks);
                _timerNumberSelectors[NumberSelectorType.Hour].SetNumber(remaining.Hour);
                _timerNumberSelectors[NumberSelectorType.Minute].SetNumber(remaining.Minute);
                _timerNumberSelectors[NumberSelectorType.Seconds].SetNumber(remaining.Second);
            }

            base.Update();
        }

        protected override void RenderUpdate()
        {
            _renderer.StartMainBatch();

            _renderer.RenderMainElements(_textPlaques, _alarmCalendar);

            _renderer.RenderIconedButtons(_alarmMonthChangeButtons);
            if (_isEditingAlarm)
            {
                _renderer.RenderTextedButtons(_alarmButtons[AlarmButtonType.Delete], _alarmButtons[AlarmButtonType.Save]);
            }
            else
            {
                _renderer.RenderTextedButtons(_alarmButtons[AlarmButtonType.Add]);
            }

            if (_inspectingTimer != null)
            {
                _renderer.RenderIconedButtons(_timerButtons[TimerButtonType.Discard], _isTimerStopped ? _timerButtons[TimerButtonType.Continue] 
                                                                                                      : _timerButtons[TimerButtonType.Stop]);
            }
            else
            {
                _renderer.RenderIconedButtons(_timerButtons[TimerButtonType.Add]);
            }

            _renderer.RenderNumberSelectors(_alarmNumberSelectors.Values);
            _renderer.RenderNumberSelectors(_timerNumberSelectors.Values);

            _renderer.EndMainBatch();

            _renderer.RenderTextBox(_alarmLabelTextbox);
            _renderer.RenderTextBox(_timerLabelTextbox);

            _renderer.RenderTimesListBackgrounds(_activeTimesList);
            int i = 0;
            foreach (var timeInfo in _registedTimes)
            {
                if (timeInfo.IsAlarm)
                {
                    DateTime due = timeInfo.DueTime;
                    _renderer.RenderTimesListText(_activeTimesList, i, $"{due.Day} {CalendarShowcase.MonthNames[due.Month]}, {due.Year}.");
                }
                else
                {
                    DateTime remaining = new DateTime(timeInfo.DueTime.Ticks - DateTime.Now.Ticks);
                    if (remaining.Hour != 0)
                    {
                        _renderer.RenderTimesListText(_activeTimesList, i, $"Timer ends in: {remaining.Hour}h, {remaining.Minute}min, {remaining.Second}s.");
                    }
                    else if (remaining.Minute != 0)
                    {
                        _renderer.RenderTimesListText(_activeTimesList, i, $"Timer ends in: {remaining.Minute}min, {remaining.Second}s.");
                    }
                    else
                    {
                        _renderer.RenderTimesListText(_activeTimesList, i, $"Timer ends in: {remaining.Second}s.");
                    }
                }

                i++;
            }

            base.RenderUpdate();
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            _alarmCalendar.MouseStateChangeUpdate(e);
            base.OnMouseButtonStateChange(e);
        }

        private void InitializeAlarmUI()
        {
            _alarmLabelTextbox.TextSize = TextBoxTextSize;
            _alarmLabelTextbox.Boundaries = new Rectangle(90, 305, 150, 70);
            _alarmLabelTextbox.SelectableMargin = 4;

            _alarmCalendar.ButtonHeight = TimesListLayoutCalculator.CalendarDayButtonHeight;
            _alarmCalendar.ButtonMargin = TimesListLayoutCalculator.CalendarDayButtonMargin;
            _alarmCalendar.Location = new Point(20, 100);

            int numberSelectingDistance = FontService.Regular.GetLineSpacing(NumberSelectorTextSize) + 4;
            _alarmNumberSelectors[NumberSelectorType.Hour] = new ScrollableNumberSelector(0, 23)
            {
                DistanceToChangeNumber = numberSelectingDistance,
                Boundaries = new Rectangle(20, 301, 20, 78)
            };
            _alarmNumberSelectors[NumberSelectorType.Minute] = new ScrollableNumberSelector(0, 59)
            {
                DistanceToChangeNumber = numberSelectingDistance,
                Boundaries = new Rectangle(50, 301, 20, 78)
            };

            SelectablePacket selectPacket = new SelectablePacket();
            HoldablePacket holdPacket = new HoldablePacket();
            _alarmButtons[AlarmButtonType.Add] = new TextedButton
            {
                Text = "Add",
                Boundaries = new Rectangle(164, 385, 80, 30),
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Add)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _alarmButtons[AlarmButtonType.Delete] = new TextedButton
            {
                Text = "Delete",
                Boundaries = new Rectangle(86, 385, 70, 30),
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Delete)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _alarmButtons[AlarmButtonType.Save] = new TextedButton
            {
                Text = "Save",
                Boundaries = new Rectangle(174, 385, 70, 30),
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Save)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };

            _alarmMonthChangeButtons[0] = new IconedButton
            {
                IconId = ContextWindowRenderer.AlarmCalendarPreviousMonthButtonId,
                Boundaries = new Rectangle(175, 45, TimesListLayoutCalculator.CalendarDayButtonHeight, TimesListLayoutCalculator.CalendarDayButtonHeight),
                ClickCommand = new ActionCommander<Clickable>(_ => _alarmCalendar.MoveMonth(-1)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _alarmMonthChangeButtons[1] = new IconedButton
            {
                IconId = ContextWindowRenderer.AlarmCalendarNextMonthButtonId,
                Boundaries = new Rectangle(210, 45, TimesListLayoutCalculator.CalendarDayButtonHeight, TimesListLayoutCalculator.CalendarDayButtonHeight),
                ClickCommand = new ActionCommander<Clickable>(_ => _alarmCalendar.MoveMonth(+1)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
        }

        private void InitializeTimerUI()
        {
            _timerLabelTextbox.TextSize = TextBoxTextSize;
            _timerLabelTextbox.Boundaries = new Rectangle(270, 140, 160, 28);
            _timerLabelTextbox.SelectableMargin = 4;

            _timerNumberSelectors[NumberSelectorType.Hour] = new ScrollableNumberSelector(0, 23)
            {
                Boundaries = new Rectangle(315, 45, 20, 78)
            };
            _timerNumberSelectors[NumberSelectorType.Minute] = new ScrollableNumberSelector(0, 59)
            {
                Boundaries = new Rectangle(345, 45, 20, 78)
            };
            _timerNumberSelectors[NumberSelectorType.Seconds] = new ScrollableNumberSelector(0, 59)
            {
                Boundaries = new Rectangle(375, 45, 20, 78)
            };

            SelectablePacket selectPacket = new SelectablePacket();
            HoldablePacket holdPacket = new HoldablePacket();
            _timerButtons[TimerButtonType.Add] = new IconedButton
            {
                IconId = ContextWindowRenderer.TimerAddButton,
                Boundaries = new Rectangle(338, 178, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Add)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _timerButtons[TimerButtonType.Discard] = new IconedButton
            {
                IconId = ContextWindowRenderer.TimerDiscardButton,
                Boundaries = new Rectangle(317, 178, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Discard)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _timerButtons[TimerButtonType.Stop] = new IconedButton
            {
                IconId = ContextWindowRenderer.TimerStopButton,
                Boundaries = new Rectangle(359, 178, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Stop)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _timerButtons[TimerButtonType.Continue] = new IconedButton
            {
                IconId = ContextWindowRenderer.TimerAddButton,
                Boundaries = new Rectangle(359, 178, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Continue)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
        }
    }
}
