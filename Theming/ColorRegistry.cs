using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SideKicker.Theming
{
    internal static class ColorRegistry
    {
        public const string BackgroundId = "base.background";
        /// <summary>A general border color.</summary>
        public const string BorderId = "base.border";
        /// <summary>Border color to distinguish a surface from background.</summary>
        public const string BorderAccentId = "base.borderaccent";
        /// <summary>Color id of color to be used in surfaces above background.</summary>
        public const string SurfaceId = "base.surface";
        public const string HoverOverlayId = "base.hoveroverlay";
        public const string HeldOverlayId = "base.heldoverlay";

        public const string TextColorId = "base.textcolor";
        public const string VerticalBarColorId = "base.verticalbarcolor";

        public const string SelectionBoxOverlayId = "base.selectionboxoverlay";

        private static readonly Dictionary<string, ColorDefinition> _definitions = new Dictionary<string, ColorDefinition>();

        public static IEnumerable<string> RegisteredIds => _definitions.Keys;

        [ModuleInitializer]
        public static void InitializeBaseColors()
        {
            Register(new ColorDefinition { Id = BackgroundId });
            Register(new ColorDefinition { Id = BorderId });
            Register(new ColorDefinition { Id = BorderAccentId });
            Register(new ColorDefinition { Id = SurfaceId });
            Register(new ColorDefinition { Id = HoverOverlayId });
            Register(new ColorDefinition { Id = HeldOverlayId });
            Register(new ColorDefinition { Id = TextColorId });
            Register(new ColorDefinition { Id = VerticalBarColorId });
            Register(new ColorDefinition { Id = SelectionBoxOverlayId });
        }

        public static void Register(ColorDefinition definition)
        {
            _definitions[definition.Id] = definition;
        }

        public static ColorDefinition? Get(string id)
        {
            return _definitions.TryGetValue(id, out ColorDefinition? def) ? def : null;
        }
    }
}
