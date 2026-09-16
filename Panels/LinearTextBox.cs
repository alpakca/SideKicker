using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Input;
using SideKicker.Theming;
using System;
using System.Diagnostics;

namespace SideKicker.Panels
{
    internal class LinearTextBox : PopElement, ISelectable, IKeyReceiver, ITextReceiver, IHoldable, IClipboardSupporter
    {
        private int _caretPosition;
        private int _textOffset;
        private int _caretX;
        private string _text = "";

        private int _selectionStartCaretPosition;
        private int _selectionStartCaretX;
        private int _selectionEndCaretPosition;
        private int _selectionEndCaretX;

        private bool IsSelectionBoxActive => _selectionStartCaretPosition != _selectionEndCaretPosition;

        public Rectangle SelectableArea => new Rectangle(Boundaries.X - SelectableMargin, Boundaries.Y - SelectableMargin, 
                                                         Boundaries.Width + 2 * SelectableMargin, Boundaries.Height + 2 * SelectableMargin);

        public Rectangle SelectionBox
        {
            get
            {
                if (!IsSelectionBoxActive)
                {
                    return Rectangle.Empty;
                }

                int lineSpacing = FontService.Regular.GetLineSpacing(TextSize);
                Rectangle bounds = new Rectangle(_selectionStartCaretX, (this.Boundaries.Height - lineSpacing) / 2,
                                                 _selectionEndCaretX - _selectionStartCaretX, lineSpacing);

                // adjust bounds if selection end location is less that start position
                if (bounds.Width < 0)
                {
                    bounds.X += bounds.Width;
                    bounds.Width = -bounds.Width;
                }

                return bounds;
            }
        }

        #region PopElement Interface Members

        private bool _isHeld;
        private Point _lastHeldMousePosition;

        SelectablePacket ISelectable.SelectablePacket { get; } = new SelectablePacket { EnabledOutsideSelecting = true };
        KeyReceiverPacket IKeyReceiver.KeyReceiverPacket { get; } = new KeyReceiverPacket();
        TextReceiverPacket ITextReceiver.TextReceiverPacket { get; } = new TextReceiverPacket();
        HoldablePacket IHoldable.HoldPacket { get; } = new HoldablePacket();

        bool IKeyReceiver.ReceivesKeyInput => ReceivingInput;
        bool ITextReceiver.ReceivesTextInput => ReceivingInput;
        bool IHoldable.IsHeld => _isHeld;

        #endregion

        public bool ReceivingInput { get; set; }
        public int SelectableMargin { get; set; }
        public int TextSize { get; set; }
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                UpdateCaret(_caretPosition);
            }
        }

        public int TextOffset => _textOffset;
        /// <summary>X position of caret, TextOffset not included.</summary>
        public int CaretX => _caretX;

        public event Action TextValueChanged;

        public void InsertText(string str)
        {
            str = str.ReplaceLineEndings(string.Empty);

            _text = _text.Insert(_caretPosition, str);
            UpdateCaret(_caretPosition + str.Length);

            TextValueChanged?.Invoke();
        }

        public void Backspace()
        {
            if (_caretPosition == 0 || _text.Length == 0)
            {
                return;
            }

            if (IsSelectionBoxActive)
            {
                int left = _selectionStartCaretPosition;
                int right = _selectionEndCaretPosition;

                if (_selectionStartCaretPosition > _selectionEndCaretPosition)
                {
                    (left, right) = (right, left);
                }

                _text = _text[..left] + _text[right..];
                UpdateCaret(left);
            }
            else
            {
                _text = _text[..(_caretPosition - 1)] + _text[_caretPosition..];
                UpdateCaret(_caretPosition - 1);
            }

            TextValueChanged?.Invoke();
        }

        public void KeyInput(Keys key)
        {
            switch (key)
            {
                case Keys.Up:
                    UpdateCaret(_text.Length);
                    break;

                case Keys.Down:
                    UpdateCaret(0);
                    break;

                case Keys.Left:

                    if (IsSelectionBoxActive)
                    {
                        // start and end doesn't represent left-right
                        UpdateCaret(Math.Min(_selectionStartCaretPosition, _selectionEndCaretPosition));
                    }
                    else
                    {
                        UpdateCaret(_caretPosition - 1);
                    }

                    break;

                case Keys.Right:

                    if (IsSelectionBoxActive)
                    {
                        // start and end doesn't represent left-right
                        UpdateCaret(Math.Max(_selectionStartCaretPosition, _selectionEndCaretPosition));
                    }
                    else
                    {
                        UpdateCaret(_caretPosition + 1);
                    }

                    break;

                case Keys.Back:
                    Backspace();
                    break;
            }
        }

        public bool CanBeSelected(Point point)
        {
            return SelectableArea.Contains(point);
        }

        public bool CanBeHeld(Point mousePosition)
        {
            return SelectableArea.Contains(mousePosition);
        }

        public void OnSelect(PopWindowMouseEventArgs e)
        {
            ReceivingInput = true;

            if (_text.Length == 0)
            {
                UpdateCaret(0);
                return;
            }

            UpdateCaret(GetNearestColumn(e.MousePosition));
        }

        private int GetNearestColumn(Point mousePosition)
        {
            Point inTextBoxPosition = mousePosition - SelectableArea.Location - new Point(_textOffset, 0); // don't forget to include text offset
            int targetX = inTextBoxPosition.X;

            int bestColumn = 0;
            float bestDist = float.MaxValue;

            for (int c = 0; c <= _text.Length; c++)
            {
                float x = FontService.Regular.MeasureWidth(TextSize, _text.Substring(0, c));
                float dist = Math.Abs(x - targetX);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestColumn = c;
                }
            }

            return bestColumn;
        }

        private void UpdateCaret(int value)
        {
            _caretPosition = Math.Clamp(value, 0, _text.Length);
            _caretX = (int)FontService.Regular.MeasureWidth(TextSize, _text[.._caretPosition]);

            AlignTextOffset(_caretPosition, _caretX);
            DiscardSelection();
        }

        private void AlignTextOffset(int caretPosition, int caretXPosition)
        {
            int previousCharWidth = caretPosition > 0 ? (int)FontService.Regular.MeasureWidth(TextSize, _text[caretPosition - 1].ToString())
                                                      : 0;

            if (_textOffset + caretXPosition + SelectableMargin > this.Boundaries.Width)
            {
                _textOffset = this.Boundaries.Width - caretXPosition - SelectableMargin;
            }
            else if (_textOffset + caretXPosition - previousCharWidth < 0)
            {
                _textOffset = previousCharWidth - caretXPosition;
            }

            _textOffset = Math.Min(0, _textOffset);
        }

        public void OnSelectingOutside(PopWindowMouseEventArgs e)
        {
            if (!IsSelectionBoxActive)
            {
                ReceivingInput = false;
            }

            if (IsSelectionBoxActive && e.FirstTimeButtons.HasLeft)
            {
                ReceivingInput = false;
                DiscardSelection();
            }
        }

        public void OnKeyDown(PopWindowMouseEventArgs e, Keys key)
        {
            KeyInput(key);
        }

        public void OnKeyUp(PopWindowMouseEventArgs e, Keys key)
        {
        }

        public void OnTextInputReceive(PopWindowMouseEventArgs e, char ch)
        {
            if (FontService.Regular.CanCharacterResolved(TextSize, ch))
            {
                InsertText(ch.ToString());
            }
        }

        public override void Update()
        {
            // placed in here since hold event raised when mouse position changed / mouse button state changed, and not every frames
            if (_isHeld)
            {
                _selectionEndCaretPosition = GetNearestColumn(_lastHeldMousePosition);
                _selectionEndCaretX = (int)FontService.Regular.MeasureWidth(TextSize, _text[.._selectionEndCaretPosition]);

                AlignTextOffset(_selectionEndCaretPosition, _selectionEndCaretX);
            }

            base.Update();
        }

        public void OnHolding(PopWindowMouseEventArgs e)
        {
            _lastHeldMousePosition = e.MousePosition;
        }

        public void OnHoldStateChange(PopWindowMouseEventArgs e, bool startedHolding)
        {
            _isHeld = startedHolding;

            if (startedHolding)
            {
                _lastHeldMousePosition = e.MousePosition;

                _selectionStartCaretPosition = GetNearestColumn(e.MousePosition);
                _selectionStartCaretX = (int)FontService.Regular.MeasureWidth(TextSize, _text[.._selectionStartCaretPosition]);

                DiscardSelection();
            }
            else
            {
                // if mouse left outside of boundaries, don't update its last position
                if (!SelectableArea.Contains(e.MousePosition))
                {
                    return;
                }

                _selectionEndCaretPosition = GetNearestColumn(e.MousePosition);
                _selectionEndCaretX = (int)FontService.Regular.MeasureWidth(TextSize, _text[.._selectionEndCaretPosition]);
            }
        }

        private void DiscardSelection()
        {
            _selectionEndCaretPosition = _selectionStartCaretPosition;
            _selectionEndCaretX = _selectionStartCaretX;
        }

        public void CopyFromClipboard(string text)
        {
            // remove selected text before inserting new
            if (IsSelectionBoxActive)
            {
                Backspace();
            }

            InsertText(text);
        }

        public string GetSelectedText()
        {
            if (!ReceivingInput) return null;

            if (!IsSelectionBoxActive) return string.Empty;

            int left = _selectionStartCaretPosition;
            int right = _selectionEndCaretPosition;

            if (_selectionStartCaretPosition > _selectionEndCaretPosition)
            {
                (left, right) = (right, left);
            }

            return _text.Substring(left, right - left);
        }
    }
}
