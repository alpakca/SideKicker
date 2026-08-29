using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Surface;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels
{
    internal static class DrawHelperExtensions
    {
        private static readonly RasterizerState _scissorRasterizer = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace, ScissorTestEnable = true };
        private static readonly BatchDrawOptions _alphaBlendDrawOptions = new BatchDrawOptions { BlendState = BlendState.AlphaBlend };
        private static readonly BatchDrawOptions _alphaBlendScissoredOptions = new BatchDrawOptions { BlendState = BlendState.AlphaBlend, RasterizerState = _scissorRasterizer };

        extension(DrawHelper)
        {
            public static RasterizerState ScissorRasterizer => _scissorRasterizer;
            public static BatchDrawOptions AlphaBlendDrawOptions => _alphaBlendDrawOptions;
            public static BatchDrawOptions AlphaScissoredDrawOptions => _alphaBlendScissoredOptions;
        }
    }
}
