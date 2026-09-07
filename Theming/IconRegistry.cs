using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SideKicker.Theming
{
    internal static class IconRegistry
    {
        private static readonly Dictionary<string, IconDefinition> _definitions = new Dictionary<string, IconDefinition>();

        public static IEnumerable<string> RegisteredIds => _definitions.Keys;

        public const string EditButtonId = "base.editbutton";
        public const string CloseButtonId = "base.closebutton";

        [ModuleInitializer]
        public static void Initialize()
        {
            Register(new IconDefinition
            {
                Id = EditButtonId,
                DefaultPath = @"panels\EditButton.png"
            });

            Register(new IconDefinition
            {
                Id = CloseButtonId,
                DefaultPath = @"panels\CloseButton.png"
            });
        }

        public static void Register(IconDefinition definition)
        {
            _definitions[definition.Id] = definition;
        }

        public static IconDefinition Get(string id)
        {
            return _definitions.TryGetValue(id, out IconDefinition def) ? def : null;
        }
    }
}
