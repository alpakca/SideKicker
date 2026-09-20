using Microsoft.Xna.Framework;
using PopUpUI.Elements;
using PopUpUI.Input;
using SideKicker.PanelSystem.Message;
using SideKicker.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SideKicker.Panels.Timers
{
    internal class ContextWindow : MessageSupporterWindow
    {
        private const int TextBoxTextSize = 12;
        private const int NumberSelectorTextSize = 16;
        private const int HeaderTextSize = 20;

        // alarm UI elements
        private bool _isInspectingAlarm;
        private bool _anyRepeatingDaysSelected;
        private readonly MultiLineTextBox _alarmLabelTextbox;
        private readonly CalendarShowcase _alarmCalendar;
        private readonly OrderedDictionary<NumberSelectorType, ScrollableNumberSelector> _alarmNumberSelectors;
        private readonly Dictionary<AlarmButtonType, TextedButton> _alarmButtons;
        private readonly IconedButton[] _alarmMonthChangeButtons;
        private readonly ToggleButton[] _weekdaysButtons; // buttons start from monday to sunday

        // timer UI elements
        private TimerInfo? _inspectingTimer;
        private readonly RecentTimeButton[] _recentTimeButtons;
        private readonly MultiLineTextBox _timerLabelTextbox;
        private readonly OrderedDictionary<NumberSelectorType, ScrollableNumberSelector> _timerNumberSelectors;
        private readonly Dictionary<TimerButtonType, IconedButton> _timerButtons;

        // stopwatch ui
        private readonly StopwatchShowcase _stopWatchShowcase;
        private readonly Dictionary<TimerButtonType, IconedButton> _stopwatchButtons;

        private readonly TextPlaque[] _textPlaques;
        private readonly ScrollableButtonList _timesList;

        private readonly ContextWindowRenderer _renderer;
        private readonly IReadOnlyList<ITimeInfo> _registedTimes;

        public event Action<AlarmButtonType>? AlarmButtonPressed;
        public event Action<TimerButtonType>? TimerButtonPressed;
        public event Action<int>? TimesListButtonEditPressed;

        public ContextWindow(IReadOnlyList<ITimeInfo> registeredTimes)
        {
            _registedTimes = registeredTimes;

            _alarmCalendar = new CalendarShowcase();
            _alarmCalendar.HitDay += () =>
            {
                _anyRepeatingDaysSelected = false;
                foreach (var button in _weekdaysButtons!)
                {
                    button.SetStatusWithoutCommander(false);
                }
            };

            _alarmLabelTextbox = new MultiLineTextBox();
            _alarmNumberSelectors = new OrderedDictionary<NumberSelectorType, ScrollableNumberSelector>();
            _alarmButtons = new Dictionary<AlarmButtonType, TextedButton>();
            _alarmMonthChangeButtons = new IconedButton[2];
            _weekdaysButtons = new ToggleButton[7];
            InitializeAlarmUI();

            _recentTimeButtons = new RecentTimeButton[2];
            _timerLabelTextbox = new MultiLineTextBox();
            _timerNumberSelectors = new OrderedDictionary<NumberSelectorType, ScrollableNumberSelector>();
            _timerButtons = new Dictionary<TimerButtonType, IconedButton>();
            InitializeTimerUI();
            RecentTimeLoader.Load(_recentTimeButtons);

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
                Boundaries = new Rectangle(260, 340, 178, 114),
                ButtonHeight = TimesListLayoutCalculator.IconSideLength,
                ButtonMargin = 4
            };
            _timesList.ButtonPressed += ActiveTimesList_ButtonPressed;

            this.Elements.AddRange([_alarmLabelTextbox, .. _alarmNumberSelectors.Values, .. _alarmButtons.Values, .. _alarmMonthChangeButtons, .. _weekdaysButtons, 
                                    _timerLabelTextbox, .. _timerNumberSelectors.Values, .. _timerButtons.Values, .. _recentTimeButtons,
                                                                                         .. _stopwatchButtons.Values]);
            RegisterClipboardSupporters(_alarmLabelTextbox, _timerLabelTextbox);

            this.Boundaries = new Rectangle(0, 0, 450, 460);
            _renderer = new ContextWindowRenderer(WindowTexture);
        }

        public void OnExiting()
        {
            RecentTimeLoader.Save(_recentTimeButtons);
        }

        public void SetTimesListCount(int value)
        {
            _timesList.ButtonCount = value;
            _timesList.UpdateScrolledLength();
        }

        public void ResetAlarm()
        {
            DateTime now = DateTime.Now;

            _alarmLabelTextbox.SetText("");
            _alarmCalendar.SetDate(now.Year, now.Month, now.Day);
            _alarmNumberSelectors[NumberSelectorType.Hour].SetNumber(now.Hour);
            _alarmNumberSelectors[NumberSelectorType.Minute].SetNumber(now.Minute);

            _alarmButtons[AlarmButtonType.Delete].Enabled = false;
            _alarmButtons[AlarmButtonType.Save].Enabled = false;
            _alarmButtons[AlarmButtonType.Add].Enabled = true;

            _isInspectingAlarm = false;
            _anyRepeatingDaysSelected = false;
        }

        public void InspectAlarm(AlarmInfo info)
        {
            _alarmLabelTextbox.SetText(info.LabelText);

            DateTime due = info.DueTime;
            _alarmCalendar.SetDate(due.Year, due.Month, due.Day);
            _alarmNumberSelectors[NumberSelectorType.Hour].SetNumber(due.Hour);
            _alarmNumberSelectors[NumberSelectorType.Minute].SetNumber(due.Minute);

            _alarmButtons[AlarmButtonType.Add].Enabled = false;
            _alarmButtons[AlarmButtonType.Delete].Enabled = true;
            _alarmButtons[AlarmButtonType.Save].Enabled = true;

            _isInspectingAlarm = true;
            _anyRepeatingDaysSelected = info.IsRepeating;

            if (info.IsRepeating)
            {
                foreach (DayOfWeek day in info.RepeatingDays!)
                {
                    // from Sunday zero based to Monday based zero
                    int index = ((int)day - 1 + 7) % 7;
                    _weekdaysButtons[index].SetStatusWithoutCommander(true);
                }
            }
        }

        public void UpdateAlarmWithUI(AlarmInfo info)
        {
            info.LabelText = _alarmLabelTextbox.Text;

            if (_anyRepeatingDaysSelected)
            {
                info.SetRepeating(GetAlarmUIDateTime().TimeOfDay, GetAlarmUIRepeatingDays());
            }
            else
            {
                info.SetSingle(GetAlarmUIDateTime());
            }
        }

        public bool IsSelectedAlarmAlreadyPassed()
        {
            // repeating alarms doesn't get passed
            if (_anyRepeatingDaysSelected) return false;

            return DateTime.Now >= GetAlarmUIDateTime();
        }

        private DateTime GetAlarmUIDateTime()
        {
            return new DateTime(_alarmCalendar.SelectedYear, _alarmCalendar.SelectedMonth, _alarmCalendar.SelectedDay,
                                _alarmNumberSelectors[NumberSelectorType.Hour].CurrentNumber,
                                _alarmNumberSelectors[NumberSelectorType.Minute].CurrentNumber,
                                second: 0);
        }

        private DayOfWeek[] GetAlarmUIRepeatingDays()
        {
            int count = 0;
            Span<DayOfWeek> days = stackalloc DayOfWeek[7];

            for (int i = 0; i < 7; i++)
            {
                if (_weekdaysButtons[i].IsChecked)
                {
                    // buttons start from Monday, DayOfWeek's zero index start from Sunday
                    // add one to adjust it
                    days[count++] = (DayOfWeek)((i + 1) % 7);
                }
            }

            return days.Slice(0, count).ToArray();
        }

        public void ResetTimer()
        {
            _timerLabelTextbox.SetText("");
            SetTimerNumberSelectors(TimeSpan.Zero);

            _timerButtons[TimerButtonType.Discard].Enabled = false;
            _timerButtons[TimerButtonType.Stop].Enabled = false;
            _timerButtons[TimerButtonType.Continue].Enabled = false;
            _timerButtons[TimerButtonType.Add].Enabled = true;

            _inspectingTimer = null;
        }

        public void InspectTimer(TimerInfo info)
        {
            _timerLabelTextbox.SetText(info.LabelText);

            TimeSpan remaining = info.GetRemainingTime(DateTime.Now);
            SetTimerNumberSelectors(remaining);

            _timerButtons[TimerButtonType.Discard].Enabled = true;
            _timerButtons[TimerButtonType.Stop].Enabled = !info.IsPaused;
            _timerButtons[TimerButtonType.Continue].Enabled = info.IsPaused;
            _timerButtons[TimerButtonType.Add].Enabled = false;

            _inspectingTimer = info;
        }

        /// <summary>
        /// Gets a timer TimeInfo object using UI elements. Sets DueTime as adding given <paramref name="dateTime"/> to number selectors in UI.
        /// </summary>
        /// <param name="dateTime">A DateTime object to add to values of number selectors in UI, to offset them.</param>
        /// <returns>Created TimeInfo object.</returns>
        public void UpdateTimerWithUI(TimerInfo info)
        {
            info.LabelText = _timerLabelTextbox.Text;

            TimeSpan duration = new TimeSpan(_timerNumberSelectors[NumberSelectorType.Hour].CurrentNumber,
                                             _timerNumberSelectors[NumberSelectorType.Minute].CurrentNumber,
                                             _timerNumberSelectors[NumberSelectorType.Seconds].CurrentNumber);
            info.SetDuration(duration);

            // set timer value to recent timers only if there wasn't an inspecting timer,
            // it was for adding a new timer
            if (_inspectingTimer == null)
            {
                InsertRecentTimeValue(duration);
            }
        }

        private void SetTimerNumberSelectors(TimeSpan value)
        {
            _timerNumberSelectors[NumberSelectorType.Hour].SetNumber(value.Hours);
            _timerNumberSelectors[NumberSelectorType.Minute].SetNumber(value.Minutes);
            _timerNumberSelectors[NumberSelectorType.Seconds].SetNumber(value.Seconds);
        }

        public bool IsTimerSelectedTimeZero()
        {
            return _timerNumberSelectors[NumberSelectorType.Hour].CurrentNumber == 0 &&
                   _timerNumberSelectors[NumberSelectorType.Minute].CurrentNumber == 0 &&
                   _timerNumberSelectors[NumberSelectorType.Seconds].CurrentNumber == 0;
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
            if (TimesListLayoutCalculator.Instance.TryGetHoveredCommonIconRegion(_timesList.RelativeMouseToHovered, _timesList.ButtonSize,
                                                                                 out TimesListRegion region) &&
                region == TimesListRegion.EditButton)
            {
                TimesListButtonEditPressed?.Invoke(buttonIndex);
            }
        }

        private void InsertRecentTimeValue(TimeSpan duration)
        {
            for (int i = _recentTimeButtons.Length - 2; i >= 0; i--)
            {
                _recentTimeButtons[i + 1].SetValues(_recentTimeButtons[i]);
            }

            _recentTimeButtons[0].SetNew(duration);
        }

        protected override void Update()
        {
            if (_inspectingTimer != null && !_inspectingTimer.IsPaused)
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

        protected override void RenderUpdateWithoutMessages()
        {
            _renderer.StartMainBatch();

            _renderer.Render(_textPlaques, _alarmCalendar, _weekdaysButtons, _anyRepeatingDaysSelected);

            _renderer.RenderIconedButtons(_alarmMonthChangeButtons);
            if (_isInspectingAlarm)
            {
                _renderer.RenderTextedButtons(_alarmButtons[AlarmButtonType.Delete], _alarmButtons[AlarmButtonType.Save]);
            }
            else
            {
                _renderer.RenderTextedButtons(_alarmButtons[AlarmButtonType.Add]);
            }

            _renderer.RenderTextedButtons(_recentTimeButtons);
            if (_inspectingTimer != null)
            {
                _renderer.RenderIconedButtons(_timerButtons[TimerButtonType.Discard], _timerButtons[TimerButtonType.Save], 
                                              _timerButtons[_inspectingTimer.IsPaused ? TimerButtonType.Continue : TimerButtonType.Stop]);
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

            _renderer.RenderScrollableButtonList(_timesList, _registedTimes);
        }

        protected override void OnMouseButtonStateChangeWithoutMessages(PopWindowMouseEventArgs e)
        {
            _alarmCalendar.MouseStateChangeUpdate(e);
            _timesList.MouseStateChangeUpdate(e);
        }

        protected override void OnMousePositionChangeWithoutMessages(PopWindowMouseEventArgs e)
        {
            _timesList.MousePositionChanged(e);
        }

        protected override void OnMouseVerticalScrollChange(PopWindowMouseEventArgs e, int change)
        {
            if (_timesList.Boundaries.Contains(e.MousePosition))
            {
                _timesList.Scroll(e, -change / 6);
            }

            base.OnMouseVerticalScrollChange(e, change);
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
                TextSize = TextBoxTextSize,
                Boundaries = new Rectangle(164, 385, 80, 30),
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Add)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _alarmButtons[AlarmButtonType.Delete] = new TextedButton
            {
                Text = "Delete",
                TextSize = TextBoxTextSize,
                Boundaries = new Rectangle(86, 385, 70, 30),
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Delete)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _alarmButtons[AlarmButtonType.Save] = new TextedButton
            {
                Text = "Save",
                TextSize = TextBoxTextSize,
                Boundaries = new Rectangle(174, 385, 70, 30),
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Save)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };

            _alarmMonthChangeButtons[0] = new IconedButton
            {
                IconType = Rendering.IconType.UpArrow,
                Boundaries = new Rectangle(175, 45, TimesListLayoutCalculator.CalendarDayButtonHeight, TimesListLayoutCalculator.CalendarDayButtonHeight),
                ClickCommand = new ActionCommander<Clickable>(_ => _alarmCalendar.MoveMonth(-1)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _alarmMonthChangeButtons[1] = new IconedButton
            {
                IconType = Rendering.IconType.DownArrow,
                Boundaries = new Rectangle(210, 45, TimesListLayoutCalculator.CalendarDayButtonHeight, TimesListLayoutCalculator.CalendarDayButtonHeight),
                ClickCommand = new ActionCommander<Clickable>(_ => _alarmCalendar.MoveMonth(+1)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };

            Point dayWeekOffset = new Point(20, 70);
            ActionCommander<ToggleButton> weekDayToggleAction = new ActionCommander<ToggleButton>(_ =>
            {
                _anyRepeatingDaysSelected = _weekdaysButtons.Any(button => button.IsChecked);
            });

            for (int i = 0; i < _weekdaysButtons.Length; i++)
            {
                Rectangle bounds = TimesListLayoutCalculator.CalendarWeekDayBounds(i);
                _weekdaysButtons[i] = new ToggleButton
                {
                    Boundaries = new Rectangle(dayWeekOffset + bounds.Location, bounds.Size),
                    ToggleCommand = weekDayToggleAction,

                    SelectablePacket = selectPacket,
                    HoldPacket = holdPacket
                };
            }
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
                IconType = Rendering.IconType.TimerAddButton,
                Boundaries = new Rectangle(338, 215, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Add)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _timerButtons[TimerButtonType.Discard] = new IconedButton
            {
                IconType = Rendering.IconType.TimerDiscardButton,
                Boundaries = new Rectangle(299, 215, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Discard)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _timerButtons[TimerButtonType.Stop] = new IconedButton
            {
                IconType = Rendering.IconType.TimerPauseButton,
                Boundaries = new Rectangle(341, 215, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Stop)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _timerButtons[TimerButtonType.Continue] = new IconedButton
            {
                IconType = Rendering.IconType.TimerAddButton,
                Boundaries = new Rectangle(341, 215, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Continue)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _timerButtons[TimerButtonType.Save] = new IconedButton
            {
                IconType = Rendering.IconType.TimerSaveButton,
                Boundaries = new Rectangle(383, 215, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Save)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };

            ActionCommander<Clickable> recentTimeButtonAction = new ActionCommander<Clickable>(button =>
            {
                SetTimerNumberSelectors(((RecentTimeButton)button).Timer);
            });

            _recentTimeButtons[0] = new RecentTimeButton
            {
                TextSize = TextBoxTextSize,
                Boundaries = new Rectangle(266, 178, 80, 30),
                ClickCommand = recentTimeButtonAction,

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket,
            };
            _recentTimeButtons[1] = new RecentTimeButton
            {
                TextSize = TextBoxTextSize,
                Boundaries = new Rectangle(354, 178, 80, 30),
                ClickCommand = recentTimeButtonAction,

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket,
            };
        }

        private void InitializeStopwatchUI()
        {
            SelectablePacket selectPacket = new SelectablePacket();
            HoldablePacket holdPacket = new HoldablePacket();
            _stopwatchButtons[TimerButtonType.Add] = new IconedButton
            {
                IconType = Rendering.IconType.TimerAddButton,
                Boundaries = new Rectangle(338, 300, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => SetStopwatchStatus(isStarted: true, isPaused: false)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _stopwatchButtons[TimerButtonType.Discard] = new IconedButton
            {
                IconType = Rendering.IconType.TimerDiscardButton,
                Boundaries = new Rectangle(317, 300, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => SetStopwatchStatus(isStarted: false, isPaused: true)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _stopwatchButtons[TimerButtonType.Stop] = new IconedButton
            {
                IconType = Rendering.IconType.TimerPauseButton,
                Boundaries = new Rectangle(359, 300, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => SetStopwatchStatus(isStarted: true, isPaused: true)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
            _stopwatchButtons[TimerButtonType.Continue] = new IconedButton
            {
                IconType = Rendering.IconType.TimerAddButton,
                Boundaries = new Rectangle(359, 300, 32, 32),
                ClickCommand = new ActionCommander<Clickable>(_ => SetStopwatchStatus(isStarted: true, isPaused: false)),

                SelectablePacket = selectPacket,
                HoldPacket = holdPacket
            };
        }
    }
}
