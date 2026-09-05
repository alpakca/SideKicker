using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SideKicker.Panels
{
    internal static class PanelContentSerializer<T>
    {
        private static string _saveFileName;
        private static string _saveFilePath;

        public static string SaveFileName
        {
            get => _saveFileName;
            set
            {
                _saveFileName = value;
                _saveFilePath = Path.Combine(Program.SavesFolderPath, value);
            }
        }

        public static T[] Load()
        {
            try
            {
                string json = File.ReadAllText(_saveFilePath);
                return JsonSerializer.Deserialize<T[]>(json);
            }
            catch (IOException)
            {
                string emptyJson = JsonSerializer.Serialize(Array.Empty<T>());
                File.WriteAllText(_saveFilePath, emptyJson);
                return [];
            }
            catch (JsonException)
            {
                string emptyJson = JsonSerializer.Serialize(Array.Empty<T>());
                File.WriteAllText(_saveFilePath, emptyJson);
                return [];
            }
        }

        public static async Task SaveAsync(IEnumerable<T> commands)
        {
            string json = JsonSerializer.Serialize(commands);
            await File.WriteAllTextAsync(_saveFilePath, json);
        }

        public static void Save(IEnumerable<T> commands)
        {
            string json = JsonSerializer.Serialize(commands);
            File.WriteAllText(_saveFilePath, json);
        }
    }
}
