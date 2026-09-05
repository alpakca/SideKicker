using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace SideKicker.Native
{
    internal static class GlobalHitRegionManager
    {
        private const int RGN_OR = 2;

        private const int GWL_EXSTYLE = -20;

        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;

        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;

        private static IntPtr _windowHandle;

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        private static extern int CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, int fnCombineMode);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        public static void Initialize(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            SetTransparent();
            SetWindowFlags();
        }

        private static void SetTransparent()
        {
            MARGINS margin = new MARGINS
            {
                cxLeftWidth = -1,
                cxRightWidth = -1,
                cyBottomHeight = -1,
                cyTopHeight = -1
            };
            DwmExtendFrameIntoClientArea(_windowHandle, ref margin);
        }

        private static void SetWindowFlags()
        {
            IntPtr exStyle = GetWindowLongPtr64(_windowHandle, GWL_EXSTYLE);
            exStyle |= WS_EX_TOOLWINDOW;
            exStyle &= ~WS_EX_APPWINDOW;
            SetWindowLongPtr64(_windowHandle, GWL_EXSTYLE, exStyle);

            RestoreTopMost();
        }

        public static void RestoreTopMost()
        {
            SetWindowPos(_windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        public static void SetAreas(Rectangle first, Rectangle second)
        {
            IntPtr combined = CreateRectRgn(first.Left, first.Top, first.Right, first.Bottom);
            IntPtr toAdd = CreateRectRgn(second.Left, second.Top, second.Right, second.Bottom);

            CombineRgn(combined, combined, toAdd, RGN_OR);
            DeleteObject(toAdd);

            SetWindowRgn(_windowHandle, combined, bRedraw: false);
        }

        public static void SetArea(Rectangle rect)
        {
            IntPtr rgn = CreateRectRgn(rect.Left, rect.Top, rect.Right, rect.Bottom);
            SetWindowRgn(_windowHandle, rgn, bRedraw: false);
        }
    }
}
