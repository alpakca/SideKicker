using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace SideKicker.Panels
{
    internal interface IClipboardSupporter
    {
        public bool ReceivingInput { get; }

        public void CopyFromClipboard(string text);
        public string? GetSelectedText();
    }
}
