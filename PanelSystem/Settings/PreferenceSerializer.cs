using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace SideKicker.PanelSystem.Settings
{
    internal static class PreferenceSerializer
    {
        private const string FileName = "preferences.json";
        private static readonly string FilePath = Path.Combine(Program.RootPath, FileName);

        public static Preferences Load()
        {
            try
            {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<Preferences>(json);
            }
            catch (IOException)
            {
                return Preferences.Default;
            }
            catch (JsonException)
            {
                return Preferences.Default;
            }
        }

        public static void Save(Preferences preferences)
        {
            string json = JsonSerializer.Serialize(preferences);

            string tempPath = FilePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, FilePath, overwrite: true);
        }
    }
}
