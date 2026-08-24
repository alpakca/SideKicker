using Microsoft.Xna.Framework;

namespace SideKicker.Theming
{
    internal class ColorDefinition
    {
        public string Id { get; init; }
        public Color? DefaultValue { get; init; }
        public string? FallbackColorId { get; init; }
    }
}
