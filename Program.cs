using System;

namespace SideKicker
{
    public class Program
    {
        public static string RootPath { get; private set; }

        public static void Main(string[] args)
        {
            RootPath = AppContext.BaseDirectory;

            using var game = new Game1();
            game.Run();
        }
    }
}