using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SideKicker.Panels.PinnedCommands
{
    internal static class CommandSerializer
    {
        private const string SaveFileName = "pinnedcommands.json";
        private static readonly string SaveFilePath = Path.Combine(Program.SavesFolderPath, SaveFileName);

        public static SavedCommand[] Load()
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                return JsonSerializer.Deserialize<SavedCommand[]>(json); 
            }
            catch (IOException)
            {
                string emptyJson = JsonSerializer.Serialize(Array.Empty<SavedCommand>());
                File.WriteAllText(SaveFilePath, emptyJson);
                return [];
            }
            catch (JsonException)
            {
                string emptyJson = JsonSerializer.Serialize(Array.Empty<SavedCommand>());
                File.WriteAllText(SaveFilePath, emptyJson);
                return [];
            }
        }

        public static async Task SaveAsync(IEnumerable<SavedCommand> commands)
        {
            string json = JsonSerializer.Serialize(commands);
            await File.WriteAllTextAsync(SaveFilePath, json);
        }

        public static void Save(IEnumerable<SavedCommand> commands)
        {
            string json = JsonSerializer.Serialize(commands);
            File.WriteAllText(SaveFilePath, json);
        }
    }
}
