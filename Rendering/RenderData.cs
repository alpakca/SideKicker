using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Toolkit;
using SideKicker.Panels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SideKicker.Rendering
{
    internal static class RenderData
    {
        private static readonly Dictionary<IconType, TextureInfo> _icons = new Dictionary<IconType, TextureInfo>
        {
            { IconType.PinnedCommandsPanel, new TextureInfo(@"pinnedcommandspanelicon.png") }, 
            { IconType.TimersPanel, new TextureInfo(@"timerspanelicon.png") },
            { IconType.DesktopSearcherPanel, new TextureInfo(@"desktopsearcherpanelicon.png") }, 
            { IconType.SettingsPanel, new TextureInfo(@"settingspanelicon.png") },

            { IconType.EditButton, new TextureInfo(@"editbutton.png") }, 
            { IconType.CloseButton, new TextureInfo(@"closebutton.png") },
            { IconType.ReorderButton, new TextureInfo(@"reorderbutton.png") }, 
            { IconType.CheckedToggleButton, new TextureInfo(@"checkedtogglebutton.png") },

            { IconType.DownArrow, new TextureInfo(@"downarrow.png") }, 
            { IconType.UpArrow, new TextureInfo(@"uparrow.png") },

            { IconType.TimerAddButton, new TextureInfo(@"timeraddbutton.png") }, 
            { IconType.TimerDiscardButton, new TextureInfo(@"timerdiscardbutton.png") },
            { IconType.TimerPauseButton, new TextureInfo(@"timerpausebutton.png") }, 
            { IconType.TimerSaveButton, new TextureInfo(@"timersavebutton.png") }
        };

        public static Color Border => new Color(145, 132, 238, 255);
        public static Color BorderAccent => new Color(14, 14, 14, 255);
        public static Color Background => new Color(27, 27, 27, 255);
        public static Color Surface => new Color(44, 44, 44, 255);

        public static Color HoverOverlay => new Color(104, 104, 104, 72);
        public static Color HeldOverlay => new Color(72, 72, 72, 72);
        public static Color SelectionBox => new Color(59, 122, 186, 72);

        public static Color Text => new Color(255, 255, 255, 255);

        public static Color VerticalBar => new Color(235, 235, 235, 255);

        public static Texture2D GetIcon(IconType type) => LoadIfNotPresent(type);

        public static Texture2D GetPanelIcon(ContextPanelType panelType)
        {
            return GetIcon(panelType switch
            {
                ContextPanelType.PinnedCommands => IconType.PinnedCommandsPanel,
                ContextPanelType.Timers => IconType.TimersPanel,
                ContextPanelType.DesktopSearcher => IconType.DesktopSearcherPanel,
                ContextPanelType.Settings => IconType.SettingsPanel,

                _ => throw new ArgumentException("Given panel type couldn't resolved " + panelType)
            });
        }

        private static Texture2D LoadIfNotPresent(IconType type)
        {
            TextureInfo info = _icons[type];
            info.LoadedTexture ??= Texture2D.FromFile(DrawHelper.Device, info.LoadPath);

            return info.LoadedTexture;
        }

        private class TextureInfo
        {
            public string LoadPath { get; }
            public Texture2D? LoadedTexture { get; set; }

            public TextureInfo(string path)
            {
                LoadPath = Path.Combine(Program.AssetsFolderPath, "panels", path);
            }
        }
    }
}
