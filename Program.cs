using System;

namespace SideKicker
{
    public class Program
    {
        public static string RootPath { get; private set; }

        public static void Main(string[] args)
        {
            RootPath = AppContext.BaseDirectory;

#if DEBUG
            RootPath = "C:\\Users\\user\\Desktop\\C#\\SideKicker\\SideKicker";
#endif

            using var game = new Game1();
            game.Run();
        }
    }
}