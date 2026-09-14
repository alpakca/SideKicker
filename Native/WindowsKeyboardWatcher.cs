using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

#nullable enable

namespace SideKicker.Native
{
    [SupportedOSPlatform("windows")]
    public static class WindowsKeyboardWatcher
    {
        public static event Action? CtrlCDetected;
        public static event Action? ALTGR_Plus_LessThan_Detected;

        public static int DoubleTapThresholdMs { get; set; } = GetDoubleClickTime();

        public static bool IsRunning => _hookThreadId != 0;

        private const int VK_CKEY = 0x43; // virtual key for C key
        private const int VK_CTRL = 0xA2; // CTRL key
        private const int VK_ALTGR = 0xA5; // ALTGR key
        private const int VK_LESSTHAN = 0xE2; // '<' key

        public static void Initialize()
        {
            if (_thread != null)
                return;

            _readySignal.Reset();
            _thread = new Thread(HookThreadProc)
            {
                IsBackground = true,
                Name = nameof(WindowsKeyboardWatcher)
            };
            _thread.Start();

            // Block briefly until the hook is actually installed (or fails),
            // so callers know for sure what state things are in afterwards.
            _readySignal.Wait();

            if (_hookHandle == IntPtr.Zero)
            {
                _thread = null;
                throw new InvalidOperationException(
                    $"Failed to install the global Tab keyboard hook (Win32 error {_lastError}).");
            }
        }

        public static void Dispose()
        {
            uint threadId = _hookThreadId;
            if (threadId == 0)
                return;

            PostThreadMessage(threadId, WM_QUIT, UIntPtr.Zero, IntPtr.Zero);
            _thread?.Join(1000);
            _thread = null;
        }

        public static void Update()
        {
            int ctrlCCount = Interlocked.Exchange(ref _pendingCtrlCTaps, 0);
            for (int i = 0; i < ctrlCCount; i++)
                CtrlCDetected?.Invoke();

            int altgr_Plus_LessThan_Count = Interlocked.Exchange(ref _pendingALTGR_Plus_LESSTHAN_Taps, 0);
            for (int i = 0; i < altgr_Plus_LessThan_Count; i++)
                ALTGR_Plus_LessThan_Detected?.Invoke();
        }

        // ---------------------------------------------------------------
        // Win32 interop
        // ---------------------------------------------------------------

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_QUIT = 0x0012;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern int GetDoubleClickTime();

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool PostThreadMessage(uint idThread, uint msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        // Keep a strong reference so the GC never collects the delegate
        // while it's registered with SetWindowsHookEx.
        private static readonly LowLevelKeyboardProc _proc = HookCallback;

        private static Thread? _thread;
        private static readonly ManualResetEventSlim _readySignal = new(false);
        private static uint _hookThreadId;
        private static IntPtr _hookHandle = IntPtr.Zero;
        private static int _lastError;

        private static bool _controlCurrentlyDown;
        private static bool _cKeyCurrentlyDown;
        private static bool _altgrKeyCurrentlyDown;
        private static bool _lessThanKeyCurrentlyDown;

        private static int _pendingCtrlCTaps;
        private static bool _isCtrlCApplied;

        private static int _pendingALTGR_Plus_LESSTHAN_Taps;
        private static bool _isALTGR_Plus_LESSTHAN_Applied;

        private static void HookThreadProc()
        {
            _hookThreadId = GetCurrentThreadId();
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(null), 0);
            if (_hookHandle == IntPtr.Zero)
                _lastError = Marshal.GetLastWin32Error();

            _readySignal.Set();

            if (_hookHandle == IntPtr.Zero)
            {
                _hookThreadId = 0;
                return;
            }

            try
            {
                while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }
            finally
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
                _hookThreadId = 0;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                int msg = wParam.ToInt32();
                bool isDown = msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN;
                // bool isUp = msg == WM_KEYUP || msg == WM_SYSKEYUP; // should equal to !isDown in all cases

                if (data.vkCode == VK_CKEY)
                {
                    _cKeyCurrentlyDown = isDown;

                    if (!isDown) _isCtrlCApplied = false;
                }
                else if (data.vkCode == VK_CTRL)
                {
                    _controlCurrentlyDown = isDown;

                    if (!isDown) _isCtrlCApplied = false;
                }
                else if (data.vkCode == VK_ALTGR)
                {
                    _altgrKeyCurrentlyDown = isDown;

                    if (!isDown) _isALTGR_Plus_LESSTHAN_Applied = false;
                }
                else if (data.vkCode == VK_LESSTHAN)
                {
                    _lessThanKeyCurrentlyDown = isDown;

                    if (!isDown) _isALTGR_Plus_LESSTHAN_Applied = false;
                }

                if (!_isCtrlCApplied && _cKeyCurrentlyDown && _controlCurrentlyDown)
                {
                    _isCtrlCApplied = true;
                    Interlocked.Increment(ref _pendingCtrlCTaps);
                }
                else if (!_isALTGR_Plus_LESSTHAN_Applied && _altgrKeyCurrentlyDown && _lessThanKeyCurrentlyDown)
                {
                    _isALTGR_Plus_LESSTHAN_Applied = true;
                    Interlocked.Increment(ref _pendingALTGR_Plus_LESSTHAN_Taps);
                }
            }

            // Never swallow the key - always pass it down the chain so the
            // focused window (ours or anyone else's) still receives it.
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }
    }
}