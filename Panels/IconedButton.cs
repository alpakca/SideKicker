using PopUpUI.Elements;
using SideKicker.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideKicker.Panels
{
    internal class IconedButton : Clickable
    {
        public string IconId { get; set; }
        public IconType IconType { get; set; }
    }
}
