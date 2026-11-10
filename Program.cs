using System;
using System.IO;
using System.Threading;

namespace SideKicker
{
    public class Program
    {
        private const string AppGUID = "f07f819e-6118-4b15-8e27-e651c96c2bdd";

        public static string RootPath { get; private set; }
        public static string AssetsFolderPath { get; private set; }
        public static string SavesFolderPath { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            Mutex mutex = new Mutex(initiallyOwned: true, "Global\\" + AppGUID, out bool createdNew);
            if (!createdNew)
            {
                mutex?.Dispose();
                return;
            }

            RootPath = AppContext.BaseDirectory;

            AssetsFolderPath = Path.Combine(RootPath, "assets");
            SavesFolderPath = Path.Combine(RootPath, "saves");

            using var game = new Game1();
            game.Run();

            mutex?.Dispose();
        }
    }
}