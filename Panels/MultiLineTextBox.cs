using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Input;
using SideKicker.Theming;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels
{
    internal class MultiLineTextBox : PopElement, ISelectable, IKeyReceiver, ITextReceiver, IMouseWheelReceiver
    {
        private readonly StringBuilder _text;
        private readonly List<LineInfo> _lines;

        private int _caret;
        private int _caretLine; // for visual, as last index of a line shares the same index with the zero-index above the line

        private float _desiredCaretX;
        private int _scrolledOffset;

        SelectablePacket ISelectable.SelectablePacket { get; } = new SelectablePacket { EnabledOutsideSelecting = true };
        KeyReceiverPacket IKeyReceiver.KeyReceiverPacket { get; } = new KeyReceiverPacket();
        TextReceiverPacket ITextReceiver.TextReceiverPacket { get; } = new TextReceiverPacket();
        MouseWheelReceiverPacket IMouseWheelReceiver.MouseWheelReceiverPacket { get; } = new MouseWheelReceiverPacket();

        bool IKeyReceiver.ReceivesKeyInput => ReceivingInput;
        bool ITextReceiver.ReceivesTextInput => ReceivingInput;

        public Rectangle SelectableArea => new Rectangle(Boundaries.X - SelectableMargin, Boundaries.Y - SelectableMargin,
                                                         Boundaries.Width + 2 * SelectableMargin, Boundaries.Height + 2 * SelectableMargin);

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
                LineInfo line = _lines[_caretLine];
                return new Point((int)MeasureWidth(line.Text.Substring(0, _caret - line.StartIndex)),
                                 _caretLine * FontService.Regular.GetLineSpacing(TextSize) - _scrolledOffset);
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

        public MultiLineTextBox()
        {
            _lines = new List<LineInfo>();
            _text = new StringBuilder();
        }

        public void SetText(string value)
        {
            _text.Clear();
            _caret = 0;

            InsertText(value);
        }

        public void InsertText(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            _text.Insert(_caret, str);
            RebuildLayout();

            _caret += str.Length;
            _caretLine = ResolveDefaultLine(_caret);
            _desiredCaretX = GetCaretX(_caret, _caretLine);
        }

        public void Backspace()
        {
            if (_caret == 0)
            {
                return;
            }

            _text.Remove(_caret - 1, 1);
            RebuildLayout();

            _caret--;
            _caretLine = ResolveDefaultLine(_caret);
            _desiredCaretX = GetCaretX(_caret, _caretLine);
        }

        public void KeyInput(Keys key)
        {
            switch (key)
            {
                case Keys.Up:
                    MoveCaretVertical(-1);
                    break;

                case Keys.Down:
                    MoveCaretVertical(+1);
                    break;

                case Keys.Left:
                    MoveCaretHorizontal(-1);
                    break;

                case Keys.Right:
                    MoveCaretHorizontal(+1);
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

            Point mouseInBoundaries = e.MousePosition - this.Boundaries.Location + new Point(0, _scrolledOffset);

            int lineIndex = (int)(mouseInBoundaries.Y / FontService.Regular.GetLineSpacing(TextSize));
            lineIndex = Math.Clamp(lineIndex, 0, _lines.Count - 1);

            LineInfo line = _lines[lineIndex];
            int column = FindNearestColumn(line.Text, mouseInBoundaries.X);

            _caret = line.StartIndex + column;
            _caretLine = lineIndex;
            _desiredCaretX = GetCaretX(_caret, _caretLine);
        }

        private void MoveCaretHorizontal(int amount)
        {
            LineInfo line = _lines[_caretLine];
            int column = _caret - line.StartIndex;

            if (amount > 0 && column == line.Text.Length && IsWrapBoundaryAfter(_caretLine))
            {
                // caret is at the end of a wrapped line, increase caret line by one, without consuming a character
                _caretLine++;
            }
            else if (amount < 0 && column == 0 && IsWrapBoundaryAfter(_caretLine - 1))
            {
                // caret is at the beginning after a wrapped line, decrease line index by one without consuming a character
                _caretLine--;
            }
            else
            {
                _caret = Math.Clamp(_caret + amount, 0, _text.Length);
                _caretLine = ResolveDefaultLine(_caret);
            }

            _desiredCaretX = GetCaretX(_caret, _caretLine);
        }

        private void MoveCaretVertical(int amount)
        {
            int lineIndex = _caretLine;

            if (amount < 0 && lineIndex == 0)
            {
                // if caret on first line, goes to position zero
                _caret = 0;
                _caretLine = 0;
                _desiredCaretX = GetCaretX(_caret, _caretLine);
                return;
            }

            if (amount > 0 && lineIndex == _lines.Count - 1)
            {
                // if caret on last line, goes to max position
                _caret = _text.Length;
                _caretLine = lineIndex;
                _desiredCaretX = GetCaretX(_caret, _caretLine);
                return;
            }

            int targetLineIndex = lineIndex + amount;
            LineInfo targetLine = _lines[targetLineIndex];
            int column = FindNearestColumn(targetLine.Text, _desiredCaretX);

            _caret = targetLine.StartIndex + column;
            _caretLine = targetLineIndex;
        }

        private float GetCaretX(int caretIndex, int lineIndex)
        {
            int column = caretIndex - _lines[lineIndex].StartIndex;
            return MeasureWidth(_lines[lineIndex].Text.Substring(0, column));
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

        public bool CanBeSelected(Point point)
        {
            return SelectableArea.Contains(point);
        }

        public void OnSelectingOutside(PopWindowMouseEventArgs e)
        {
            ReceivingInput = false;
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
    }
}
