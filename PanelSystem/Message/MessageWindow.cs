using Microsoft.Xna.Framework;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using SideKicker.Theming;
using System;
using System.Collections.Generic;

namespace SideKicker.PanelSystem.Message
{
    internal class MessageWindow : PopWindow
    {
        private const int TopCancelButtonHeight = 35;
        private const int TopCancelButtonWidth = 60;

        private const int MaxTextRowWidth = 300;
        private const int FrameMargin = 40;
        private const int TextSize = 12;

        private const int BottomButtonHeight = 30;
        private const int BottomButtonWidth = 80;
        private const int BottomButtonMargin = 20;

        private const string OkButtonText = "Ok";
        private const string CancelButtonText = "Cancel";

        private static readonly Queue<MessageWindow> _pooledWindows = new Queue<MessageWindow>();
        private static readonly Stack<MessageWindow> _activeWindows = new Stack<MessageWindow>();

        private bool _isActive;
        private Point _rightMostButtonPosition;
        private Point _textStartPosition;
        private Action<MessageWindowResult> _returnAction;

        private Rectangle _hoveredButtonBounds;
        private MessageWindowResult _hoveredButtonResult;
        private bool _isHoveredButtonHeld;

        private Rectangle TopCloseButtonBounds => new Rectangle(Boundaries.Width - TopCancelButtonWidth, 0, TopCancelButtonWidth, TopCancelButtonHeight);

        private readonly List<string> _lines;
        private readonly List<(string Text, MessageWindowResult Result)> _buttonTexts;

        public bool IsActive => _isActive;
        public Point WindowSize => this.Boundaries.Size;

        public static IEnumerable<MessageWindow> ActiveWindows => _activeWindows;
        public static event Action WindowAdded;

        public MessageWindow()
        {
            _lines = new List<string>();
            _buttonTexts = new List<(string, MessageWindowResult)>();
        }

        public void SetLocation(Point value)
        {
            if (IsActive)
            {
                this.Boundaries = new Rectangle(value, this.Boundaries.Size);
            }
        }

        public static void Show(string text, MessageWindowButton buttons, Action<MessageWindowResult> returnAction = null)
        {
            if (!_pooledWindows.TryDequeue(out MessageWindow messageWindow))
            {
                messageWindow = new MessageWindow();
            }

            _activeWindows.Push(messageWindow);

            messageWindow._returnAction = returnAction;
            messageWindow._isActive = true;
            messageWindow.ShowWindow(text, buttons);

            foreach (var window in PopWindowManager.ActiveWindows) window.Frozen = true;
            PopWindowManager.ActiveWindows.Add(messageWindow);

            WindowAdded?.Invoke();
        }

        private void ShowWindow(string text, MessageWindowButton buttons)
        {
            _buttonTexts.Clear();
            switch (buttons)
            {
                case MessageWindowButton.Cancel:
                    _buttonTexts.Add((CancelButtonText, MessageWindowResult.Cancel));
                    break;

                case MessageWindowButton.Ok:
                    _buttonTexts.Add((OkButtonText, MessageWindowResult.Ok));
                    break;

                case MessageWindowButton.CancelOk:
                    _buttonTexts.Add((CancelButtonText, MessageWindowResult.Cancel));
                    _buttonTexts.Add((OkButtonText, MessageWindowResult.Ok));
                    break;

                default:
                    throw new ArgumentException();
            }

            _lines.Clear();
            if (text.Length != 0)
            {
                int lastStart = 0;
                int lastSpaceIndex = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ') lastSpaceIndex = i;
                    if (text[i] == '\n')
                    {
                        _lines.Add(text[lastStart..i]);
                        lastStart = i + 1;
                        continue;
                    }

                    if (FontService.Regular.MeasureWidth(TextSize, text[lastStart..i]) >= MaxTextRowWidth)
                    {
                        if (lastSpaceIndex == lastStart) // no space found, force break
                        {
                            _lines.Add(text[lastStart..i]);
                            lastStart = i;
                        }
                        else
                        {
                            _lines.Add(text[lastStart..lastSpaceIndex]);
                            lastStart = lastSpaceIndex;
                        }
                    }
                }

                _lines.Add(text[lastStart..]);
            }

            _textStartPosition = new Point(FrameMargin, FrameMargin);
            Point textSize = new Point(MaxTextRowWidth, FontService.Regular.GetLineSpacing(TextSize) * _lines.Count);

            _rightMostButtonPosition = new Point(_textStartPosition.X + textSize.X - BottomButtonWidth, 
                                                 _textStartPosition.Y + textSize.Y + BottomButtonMargin);

            Point windowSize = new Point(FrameMargin * 2 + textSize.X, FrameMargin + textSize.Y + BottomButtonMargin * 2 + BottomButtonHeight);
            this.Boundaries = new Rectangle(new Point(1420, 470), windowSize);
        }

        protected override void RenderUpdate()
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Point windowSize = this.Boundaries.Size;

            DrawHelper.SetBeginBatch(WindowTexture, DrawHelper.AlphaBlendDrawOptions);

            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, windowSize), theme.GetColor(ColorRegistry.BorderId),
                                              theme.GetColor(ColorRegistry.SurfaceId), 1);

            DrawHelper.DrawTextureCenteralized(TopCloseButtonBounds, theme.GetIcon(IconRegistry.CloseButtonId));

            Vector2 currentTextPosition = _textStartPosition.ToVector2();
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);
            for (int i = 0; i < _lines.Count; i++)
            {
                FontService.Regular.DrawText(TextSize, _lines[i], currentTextPosition, textColor);
                currentTextPosition.Y += FontService.Regular.GetLineSpacing(TextSize);
            }

            Point currentButtonPosition = _rightMostButtonPosition;
            Color buttonColor = theme.GetColor(ColorRegistry.BackgroundId);
            for (int i = _buttonTexts.Count - 1; i >= 0; i--)
            {
                Rectangle bounds = new Rectangle(currentButtonPosition.X, currentButtonPosition.Y, BottomButtonWidth, BottomButtonHeight);

                DrawHelper.DrawBackground(bounds, buttonColor);
                FontService.Regular.DrawCenteredText(TextSize, _buttonTexts[i].Text, bounds, textColor);

                currentButtonPosition.X -= BottomButtonMargin + BottomButtonWidth;
            }

            DrawHelper.DrawBackground(_hoveredButtonBounds, _isHoveredButtonHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                                 : theme.GetColor(ColorRegistry.HoverOverlayId));

            DrawHelper.EndBatch();
        }

        protected override void OnMousePositionChange(PopWindowMouseEventArgs e)
        {
            Point mousePosition = e.MousePosition;

            Rectangle topCloseButtonBounds = TopCloseButtonBounds;
            if (topCloseButtonBounds.Contains(mousePosition))
            {
                _hoveredButtonBounds = topCloseButtonBounds;
                _hoveredButtonResult = MessageWindowResult.Close;
                return;
            }
            else
            {
                Point currentButtonPosition = _rightMostButtonPosition;
                for (int i = 0; i < _buttonTexts.Count; i++)
                {
                    Rectangle buttonBounds = new Rectangle(currentButtonPosition.X, currentButtonPosition.Y, BottomButtonWidth, BottomButtonHeight);
                    if (buttonBounds.Contains(mousePosition))
                    {
                        _hoveredButtonBounds = buttonBounds;
                        _hoveredButtonResult = _buttonTexts[i].Result;
                        return;
                    }

                    currentButtonPosition.X -= BottomButtonMargin + BottomButtonWidth;
                }
            }

            _hoveredButtonBounds = Rectangle.Empty;
            _hoveredButtonResult = MessageWindowResult.None;
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            if (e.FirstTimeButtons.HasLeft && !_hoveredButtonBounds.IsEmpty)
            {
                _isHoveredButtonHeld = true;
            }
            else if (e.ReleasedButtons.HasLeft)
            {
                if (_isHoveredButtonHeld && !_hoveredButtonBounds.IsEmpty)
                {
                    PopWindowManager.ActiveWindows.Remove(this);
                    foreach (var window in PopWindowManager.ActiveWindows.GetReverseEnumerable())
                    {
                        window.Frozen = false;

                        if (window is MessageWindow) break;
                    }

                    _isActive = false;
                    _activeWindows.Pop();

                    _pooledWindows.Enqueue(this);
                    _returnAction?.Invoke(_hoveredButtonResult);
                }

                _isHoveredButtonHeld = false;
            }
        }
    }
}
