using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SideKicker.Native
{
    internal class WindowsNativeService : INativeService
    {
        public string DirectoryExplorerSystemPath { get; } = @"C:\Windows\explorer.exe";
        public string ExecutableExtension { get; } = ".exe";

        public event Action WindowHotkeyOpenTriggered;

        public void InitializeWindowFlags(IntPtr windowHandle)
        {
            WindowsHitRegionManager.Initialize(windowHandle);
        }

        public void InitializeWindowHotkeys()
        {
            WindowsKeyboardWatcher.Initialize();
            WindowsKeyboardWatcher.ALTGR_Plus_LessThan_Detected += () => WindowHotkeyOpenTriggered?.Invoke();
        }

        public void Update()
        {
            WindowsKeyboardWatcher.Update();
        }

        public void Dispose()
        {
            WindowsKeyboardWatcher.Dispose();
        }

        public Texture2D? ExtractIconTexture(string path, Stream stream)
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(path);
                if (icon == null) return null;

                stream.SetLength(0);
                icon.ToBitmap().Save(stream, ImageFormat.Png);

                stream.Seek(0, SeekOrigin.Begin);

                return Texture2D.FromStream(DrawHelper.Device, stream);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public void OpenDirectoryExplorer(string path)
        {
            using Process proc = Process.Start("explorer.exe", $"\"{Path.GetDirectoryName(path)}\"");
        }

        public void RestoreTopMost()
        {
            WindowsHitRegionManager.RestoreTopMost();
        }

        public void SetHittableArea(Rectangle area)
        {
            WindowsHitRegionManager.SetArea(area);
        }

        public void SetHittableArea(Rectangle firstArea, Rectangle secondArea)
        {
            WindowsHitRegionManager.SetAreas(firstArea, secondArea);
        }

        public void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        public bool TryGetClipboardText(out string text)
        {
            if (Clipboard.ContainsText())
            {
                text = Clipboard.GetText();
                return true;
            }

            text = null;
            return false;
        }
    }
}
