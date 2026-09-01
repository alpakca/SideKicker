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
        private const int ButtonsTextSize = 12;
        private const int CalendarNumbersTextSize = 16;
        private const int NumberSelectorTextSize = 16;
        private const int HeaderTextSize = 20;

        // alarm UI elements
        private readonly MultiLineTextBox _alarmLabelTextbox;
        private readonly CalendarShowcase _alarmCalendar;
        private readonly Dictionary<NumberSelectorType, ScrollableNumberSelector> _alarmNumberSelectors;
        private readonly Dictionary<AlarmButtonType, TextedButton> _alarmButtons;

        // timer UI elements
        private readonly MultiLineTextBox _timerLabelTextbox;
        private readonly Dictionary<NumberSelectorType, ScrollableNumberSelector> _timerNumberSelectors;
        private readonly Dictionary<TimerButtonType, Clickable> _timerButtons;

        private readonly TextPlaque[] _textPlaques;
        private readonly ScrollableButtonList _activeTimesList;

        public event Action<AlarmButtonType> AlarmButtonPressed;
        public event Action<TimerButtonType> TimerButtonPressed;
        public event Action<int> TimesListButtonEditPressed;
        public event Action UpdateCall;

        public ContextWindow()
        {
            _alarmLabelTextbox = new MultiLineTextBox();
            _alarmCalendar = new CalendarShowcase();
            _alarmNumberSelectors = new Dictionary<NumberSelectorType, ScrollableNumberSelector>();
            _alarmButtons = new Dictionary<AlarmButtonType, TextedButton>();
            InitializeAlarmUI();

            _timerLabelTextbox = new MultiLineTextBox();
            _timerNumberSelectors = new Dictionary<NumberSelectorType, ScrollableNumberSelector>();
            _timerButtons = new Dictionary<TimerButtonType, Clickable>();
            InitializeTimerUI();

            _textPlaques = new TextPlaque[]
            {
                new TextPlaque { IsBold = false, Text = "Set Alarm", TextSize = HeaderTextSize, Position = new Vector2(20, 20) },
                new TextPlaque { IsBold = false, Text = "Set Timer", TextSize = HeaderTextSize, Position = new Vector2(200, 20)}
            };

            _activeTimesList = new ScrollableButtonList
            {
                Boundaries = new Rectangle(),
                ButtonHeight = 0,
                ButtonMargin = 0
            };
            _activeTimesList.ButtonPressed += ActiveTimesList_ButtonPressed;
            _activeTimesList.ButtonHeld += ActiveTimesList_ButtonHeld;

            this.Elements.AddRange([_alarmLabelTextbox, .. _alarmNumberSelectors.Values, .. _alarmButtons.Values, 
                                    _timerLabelTextbox, .. _timerNumberSelectors.Values, .. _timerButtons.Values]);
            this.Boundaries = new Rectangle(0, 0, 400, 400);
        }

        public void SetTimesListCount(int value)
        {
            _activeTimesList.ButtonCount = value;
            _activeTimesList.UpdateScrolledLength();
        }

        public void SetShownTimerStatus(bool isStopped)
        {

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
            }
            else
            {
                _alarmLabelTextbox.SetText(info.LabelText);

                DateTime due = info.DueTime;
                _alarmCalendar.SetDate(due.Year, due.Month, due.Day);
                _alarmNumberSelectors[NumberSelectorType.Hour].SetNumber(due.Hour);
                _alarmNumberSelectors[NumberSelectorType.Minute].SetNumber(due.Minute);
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
            }
            else
            {
                _timerLabelTextbox.SetText(info.LabelText);

                DateTime remaining = new DateTime((info.DueTime - DateTime.Now).Ticks);
                _timerNumberSelectors[NumberSelectorType.Hour].SetNumber(remaining.Hour);
                _timerNumberSelectors[NumberSelectorType.Minute].SetNumber(remaining.Minute);
                _timerNumberSelectors[NumberSelectorType.Seconds].SetNumber(remaining.Second);
            }
        }

        public TimeInfo GetTimer()
        {
            DateTime now = DateTime.Now;
            return new TimeInfo
            {
                IsAlarm = false,
                LabelText = _timerLabelTextbox.Text,
                DueTime = new DateTime(now.Year, now.Month, now.Day,
                                       now.Hour + _timerNumberSelectors[NumberSelectorType.Hour].CurrentNumber,
                                       now.Minute + _timerNumberSelectors[NumberSelectorType.Minute].CurrentNumber,
                                       now.Second + _timerNumberSelectors[NumberSelectorType.Seconds].CurrentNumber)
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
            base.Update();
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            _alarmCalendar.MouseStateChangeUpdate(e);
            base.OnMouseButtonStateChange(e);
        }

        private void InitializeAlarmUI()
        {
            _alarmLabelTextbox.TextSize = TextBoxTextSize;
            _alarmLabelTextbox.Boundaries = new Rectangle();
            _alarmLabelTextbox.SelectableMargin = 0;

            _alarmCalendar.ButtonHeight = FontService.Regular.GetLineSpacing(CalendarNumbersTextSize);
            _alarmCalendar.ButtonMargin = 4;
            _alarmCalendar.Location = new Point(20, 45);

            _alarmNumberSelectors[NumberSelectorType.Hour] = new ScrollableNumberSelector(0, 23)
            {
                
            };
            _alarmNumberSelectors[NumberSelectorType.Minute] = new ScrollableNumberSelector(0, 59)
            {

            };

            SelectablePacket selectPacket = new SelectablePacket();
            _alarmButtons[AlarmButtonType.Add] = new TextedButton
            {
                Text = "Add",
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Add)),

                SelectablePacket = selectPacket
            };
            _alarmButtons[AlarmButtonType.Delete] = new TextedButton
            {
                Text = "Delete",
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Delete)),

                SelectablePacket = selectPacket
            };
            _alarmButtons[AlarmButtonType.Save] = new TextedButton
            {
                Text = "Save",
                ClickCommand = new ActionCommander<Clickable>(_ => AlarmButtonPressed?.Invoke(AlarmButtonType.Save)),

                SelectablePacket = selectPacket
            };
        }

        private void InitializeTimerUI()
        {
            _timerLabelTextbox.TextSize = TextBoxTextSize;
            _timerLabelTextbox.Boundaries = new Rectangle();
            _timerLabelTextbox.SelectableMargin = 0;

            _timerNumberSelectors[NumberSelectorType.Hour] = new ScrollableNumberSelector(0, 23)
            {

            };
            _timerNumberSelectors[NumberSelectorType.Minute] = new ScrollableNumberSelector(0, 59)
            {

            };
            _timerNumberSelectors[NumberSelectorType.Seconds] = new ScrollableNumberSelector(0, 59)
            {

            };

            SelectablePacket selectPacket = new SelectablePacket();
            _timerButtons[TimerButtonType.Add] = new Clickable
            {
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Add)),

                SelectablePacket = selectPacket
            };
            _timerButtons[TimerButtonType.Discard] = new Clickable
            {
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Discard)),

                SelectablePacket = selectPacket
            };
            _timerButtons[TimerButtonType.Stop] = new Clickable
            {
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Stop)),

                SelectablePacket = selectPacket
            };
            _timerButtons[TimerButtonType.Continue] = new Clickable
            {
                ClickCommand = new ActionCommander<Clickable>(_ => TimerButtonPressed?.Invoke(TimerButtonType.Continue)),

                SelectablePacket = selectPacket
            };
        }
    }
}
