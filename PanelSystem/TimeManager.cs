using Microsoft.Xna.Framework;
using System;

namespace SideKicker.PanelSystem
{
    internal static class TimeManager
    {
        public static int DeltaTime { get; private set; }
        public static TimeSpan TotalGameTime { get; private set; }

        public static void Update(GameTime gameTime)
        {
            DeltaTime = gameTime.ElapsedGameTime.Milliseconds;
            TotalGameTime = gameTime.TotalGameTime;
        }
    }
}
