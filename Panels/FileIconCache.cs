using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Panels.PinnedCommands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SideKicker.Panels
{
    internal static class FileIconCache
    {
        private const string ExplorerFilePath = @"C:\Windows\explorer.exe";
        private const string ExeFileExtension = ".exe";

        private static readonly MemoryStream _stream = new MemoryStream();
        private static Texture2D _explorerIcon;

        private static readonly Dictionary<string, Texture2D> _savedIcons = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Texture2D> _fileTypeIcons = new Dictionary<string, Texture2D>();

        public static void AddIcons(IEnumerable<SavedCommand> savedCommands)
        {
            foreach (var command in savedCommands)
            {
                ReadOnlySpan<char> absolutePath = Path.IsPathRooted(command.FileName) ? command.FileName
                                                                                      : Path.Combine(command.PathToRunCommand, command.FileName);
                TryAddIcon(absolutePath, command.FileName);
            }
        }

        public static void AddIcons(IEnumerable<string> absolutePaths)
        {
            foreach (var path in absolutePaths)
            {
                TryAddIcon(path, path);
            }
        }

        public static void TryAddIcon(ReadOnlySpan<char> filePath, string toSave)
        {
            if (_savedIcons.ContainsKey(toSave))
            {
                return;
            }

            ReadOnlySpan<char> extension = Path.GetExtension(filePath);

            // no icon was set for this file, but was set for same extension.
            // ".exe" can't exist here as they are only added to _savedIcons dictionary
            if (_fileTypeIcons.TryGetAlternateLookup<ReadOnlySpan<char>>(out var extensionLookup) &&
                extensionLookup.TryGetValue(extension, out Texture2D icon))
            {
                _savedIcons[toSave] = icon;
                return;
            }

            RefreshIcon(filePath, toSave);
        }

        public static void RefreshIcon(ReadOnlySpan<char> filePath, string toSave)
        {
            string filePathStr = new string(filePath);

            // special case for directories: use explorer icon
            if (Directory.Exists(filePathStr))
            {
                _explorerIcon ??= GetIconTexture(Icon.ExtractAssociatedIcon(ExplorerFilePath));

                _savedIcons[toSave] = _explorerIcon;
            }
            else
            {
                Icon icon = Icon.ExtractAssociatedIcon(filePathStr);
                if (icon == null) return;

                Texture2D iconTexture = GetIconTexture(icon);
                string extension = new string(Path.GetExtension(filePath));

                // special case for executables: they have different icons with same .exe extension,
                // so don't save them for type icons
                if (extension != ExeFileExtension)
                {
                    _fileTypeIcons[extension] = iconTexture;
                }

                _savedIcons[toSave] = iconTexture;
            }
        }
        
        public static bool TryGetIcon(string savedName, out Texture2D icon)
        {
            return _savedIcons.TryGetValue(savedName, out icon);
        }

        private static Texture2D GetIconTexture(Icon icon)
        {
            _stream.SetLength(0);

            icon.ToBitmap().Save(_stream, System.Drawing.Imaging.ImageFormat.Png);
            _stream.Seek(0, SeekOrigin.Begin);

            return Texture2D.FromStream(DrawHelper.Device, _stream);
        }
    }
}
