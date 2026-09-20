using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Rendering
{
    internal static class DrawHelperExtensions
    {
        private static readonly RasterizerState _scissorRasterizer = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace, ScissorTestEnable = true };
        private static readonly BatchDrawOptions _alphaBlendDrawOptions = new BatchDrawOptions { BlendState = BlendState.AlphaBlend };
        private static readonly BatchDrawOptions _alphaBlendScissoredOptions = new BatchDrawOptions { BlendState = BlendState.AlphaBlend, RasterizerState = _scissorRasterizer };

        extension (DrawHelper)
        {
            public static RasterizerState ScissorRasterizer => _scissorRasterizer;
            public static BatchDrawOptions AlphaBlendDrawOptions => _alphaBlendDrawOptions;
            public static BatchDrawOptions AlphaScissoredDrawOptions => _alphaBlendScissoredOptions;

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
