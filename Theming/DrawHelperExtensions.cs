using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Theming
{
    internal static class DrawHelperExtensions
    {
        extension (DrawHelper)
        {
            // TODO: implement actual circular border drawing
            public static void DrawBorderedBackground(Rectangle bounds, Color borderColor, Color fillColor, float borderThickness)
            {
                DrawHelper.DrawBackground(bounds, borderColor);
                DrawHelper.DrawBackground(new Rectangle(bounds.X + (int)borderThickness, bounds.Y + (int)borderThickness,
                                                        bounds.Width - (int)(borderThickness * 2), bounds.Height - (int)(borderThickness * 2)),
                                          fillColor);
            }

            public static void DrawTextureCenteralized(Rectangle bounds, Texture2D texture)
            {
                DrawHelper.Batch.Draw(texture, new Vector2(bounds.X + (bounds.Width - texture.Width) / 2,
                                                           bounds.Y + (bounds.Height - texture.Height) / 2), Color.White);
            }
        }
    }
}
