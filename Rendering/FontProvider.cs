using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Text;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Rendering
{
    internal class FontProvider
    {
        private readonly Dictionary<int, SpriteFont> _fonts;

        public event Action FontChanged;
        
        public FontProvider(Dictionary<int, SpriteFont> fonts)
        {
            _fonts = fonts;
        }

        public void DrawText(int size, string text, Vector2 position, Color color)
        {
            DrawHelper.Batch.DrawString(_fonts[size], text, position, color);
        }

        public void DrawCenteredText(int size, string text, Rectangle boundaries, Color color)
        {
            Vector2 textSize = MeasureSize(size, text);
            DrawText(size, text, new Vector2(boundaries.X + (int)((boundaries.Width - textSize.X) / 2), boundaries.Y + (int)((boundaries.Height - textSize.Y) / 2)), color);
        }

        public void DrawVerticallyCenteredText(int size, string text, Vector2 position, int height, Color color)
        {
            position = new Vector2(position.X, position.Y + (height - _fonts[size].LineSpacing) / 2);
            DrawText(size, text, position, color);
        }

        public float MeasureWidth(int size, string text)
        {
            return _fonts[size].MeasureString(text).X;
        }

        public float MeasureWidth(int size, StringBuilder builder)
        {
            return _fonts[size].MeasureString(builder).X;
        }

        public Vector2 MeasureSize(int size, string text)
        {
            return _fonts[size].MeasureString(text);
        }

        public int GetLineSpacing(int size)
        {
            return _fonts[size].LineSpacing;
        }

        public void UpdateProcesserFont(TextProcesser processer, int size)
        {
            processer.Font = _fonts[size];
        }

        public bool CanCharacterResolved(int size, char ch)
        {
            return _fonts[size].Characters.Contains(ch);
        }
    }
}
