using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SideKicker.Theming
{
    internal static class ThemeService
    {
        public static string AssetsFolderPath { get; } = Path.Combine(Program.RootPath, "assets");

        private static Dictionary<string, Theme> _loadedThemes;

        public static ResolvedTheme CurrentTheme { get; private set; }
        public static event Action ThemeChanged;

        public static void Refresh()
        {
            ThemeImporter.RefreshLoaded();
            _loadedThemes = ThemeImporter.LoadedThemes.ToDictionary(theme => theme.Id);
        }

        public static bool SetTheme(string themeId)
        {
            if (!_loadedThemes.TryGetValue(themeId, out Theme theme))
            {
                return false;
            }

            HashSet<string> visitedBuffer = new HashSet<string>();

            Dictionary<string, Color> colors = new Dictionary<string, Color>();
            foreach (var colorId in ColorRegistry.RegisteredIds)
            {
                colors[colorId] = ResolveColor(theme, colorId, visitedBuffer);
                visitedBuffer.Clear();
            }

            Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
            foreach (var iconId in IconRegistry.RegisteredIds)
            {
                icons[iconId] = ResolveIcon(theme, iconId, visitedBuffer);
                visitedBuffer.Clear();
            }

            CurrentTheme = new ResolvedTheme(theme.Id, colors, icons);
            ThemeChanged?.Invoke();

            return true;
        }

        private static Color ResolveColor(Theme theme, string id, HashSet<string> visitedIds)
        {
            if (!visitedIds.Add(id))
            {
                throw new InvalidOperationException("Circular fallback found on id: " + id);
            }

            if (theme.Colors.TryGetValue(id, out Color color))
            {
                return color;
            }

            ColorDefinition definition = ColorRegistry.Get(id)
                ?? throw new InvalidOperationException("No definition found at the base of fallbacks: " + id);

            if (definition.DefaultValue != null)
            {
                return definition.DefaultValue.Value;
            }

            if (definition.FallbackColorId == null)
            {
                throw new InvalidOperationException("No color was registered at the base of fallbacks: " + id);
            }

            return ResolveColor(theme, definition.FallbackColorId, visitedIds);
        }

        private static Texture2D ResolveIcon(Theme theme, string id, HashSet<string> visitedIds)
        {
            if (!visitedIds.Add(id))
            {
                throw new InvalidOperationException("Circular fallback found on id: " + id);
            }

            if (theme.Icons.TryGetValue(id, out string path))
            {
                return ContentAssistant.FromLoaded<Texture2D>(Path.Combine(AssetsFolderPath, path));
            }

            IconDefinition definition = IconRegistry.Get(id)
                ?? throw new InvalidOperationException("No definition found at the base of fallbacks: " + id);

            if (definition.DefaultPath != null)
            {
                return ContentAssistant.FromLoaded<Texture2D>(Path.Combine(AssetsFolderPath, definition.DefaultPath));
            }

            if (definition.FallbackIconId == null)
            {
                throw new InvalidOperationException("No icon was registered at the base of fallbacks: " + id);
            }

            return ResolveIcon(theme, definition.FallbackIconId, visitedIds);
        }
    }
}
