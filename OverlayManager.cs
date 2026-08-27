using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace SideKicker
{
    internal static class OverlayManager
    {
        private const int RGN_OR = 2;

        private const int GWL_EXSTYLE = -20;
        private const int GWL_WNDPROC = -4;

        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;

        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;

        private const int HotKeyId = 1;
        private const uint WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private static IntPtr _windowHandle;
        private static WndProcDelegate? _hookedProc;
        private static IntPtr _originalWndProc;

        public static event Action HotKeyTriggered;

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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

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

        public static void SetOpenCloseHotKey(uint modifiers, uint key)
        {
            _hookedProc = WndProc;

            var newProcPtr = Marshal.GetFunctionPointerForDelegate(_hookedProc);
            _originalWndProc = SetWindowLongPtr64(_windowHandle, GWL_WNDPROC, newProcPtr);

            RegisterHotKey(_windowHandle, HotKeyId, modifiers, key);
        }

        public static void Dispose()
        {
            UnregisterHotKey(_windowHandle, HotKeyId);
        }

        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_HOTKEY)
            {
                HotKeyTriggered?.Invoke();
            }

            return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
        }
    }
}
