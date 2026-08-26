using PopUpUI.Elements;
using PopUpUI.Surface;
using Microsoft.Xna.Framework;

namespace SideKicker.Panels.PinnedCommands
{
    internal class ContextWindow : PopWindow
    {
        private const string SaveFileName = "pinnedcommands.json";
        private const int PanelTextSize = 12;

        private bool _isEditingCommand;
        private readonly CommandEditor _commandEditor;
        private readonly TextPlaque[] _commandEditorTexts;
        private readonly TextedButton[] _addingCommandButtons;
        private readonly TextedButton[] _editingCommandButtons;

        public ContextWindow()
        {
            InitializeCommandEditor(out _commandEditorTexts, out _commandEditor, out _addingCommandButtons, out _editingCommandButtons);
        }

        private void InitializeCommandEditor(out TextPlaque[] commandEditorTexts, out CommandEditor commandEditor,
                                             out TextedButton[] addingCommandButtons, out TextedButton[] editingCommandButtons)
        {
            LinearTextBox commandName = new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(),
                SelectableMargin = 4
            };

            LinearTextBox pathToRunCommand = new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(),
                SelectableMargin = 4
            };

            LinearTextBox command = new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(),
                SelectableMargin = 4
            };

            commandEditorTexts = new TextPlaque[]
            {
                new TextPlaque
                {
                    Text = "Visible Name Of Command:",
                    TextSize = PanelTextSize,
                    Position = new Vector2()
                },
                new TextPlaque
                {
                    Text = "Directory To Run The Command In:",
                    TextSize = PanelTextSize,
                    Position = new Vector2()
                },
                new TextPlaque
                {
                    Text = "Command To Run:",
                    TextSize = PanelTextSize,
                    Position = new Vector2()
                }
            };

            addingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Add",
                    Boundaries = new Rectangle(),
                    ClickCommand = new ActionCommander<Clickable>(_ => { })
                }
            };

            editingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Delete",
                    Boundaries = new Rectangle(),
                    ClickCommand = new ActionCommander<Clickable>(_ => { })
                },
                new TextedButton
                {
                    Text = "Save",
                    Boundaries = new Rectangle(),
                    ClickCommand = new ActionCommander<Clickable>(_ => { })
                }
            };

            commandEditor = new CommandEditor(commandName, pathToRunCommand, command);
            commandEditor.ShowEmptyCommand();
            _isEditingCommand = false;
        }
    }
}
