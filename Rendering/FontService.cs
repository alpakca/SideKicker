using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System.Collections.Generic;

namespace SideKicker.Rendering
{
    internal static class FontService
    {
        public static Dictionary<int, SpriteFont> _fonts = new Dictionary<int, SpriteFont>
        {
            { 12, ContentAssistant.FromLoaded<SpriteFont>("Font_SegoeUI_12") },
            { 16, ContentAssistant.FromLoaded<SpriteFont>("Font_SegoeUI_16") },
            { 24, ContentAssistant.FromLoaded<SpriteFont>("Font_SegoeUI_24") },
            { 20, ContentAssistant.FromLoaded<SpriteFont>("Font_SegoeUI_20") }
        };

        public static void DrawText(int size, string text, Vector2 position, Color color)
        {
            DrawHelper.Batch.DrawString(_fonts[size], text, position, color);
        }

        public static void DrawCenteredText(int size, string text, Rectangle boundaries, Color color)
        {
            Vector2 textSize = MeasureSize(size, text);
            DrawText(size, text, new Vector2(boundaries.X + (int)((boundaries.Width - textSize.X) / 2), boundaries.Y + (int)((boundaries.Height - textSize.Y) / 2)), color);
        }

        public static void DrawVerticallyCenteredText(int size, string text, Vector2 position, int height, Color color)
        {
            position = new Vector2(position.X, position.Y + (height - _fonts[size].LineSpacing) / 2);
            DrawText(size, text, position, color);
        }

        public static float MeasureWidth(int size, string text)
        {
            return _fonts[size].MeasureString(text).X;
        }

        public static Vector2 MeasureSize(int size, string text)
        {
            return _fonts[size].MeasureString(text);
        }

        public static int GetLineSpacing(int size)
        {
            return _fonts[size].LineSpacing;
        }

        public static bool CanCharacterResolved(int size, char ch)
        {
            return _fonts[size].Characters.Contains(ch);
        }
    }
}
