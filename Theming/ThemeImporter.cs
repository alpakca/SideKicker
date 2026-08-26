using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

#nullable enable

namespace SideKicker.Theming
{
    internal static class ThemeImporter
    {
        private static readonly string ThemesDirectoryPath = Path.Combine(Program.AssetsFolderPath, "themes");
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            Converters = { new ColorConverter() }
        };

        private static readonly Dictionary<string, Theme> _loadedThemes = new Dictionary<string, Theme>();

        public static IEnumerable<Theme> LoadedThemes => _loadedThemes.Values;

        public static void RefreshLoaded()
        {
            string[] fileNames = Directory.GetFiles(ThemesDirectoryPath);
            foreach (var filePath in fileNames)
            {
                if (_loadedThemes.ContainsKey(filePath))
                {
                    continue;
                }

                try
                {
                    Theme loaded = JsonSerializer.Deserialize<Theme>(File.ReadAllText(filePath), _options)!;
                    _loadedThemes.Add(filePath, loaded);
                }
                catch (JsonException e)
                {

                }
            }

            List<string> toRemove = new List<string>();
            foreach (var loadedFilePath in _loadedThemes.Keys)
            {
                if (!fileNames.Contains(loadedFilePath))
                {
                    toRemove.Add(loadedFilePath);
                }
            }

            foreach (var path in toRemove)
            {
                _loadedThemes.Remove(path);
            }
        }
    }
}
