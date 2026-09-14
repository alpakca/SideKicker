using System;
using System.IO;
using System.Text.Json;

namespace SideKicker.Panels.Timers
{
    internal static class RecentTimeLoader
    {
        private static readonly string SaveFilePath = Path.Combine(Program.SavesFolderPath, "recenttimers.json");

        public static void Load(RecentTimeButton[] buttons)
        {
            TimeSpan[] values;
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                values = JsonSerializer.Deserialize<TimeSpan[]>(json);
            }
            catch (IOException)
            {
                values = [TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)]; // default durations
            }
            catch (JsonException)
            {
                values = [TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5)]; // default durations
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetNew(values[i]);
            }
        }

        public static void Save(RecentTimeButton[] buttons)
        {
            TimeSpan[] values = new TimeSpan[buttons.Length];
            for (int i = 0; i < buttons.Length; i++)
            {
                values[i] = buttons[i].Timer;
            }

            string json = JsonSerializer.Serialize(values);
            File.WriteAllText(SaveFilePath, json);
        }
    }
}
