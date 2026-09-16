using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SideKicker.Native
{
    internal interface INativeService
    {
        public string DirectoryExplorerSystemPath { get; }
        public string ExecutableExtension { get; }

        public event Action WindowHotkeyOpenTriggered;

        /// <summary>Initializes window's transparency and toolbar-visiblity.</summary>
        public void InitializeWindowFlags(IntPtr windowHandle);
        public void InitializeWindowHotkeys();

        public void Update();
        public void Dispose();

        public void RestoreTopMost();

        public void SetHittableArea(Rectangle area);
        public void SetHittableArea(Rectangle firstArea, Rectangle secondArea);

        public Texture2D? ExtractIconTexture(string path, Stream stream);
        public void OpenDirectoryExplorer(string path);

        public void CopyToClipboard(string text);
        public bool TryGetClipboardText(out string text);
    }
}
