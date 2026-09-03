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
        private bool _isTimerPaused;
        private TimeInfo? _inspectingTimer;
        private readonly MultiLineTextBox _timerLabelTextbox;
        private readonly OrderedDictionary<NumberSelectorType, ScrollableNumberSelector> _timerNumberSelectors;
        private readonly Dictionary<TimerButtonType, IconedButton> _timerButtons;

        // stopwatch ui
        private readonly StopwatchShowcase _stopWatchShowcase;
        private readonly Dictionary<TimerButtonType, IconedButton> _stopwatchButtons;

        private readonly TextPlaque[] _textPlaques;
        private readonly ScrollableButtonList _timesList;

        private readonly ContextWindowRenderer _renderer;
        private readonly IList<TimeInfo> _registedTimes;

        public event Action<AlarmButtonType> AlarmButtonPressed;
        public event Action<TimerButtonType> TimerButtonPressed;
        public event Action<int> TimesListButtonEditPressed;
        public event Action UpdateCall;

        public ContextWindow(IList<TimeInfo> registeredTimes)
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

            _stopWatchShowcase = new StopwatchShowcase();
            _stopwatchButtons = new Dictionary<TimerButtonType, IconedButton>();
            InitializeStopwatchUI();

            _textPlaques = new TextPlaque[]
            {
                new TextPlaque { IsBold = false, Text = "Set Alarm", TextSize = HeaderTextSize, Position = new Vector2(20, 10) },
                new TextPlaque { IsBold = false, Text = "Set Timer", TextSize = HeaderTextSize, Position = new Vector2(260, 10)}
            };

            _timesList = new ScrollableButtonList
            {
                Boundaries = new Rectangle(260, 301, 178, 114),
                ButtonHeight = TimesListLayoutCalculator.TimesListButtonHeight,
                ButtonMargin = 4
            };
            _timesList.ButtonPressed += ActiveTimesList_ButtonPressed;

            this.Elements.AddRange([_alarmLabelTextbox, .. _alarmNumberSelectors.Values, .. _alarmButtons.Values, .. _alarmMonthChangeButtons, 
                                    _timerLabelTextbox, .. _timerNumberSelectors.Values, .. _timerButtons.Values,
                                                                                         .. _stopwatchButtons.Values]);

            this.Boundaries = new Rectangle(0, 0, 450, 450);
            _renderer = new ContextWindowRenderer(WindowTexture);
        }

        public void SetTimesListCount(int value)
        {
            _timesList.ButtonCount = value;
            _timesList.UpdateScrolledLength();
        }

        public void SetShownTimerStatus(bool isPaused)
        {
            if (isPaused)
            {
                _timerButtons[TimerButtonType.Continue].Enabled = true;
                _timerButtons[TimerButtonType.Stop].Enabled = false;
            }
            else
            {
                _timerButtons[TimerButtonType.Continue].Enabled = false;
                _timerButtons[TimerButtonType.Stop].Enabled = true;
            }

            _isTimerPaused = isPaused;
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
            return new TimeInfo(new DateTime(_alarmCalendar.SelectedYear, _alarmCalendar.SelectedMonth, _alarmCalendar.SelectedDay,
                                              _alarmNumberSelectors[NumberSelectorType.Hour].CurrentNumber,
                                              _alarmNumberSelectors[NumberSelectorType.Minute].CurrentNumber,
                                              second: 0))
            {
                IsAlarm = true,
                LabelText = _alarmLabelTextbox.Text
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

                TimeSpan remaining = info.GetRemainingTime(DateTime.Now);
                _timerNumberSelectors[NumberSelectorType.Hour].SetNumber(remaining.Hours);
                _timerNumberSelectors[NumberSelectorType.Minute].SetNumber(remaining.Minutes);
                _timerNumberSelectors[NumberSelectorType.Seconds].SetNumber(remaining.Seconds);

                _timerButtons[TimerButtonType.Discard].Enabled = true;
                _timerButtons[_isTimerPaused ? TimerButtonType.Stop : TimerButtonType.Continue].Enabled = true;
                _timerButtons[TimerButtonType.Add].Enabled = false;
            }

            _inspectingTimer = info;
        }

        public TimeInfo GetTimer()
        {
            DateTime now = DateTime.Now;
            return new TimeInfo(DateTime.Now.Add(new TimeSpan(_timerNumberSelectors[NumberSelectorType.Hour].CurrentNumber,
                                                              _timerNumberSelectors[NumberSelectorType.Minute].CurrentNumber,
                                                              _timerNumberSelectors[NumberSelectorType.Seconds].CurrentNumber)))
            {
                IsAlarm = false,
                LabelText = _timerLabelTextbox.Text
            };
        }

        private void SetStopwatchStatus(bool isStarted, bool isPaused)
        {
            _stopwatchButtons[TimerButtonType.Add].Enabled = !isStarted;
            _stopwatchButtons[TimerButtonType.Discard].Enabled = isStarted;
            _stopwatchButtons[TimerButtonType.Stop].Enabled = isStarted && !isPaused;
            _stopwatchButtons[TimerButtonType.Continue].Enabled = isStarted && isPaused;

            if (isStarted) _stopWatchShowcase.Start();
            else _stopWatchShowcase.Stop();

            if (isPaused) _stopWatchShowcase.Pause();
            else _stopWatchShowcase.Resume();
        }

        private void ActiveTimesList_ButtonPressed(int buttonIndex)
        {
            if (TimesListLayoutCalculator.HitEditButton(_timesList.GetButtonBounds(buttonIndex).Size, _timesList.RelativeMouseToHovered))
            {
                TimesListButtonEditPressed?.Invoke(buttonIndex);
            }
        }

        protected override void Update()
        {
            UpdateCall?.Invoke();

            if (_inspectingTimer != null)
            {
                // update the selectors to left time of inspecting timer
                TimeSpan remaining = _inspectingTimer.GetRemainingTime(DateTime.Now);
                _timerNumberSelectors[NumberSelectorType.Hour].SetNumber(remaining.Hours);
                _timerNumberSelectors[NumberSelectorType.Minute].SetNumber(remaining.Minutes);
                _timerNumberSelectors[NumberSelectorType.Seconds].SetNumber(remaining.Seconds);
            }

            _stopWatchShowcase.Update();
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
                _renderer.RenderIconedButtons(_timerButtons[TimerButtonType.Discard], _isTimerPaused ? _timerButtons[TimerButtonType.Continue] 
                                                                                                     : _timerButtons[TimerButtonType.Stop]);
            }
            else
            {
                _renderer.RenderIconedButtons(_timerButtons[TimerButtonType.Add]);
            }

            _renderer.RenderNumberSelectors(_alarmNumberSelectors.Values);
            _renderer.RenderNumberSelectors(_timerNumberSelectors.Values);

            _renderer.RenderStopwatch(_stopWatchShowcase);
            if (_stopWatchShowcase.IsStarted)
            {
                _renderer.RenderIconedButtons(_stopwatchButtons[TimerButtonType.Discard], _stopWatchShowcase.IsPaused ? _stopwatchButtons[TimerButtonType.Continue]
                                                                                                                      : _stopwatchButtons[TimerButtonType.Stop]);
            }
            else
            {
                _renderer.RenderIconedButtons(_stopwatchButtons[TimerButtonType.Add]);
            }

            _renderer.EndMainBatch();

            _renderer.RenderTextBox(_alarmLabelTextbox);
            _renderer.RenderTextBox(_timerLabelTextbox);

            // times list
            DateTime now = DateTime.Now;
            for (int i = 0; i < _registedTimes.Count; i++)
            {
                TimeInfo timeInfo = _registedTimes[i];
                if (timeInfo.IsAlarm)
                {
                    DateTime due = timeInfo.DueTime;
                    _renderer.RenderTimesListText(_timesList, i, $"{due.Day} {CalendarShowcase.MonthNames[due.Month]}, {due.Year}.");
                }
                else
                {
                    TimeSpan remaining = timeInfo.GetRemainingTime(now);
                    if (remaining.Hours != 0)
                    {
                        _renderer.RenderTimesListText(_timesList, i, $"Timer ends in: {remaining.Hours}h, {remaining.Minutes}min, {remaining.Seconds}s.");
                    }
                    else if (remaining.Minutes != 0)
                    {
                        _renderer.RenderTimesListText(_timesList, i, $"Timer ends in: {remaining.Minutes}min, {remaining.Seconds}s.");
                    }
                    else
                    {
                        _renderer.RenderTimesListText(_timesList, i, $"Timer ends in: {remaining.Seconds}s.");
                    }
                }
            }
            _renderer.RenderTimesListBackgrounds(_timesList);
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            _alarmCalendar.MouseStateChangeUpdate(e);
            _timesList.MouseStateChangeUpdate(e);
            base.OnMouseButtonStateChange(e);
        }

        protected override void OnMousePositionChange(PopWindowMouseEventArgs e)
        {
            _timesList.MousePositionChanged(e);
            base.OnMousePositionChange(e);
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

            int numberSelectingDistance = FontService.Regular.GetLineSpacing(NumberSelectorTextSize) + 4;
            _timerNumberSelectors[NumberSelectorType.Hour] = new ScrollableNumberSelector(0, 23)
            {
                DistanceToChangeNumber = numberSelectingDistance,
                Boundaries = new Rectangle(315, 45, 20, 78)
            };
            _timerNumberSelectors[NumberSelectorType.Minute] = new ScrollableNumberSelector(0, 59)
            {
                DistanceToChangeNumber = numberSelectingDistance,
                Boundaries = new Rectangle(345, 45, 20, 78)
            };
            _timerNumberSelectors[NumberSelectorType.Seconds] = new ScrollableNumberSelector(0, 59)
            {
                DistanceToChangeNumber = numberSelectingDistance,
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

        private void InitializeStopwatchUI()
        {
            SelectablePacket selectPacket = new SelectablePacket();
            HoldablePacket holdPacket = new HoldablePacket();
            _stopwatchButtons[TimerButtonType.Add] = new IconedButton
            {
                IconId = ContextWindowRenderer.TimerAddButton,
                Boundaries = new Rectangle(338, 260, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => SetStopwatchStatus(isStarted: true, isPaused: false)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _stopwatchButtons[TimerButtonType.Discard] = new IconedButton
            {
                IconId = ContextWindowRenderer.TimerDiscardButton,
                Boundaries = new Rectangle(317, 260, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => SetStopwatchStatus(isStarted: false, isPaused: true)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _stopwatchButtons[TimerButtonType.Stop] = new IconedButton
            {
                IconId = ContextWindowRenderer.TimerStopButton,
                Boundaries = new Rectangle(359, 260, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => SetStopwatchStatus(isStarted: true, isPaused: true)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _stopwatchButtons[TimerButtonType.Continue] = new IconedButton
            {
                IconId = ContextWindowRenderer.TimerAddButton,
                Boundaries = new Rectangle(359, 260, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => SetStopwatchStatus(isStarted: true, isPaused: false)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
        }
    }
}
