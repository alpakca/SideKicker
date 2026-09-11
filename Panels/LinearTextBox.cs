using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Input;
using SideKicker.Theming;
using System;
using System.Text;

namespace SideKicker.Panels
{
    internal class LinearTextBox : PopElement, ISelectable, IKeyReceiver, ITextReceiver
    {
        private int _caretPosition;
        private int _textOffset;
        private int _caretX;
        private string _text = "";

        public Rectangle SelectableArea => new Rectangle(Boundaries.X - SelectableMargin, Boundaries.Y - SelectableMargin, 
                                                         Boundaries.Width + 2 * SelectableMargin, Boundaries.Height + 2 * SelectableMargin);

        SelectablePacket ISelectable.SelectablePacket { get; } = new SelectablePacket { EnabledOutsideSelecting = true };
        KeyReceiverPacket IKeyReceiver.KeyReceiverPacket { get; } = new KeyReceiverPacket();
        TextReceiverPacket ITextReceiver.TextReceiverPacket { get; } = new TextReceiverPacket();

        bool IKeyReceiver.ReceivesKeyInput => ReceivingInput;
        bool ITextReceiver.ReceivesTextInput => ReceivingInput;

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

            _text = _text[..(_caretPosition - 1)] + _text[_caretPosition..];
            UpdateCaret(_caretPosition - 1);

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
                    UpdateCaret(_caretPosition - 1);
                    break;

                case Keys.Right:
                    UpdateCaret(_caretPosition + 1);
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

        public void OnSelect(PopWindowMouseEventArgs e)
        {
            ReceivingInput = true;

            if (_text.Length == 0)
            {
                UpdateCaret(0);
                return;
            }

            Point inTextBoxPosition = e.MousePosition - SelectableArea.Location - new Point(_textOffset, 0); // don't forget to include text offset

            // input inside box will be single line, so we can directly pass zero as Y value
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < _text.Length - 1; i++)
            {
                if (FontService.Regular.MeasureWidth(TextSize, builder)
                  + FontService.Regular.MeasureWidth(TextSize, _text[i].ToString()) >= inTextBoxPosition.X)
                {
                    UpdateCaret(i);
                    return;
                }

                builder.Append(_text[i]);
            }

            UpdateCaret(_text.Length);
        }

        private void UpdateCaret(int value)
        {
            _caretPosition = Math.Clamp(value, 0, _text.Length);
            _caretX = (int)FontService.Regular.MeasureWidth(TextSize, _text[.._caretPosition]);

            int previousCharWidth = _caretPosition > 0 ? (int)FontService.Regular.MeasureWidth(TextSize, _text[_caretPosition - 1].ToString())
                                                       : 0;

            if (_textOffset + _caretX + SelectableMargin > this.Boundaries.Width)
            {
                _textOffset = this.Boundaries.Width - _caretX - SelectableMargin;
            }
            else if (_textOffset + _caretX - previousCharWidth < 0)
            {
                _textOffset = previousCharWidth - _caretX;
            }

            _textOffset = Math.Min(0, _textOffset);
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
    }
}
