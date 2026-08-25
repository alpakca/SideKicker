using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PopUpUI.Elements;
using PopUpUI.Input;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using SideKicker.Theming;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels
{
    internal class LinearTextBox : PopElement, ISelectable, IKeyReceiver, ITextReceiver, IDisposable
    {
        private int _caretPosition;
        private int _textOffset;
        private string _text;
        private RenderTarget2D _target;

        private readonly TextureDrawInfo _drawInfo = new TextureDrawInfo();

        private Rectangle SelectableArea => new Rectangle(Boundaries.X - SelectableMargin, Boundaries.Y - SelectableMargin, 
                                                          Boundaries.Width + 2 * SelectableMargin, Boundaries.Height + 2 * SelectableMargin);

        SelectablePacket ISelectable.SelectablePacket { get; } = new SelectablePacket();
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

        public override Rectangle Boundaries 
        {
            set
            {
                if (value.Width != 0 && value.Height != 0)
                {
                    base.Boundaries = value;
                    _drawInfo.DestinationArea = value;

                    _target?.Dispose();
                    _target = DrawHelper.CreateRenderTarget(value.Width, value.Height);
                    _drawInfo.Texture = _target;
                }
            }
        }

        public void Dispose()
        {
            _target?.Dispose();
        }

        public override void RenderUpdate()
        {
            DrawHelper.SetBeginBatch(_target);

            FontService.Regular.DrawText(TextSize, _text, new Vector2(_textOffset), ThemeService.CurrentTheme.GetColor(ColorRegistry.TextColorId));

            if (ReceivingInput)
            {
                const int CaretWidth = 2;

                int caretX = (int)FontService.Regular.MeasureWidth(TextSize, _text[.._caretPosition]) + _textOffset;
                caretX = Math.Clamp(caretX, 0, this.Boundaries.Width - CaretWidth);

                DrawHelper.DrawBackground(new Rectangle(caretX, 0, CaretWidth, FontService.Regular.GetLineSpacing(TextSize)),
                                          ThemeService.CurrentTheme.GetColor(ColorRegistry.TextColorId));
            }

            DrawHelper.EndBatch();

            base.RenderUpdate();
        }

        public override IEnumerable<UIDrawInfo> GetDrawInfos()
        {
            return base.GetDrawInfos();
        }

        public void InsertText(string str)
        {
            _text = _text.Insert(_caretPosition, str);
            UpdateCaret(_caretPosition + str.Length);
        }

        public void Backspace()
        {
            if (_caretPosition == 0 || _text.Length == 0)
            {
                return;
            }

            _text = _text[..(_caretPosition - 1)] + _text[_caretPosition..];
            UpdateCaret(_caretPosition - 1);
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

            Point inTextBoxPosition = e.MousePosition - SelectableArea.Location;

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
            int width = (int)FontService.Regular.MeasureWidth(TextSize, _text[.._caretPosition]);

            if (_textOffset + width > this.Boundaries.Width)
            {
                _textOffset = this.Boundaries.Width - width;
            }
            else if (_textOffset + width < 0)
            {
                _textOffset = -width;
            }
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
