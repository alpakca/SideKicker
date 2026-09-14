using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Native
{
    internal static class NativeService
    {
        private static bool _isInitialized;

        public static INativeService Current { get; private set; }

        public static void Initialize(INativeService implementation, IntPtr windowHandle)
        {
            if (_isInitialized) return;

            Current = implementation;

            Current.InitializeWindowFlags(windowHandle);
            Current.InitializeWindowHotkeys();

            _isInitialized = true;
        }
    }
}
