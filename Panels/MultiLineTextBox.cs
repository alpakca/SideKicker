using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Input;
using SideKicker.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels
{
    internal class MultiLineTextBox : PopElement, ISelectable, IKeyReceiver, ITextReceiver, IMouseWheelReceiver, IHoldable, IClipboardSupporter
    {
        private readonly StringBuilder _text;
        private readonly List<LineInfo> _lines;

        private CaretInfo _caret;

        private float _desiredCaretX;
        private int _scrolledOffset;

        private CaretInfo _selectedStartCaretPosition;
        private int _selectedStartCaretX;
        private CaretInfo _selectedEndCaretPosition;
        private int _selectedEndCaretX;

        private bool IsSelectionBoxActive => _selectedStartCaretPosition.Index != _selectedEndCaretPosition.Index;

        #region PopElement Interface Members

        private bool _isHeld;
        private Point _lastHeldMousePosition;

        SelectablePacket ISelectable.SelectablePacket { get; } = new SelectablePacket { EnabledOutsideSelecting = true };
        KeyReceiverPacket IKeyReceiver.KeyReceiverPacket { get; } = new KeyReceiverPacket();
        TextReceiverPacket ITextReceiver.TextReceiverPacket { get; } = new TextReceiverPacket();
        MouseWheelReceiverPacket IMouseWheelReceiver.MouseWheelReceiverPacket { get; } = new MouseWheelReceiverPacket();
        HoldablePacket IHoldable.HoldPacket { get; } = new HoldablePacket();

        bool IKeyReceiver.ReceivesKeyInput => ReceivingInput;
        bool ITextReceiver.ReceivesTextInput => ReceivingInput;
        bool IHoldable.IsHeld => _isHeld;

        #endregion

        public Rectangle SelectableArea => new Rectangle(Boundaries.X - SelectableMargin, Boundaries.Y - SelectableMargin,
                                                         Boundaries.Width + 2 * SelectableMargin, Boundaries.Height + 2 * SelectableMargin);

        public IEnumerable<Rectangle> SelectionBoxes
        {
            get
            {
                if (_selectedStartCaretPosition.Index == _selectedEndCaretPosition.Index)
                {
                    yield break;
                }

                CaretInfo left = _selectedStartCaretPosition;
                CaretInfo right = _selectedEndCaretPosition;

                int leftCaretX = _selectedStartCaretX;
                int rightCaretX = _selectedEndCaretX;

                if (_selectedStartCaretPosition.Index > _selectedEndCaretPosition.Index)
                {
                    (left, right) = (right, left);
                    (leftCaretX, rightCaretX) = (rightCaretX, leftCaretX);
                }

                int lineSpacing = FontService.Regular.GetLineSpacing(TextSize);
                LineInfo leftLine = _lines[left.LineIndex];

                // single line case
                if (left.LineIndex == right.LineIndex)
                {
                    yield return new Rectangle(leftCaretX, left.LineIndex * lineSpacing, rightCaretX - leftCaretX, lineSpacing);
                    yield break;
                }

                // top line
                int leftInline = left.Index - leftLine.StartIndex;
                int topWidth = (int)MeasureWidth(leftLine.Text.Substring(leftInline));

                yield return new Rectangle(leftCaretX, left.LineIndex * lineSpacing, topWidth, lineSpacing);

                // middle lines
                for (int lineIndex = left.LineIndex + 1; lineIndex < right.LineIndex; lineIndex++)
                {
                    yield return new Rectangle(0, lineIndex * lineSpacing, (int)MeasureWidth(_lines[lineIndex].Text), lineSpacing);
                }

                // bottom line
                yield return new Rectangle(0, right.LineIndex * lineSpacing, rightCaretX, lineSpacing);
            }
        }

        public bool ReceivingInput { get; set; }
        public int SelectableMargin { get; set; }
        public int TextSize { get; set; }

        public int ScrollOffset => _scrolledOffset;
        public string Text => _text.ToString();
        public IEnumerable<string> Lines
        {
            get
            {
                foreach (var info in _lines) yield return info.Text;
            }
        }
        public Point CaretPosition
        {
            get
            {
                LineInfo line = _lines[_caret.LineIndex];
                return new Point((int)MeasureWidth(line.Text.Substring(0, _caret.Index - line.StartIndex)),
                                 _caret.LineIndex * FontService.Regular.GetLineSpacing(TextSize) - _scrolledOffset);
            }
        }

        public override Rectangle Boundaries 
        {
            set
            {
                base.Boundaries = value;
                RebuildLayout();
            }
        }

        private struct LineInfo
        {
            /// <summary>Index into the raw text where this visual line's text begins.</summary>
            public int StartIndex;
            /// <summary>The visual text of this line (never contains '\n').</summary>
            public string Text;
        }

        private struct CaretInfo
        {
            public int Index;
            public int LineIndex; // for visual, as last index of a line shares the same index with the zero-index above the line
        }

        public MultiLineTextBox()
        {
            _lines = new List<LineInfo>();
            _text = new StringBuilder();
        }

        public void SetText(string value)
        {
            _text.Clear();
            _caret = new CaretInfo();

            InsertText(value);
        }

        public void InsertText(string str)
        {
            _text.Insert(_caret.Index, str);
            RebuildLayout();

            UpdateCaret(_caret.Index + str.Length);
        }

        public void Backspace()
        {
            if (_caret.Index == 0)
            {
                return;
            }

            if (IsSelectionBoxActive)
            {
                int left = _selectedStartCaretPosition.Index;
                int right = _selectedEndCaretPosition.Index;

                if (left > right)
                {
                    (left, right) = (right, left);
                }

                _text.Remove(left, right - left);
                RebuildLayout();

                UpdateCaret(left);
            }
            else
            {
                _text.Remove(_caret.Index - 1, 1);
                RebuildLayout();

                UpdateCaret(_caret.Index - 1);
            }
        }

        public void KeyInput(Keys key)
        {
            switch (key)
            {
                case Keys.Up:

                    if (IsSelectionBoxActive)
                    {
                        UpdateCaret(Math.Min(_selectedStartCaretPosition.Index, _selectedEndCaretPosition.Index));
                    }

                    MoveCaretVertical(toUp: true);

                    break;

                case Keys.Down:

                    if (IsSelectionBoxActive)
                    {
                        UpdateCaret(Math.Max(_selectedStartCaretPosition.Index, _selectedEndCaretPosition.Index));
                    }

                    MoveCaretVertical(toUp: false);

                    break;

                case Keys.Left:

                    if (IsSelectionBoxActive)
                    {
                        UpdateCaret(Math.Min(_selectedStartCaretPosition.Index, _selectedEndCaretPosition.Index));
                    }
                    else
                    {
                        MoveCaretHorizontal(toLeft: true);
                    }

                    break;

                case Keys.Right:

                    if (IsSelectionBoxActive)
                    {
                        UpdateCaret(Math.Max(_selectedStartCaretPosition.Index, _selectedEndCaretPosition.Index));
                    }
                    else
                    {
                        MoveCaretHorizontal(toLeft: false);
                    }

                    break;

                case Keys.Back:
                    Backspace();
                    break;

                case Keys.Enter:
                    InsertText("\n");
                    break;
            }
        }

        public void OnSelect(PopWindowMouseEventArgs e)
        {
            ReceivingInput = true;

            if (_lines.Count == 0) return;

            _caret = GetCaretInfo(e.MousePosition);
            _desiredCaretX = GetCaretX(_caret);

            AlignScrollOffset(_caret);
            DiscardSelection();
        }

        private void UpdateCaret(int value)
        {
            _caret = new CaretInfo 
            {
                Index = value,
                LineIndex = ResolveDefaultLine(value)
            };

            _desiredCaretX = GetCaretX(_caret);

            AlignScrollOffset(_caret);
            DiscardSelection();
        }

        private CaretInfo GetCaretInfo(Point mousePosition)
        {
            Point mouseInBoundaries = mousePosition - this.Boundaries.Location + new Point(0, _scrolledOffset);

            int lineIndex = mouseInBoundaries.Y / FontService.Regular.GetLineSpacing(TextSize);
            lineIndex = Math.Clamp(lineIndex, 0, _lines.Count - 1);

            LineInfo line = _lines[lineIndex];
            int column = FindNearestColumn(line.Text, mouseInBoundaries.X);

            return new CaretInfo { Index = line.StartIndex + column, LineIndex = lineIndex };
        }

        private void MoveCaretHorizontal(bool toLeft)
        {
            LineInfo line = _lines[_caret.LineIndex];
            int column = _caret.Index - line.StartIndex;

            if (toLeft && column == line.Text.Length && IsWrapBoundaryAfter(_caret.LineIndex))
            {
                // caret is at the end of a wrapped line, increase caret line by one, without consuming a character
                _caret.LineIndex++;
            }
            else if (!toLeft && column == 0 && IsWrapBoundaryAfter(_caret.LineIndex - 1))
            {
                // caret is at the beginning after a wrapped line, decrease line index by one without consuming a character
                _caret.LineIndex--;
            }
            else
            {
                _caret.Index = Math.Clamp(_caret.Index + (toLeft ? -1 : 1), 0, _text.Length);
                _caret.LineIndex = ResolveDefaultLine(_caret.Index);
            }

            _desiredCaretX = GetCaretX(_caret);

            AlignScrollOffset(_caret);
            DiscardSelection();
        }

        private void MoveCaretVertical(bool toUp)
        {
            int lineIndex = _caret.LineIndex;

            if (toUp && lineIndex == 0)
            {
                // if caret on first line, goes to position zero
                _caret.Index = 0;
                _caret.LineIndex = 0;
                _desiredCaretX = GetCaretX(_caret);
                return;
            }

            if (!toUp && lineIndex == _lines.Count - 1)
            {
                // if caret on last line, goes to max position
                _caret.Index = _text.Length;
                _caret.LineIndex = lineIndex;
                _desiredCaretX = GetCaretX(_caret);
                return;
            }

            int targetLineIndex = lineIndex + (toUp ? -1 : 1);
            LineInfo targetLine = _lines[targetLineIndex];
            int column = FindNearestColumn(targetLine.Text, _desiredCaretX);

            _caret.Index = targetLine.StartIndex + column;
            _caret.LineIndex = targetLineIndex;

            AlignScrollOffset(_caret);
            DiscardSelection();
        }

        private float GetCaretX(CaretInfo caretInfo)
        {
            int column = caretInfo.Index - _lines[caretInfo.LineIndex].StartIndex;
            return MeasureWidth(_lines[caretInfo.LineIndex].Text.Substring(0, column));
        }

        /// <summary>Checks whether given line and next line share a boundary (doesn't have '\n' in between them).</summary>
        /// <returns>true if next line of given line has a boundary (is wrap of other one); otherwise false.</returns>
        private bool IsWrapBoundaryAfter(int lineIndex)
        {
            return lineIndex >= 0 && lineIndex < _lines.Count - 1 &&
                   _lines[lineIndex + 1].StartIndex == _lines[lineIndex].StartIndex + _lines[lineIndex].Text.Length;
        }

        /// <summary>Gets the line caret should be placed. In cases of wrap lines (sharing same boundary at the end-start of lines),
        /// this method returns the upper line.</summary>
        /// <returns>The line caret should be placed in.</returns>
        private int ResolveDefaultLine(int caretIndex)
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                int start = _lines[i].StartIndex;
                int end = start + _lines[i].Text.Length; // inclusive

                if (caretIndex >= start && caretIndex <= end)
                    return i;
            }

            return _lines.Count - 1; // fallback, shouldn't happen
        }

        private float MeasureWidth(string text)
        {
            return string.Empty == text ? 0f : FontService.Regular.MeasureWidth(TextSize, text);
        }

        private int FindNearestColumn(string lineText, float targetX)
        {
            int bestColumn = 0;
            float bestDist = float.MaxValue;

            for (int c = 0; c <= lineText.Length; c++)
            {
                float x = MeasureWidth(lineText.Substring(0, c));
                float dist = Math.Abs(x - targetX);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestColumn = c;
                }
            }

            return bestColumn;
        }

        private void RebuildLayout()
        {
            _lines.Clear();
            string full = _text.ToString();

            int paragraphStart = 0;
            for (int i = 0; i <= full.Length; i++)
            {
                if (i == full.Length || full[i] == '\n')
                {
                    string paragraph = full[paragraphStart..i];
                    WrapParagraph(paragraph, paragraphStart);
                    paragraphStart = i + 1;
                }
            }

            if (_lines.Count == 0)
                _lines.Add(new LineInfo { StartIndex = 0, Text = "" });

            UpdateScroll(0);
        }

        /// <summary>
        /// Word-wraps a single paragraph (no '\n' inside it) into one or more visual lines,
        /// preserving runs of consecutive spaces exactly as typed.
        /// </summary>
        private void WrapParagraph(string paragraph, int paragraphStartIndex)
        {
            float maxWidth = Boundaries.Width;

            if (paragraph.Length == 0)
            {
                _lines.Add(new LineInfo { StartIndex = paragraphStartIndex, Text = "" });
                return;
            }

            int lineStart = 0;      // index within `paragraph` where the current line begins
            int lastSpaceIndex = -1; // last space seen within the current line (within `paragraph`)

            int idx = 0;
            while (idx < paragraph.Length)
            {
                char c = paragraph[idx];
                if (c == ' ')
                    lastSpaceIndex = idx;

                float width = MeasureWidth(paragraph.Substring(lineStart, idx - lineStart + 1));

                if (width > maxWidth)
                {
                    if (lastSpaceIndex >= lineStart)
                    {
                        // Break after the last space; the space stays at the end of this line.
                        string lineText = paragraph.Substring(lineStart, lastSpaceIndex - lineStart + 1);
                        _lines.Add(new LineInfo { StartIndex = paragraphStartIndex + lineStart, Text = lineText });

                        lineStart = lastSpaceIndex + 1;
                        lastSpaceIndex = -1;
                        idx = lineStart - 1; // reprocess from the new line start
                    }
                    else if (idx > lineStart)
                    {
                        // No space to break at: hard-break before this character.
                        string lineText = paragraph.Substring(lineStart, idx - lineStart);
                        _lines.Add(new LineInfo { StartIndex = paragraphStartIndex + lineStart, Text = lineText });

                        lineStart = idx;
                        idx = lineStart - 1; // reprocess from the new line start
                    }
                    // else: a single character is already wider than the box; keep it
                    // on the line anyway rather than looping forever.
                }

                idx++;
            }

            string remaining = paragraph.Substring(lineStart);
            _lines.Add(new LineInfo { StartIndex = paragraphStartIndex + lineStart, Text = remaining });
        }

        private void UpdateScroll(int value)
        {
            int maxScrollableDistance = _lines.Count * FontService.Regular.GetLineSpacing(TextSize) - this.Boundaries.Height;
            if (maxScrollableDistance <= 0)
            {
                _scrolledOffset = 0;
                return;
            }

            _scrolledOffset = Math.Clamp(_scrolledOffset + value, 0, maxScrollableDistance);
        }

        /// aligns scroll offset to be sure caret is always visible
        private void AlignScrollOffset(CaretInfo caretInfo)
        {
            int lineSpacing = FontService.Regular.GetLineSpacing(TextSize);
            int minScroll = caretInfo.LineIndex * lineSpacing;
            
            if (_scrolledOffset > minScroll)
            {
                _scrolledOffset = minScroll;
                return;
            }

            int maxScroll = minScroll + lineSpacing - this.Boundaries.Height;
            if (_scrolledOffset < maxScroll)
            {
                _scrolledOffset = maxScroll;
                return;
            }
        }

        public bool CanBeSelected(Point point)
        {
            return SelectableArea.Contains(point);
        }

        public bool CanBeHeld(Point point)
        {
            return SelectableArea.Contains(point);
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

        public bool CanReceiveWheelChangeInput(Point point)
        {
            return SelectableArea.Contains(point);
        }

        public void OnMouseVerticalScrollChangeReceive(PopWindowMouseEventArgs e, int change)
        {
            UpdateScroll(-change / 6);
        }

        public void OnMouseHorizontalScrollChangeReceive(PopWindowMouseEventArgs e, int change)
        {
        }

        public override void Update()
        {
            // placed in here since hold event raised when mouse position changed / mouse button state changed, and not every frames
            if (_isHeld)
            {
                _selectedEndCaretPosition = GetCaretInfo(_lastHeldMousePosition);
                _selectedEndCaretX = (int)GetCaretX(_selectedEndCaretPosition);

                AlignScrollOffset(_selectedEndCaretPosition);
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

                _selectedStartCaretPosition = GetCaretInfo(e.MousePosition);
                _selectedStartCaretX = (int)GetCaretX(_selectedStartCaretPosition);

                DiscardSelection();
            }
            else
            {
                // if mouse left outside of boundaries, don't update its last position
                if (!SelectableArea.Contains(e.MousePosition))
                {
                    return;
                }

                _selectedEndCaretPosition = GetCaretInfo(e.MousePosition);
                _selectedEndCaretX = (int)GetCaretX(_selectedEndCaretPosition);
            }
        }

        private void DiscardSelection()
        {
            _selectedEndCaretPosition = _selectedStartCaretPosition;
            _selectedEndCaretX = _selectedStartCaretX;
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

            int left = _selectedStartCaretPosition.Index;
            int right = _selectedEndCaretPosition.Index;

            if (left > right)
            {
                (left, right) = (right, left);
            }

            return _text.ToString(left, right - left);
        }
    }
}
