using System;
using System.IO;

namespace SideKicker
{
    public class Program
    {
        public static string RootPath { get; private set; }
        public static string AssetsFolderPath { get; private set; }
        public static string SavesFolderPath { get; private set; }

        public static void Main(string[] args)
        {
            RootPath = AppContext.BaseDirectory;

#if DEBUG
            RootPath = "C:\\Users\\user\\Desktop\\C#\\SideKicker\\SideKicker";
#endif
            AssetsFolderPath = Path.Combine(RootPath, "assets");
            SavesFolderPath = Path.Combine(RootPath, "saves");

            using var game = new Game1();
            game.Run();
        }
    }
}