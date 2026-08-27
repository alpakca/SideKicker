using PopUpUI.Elements;
using PopUpUI.Surface;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using PopUpUI.Input;

namespace SideKicker.Panels.PinnedCommands
{
    internal class ContextWindow : PopWindow
    {
        private const string SaveFileName = "pinnedcommands.json";
        private const int PanelTextSize = 12;

        private readonly Dictionary<CommandEditorTextBox, LinearTextBox> _textBoxes;
        private readonly TextPlaque[] _commandEditorTexts;
        private readonly TextedButton[] _addingCommandButtons;
        private readonly TextedButton[] _editingCommandButtons;

        private bool _inReorderMode;
        private int _reorderingButtonIndex;
        private readonly ScrollableButtonList _commandsList;

        public event Action<EditorButton> EditorButtonPressed;
        public event Action<int> CommandListButtonBodyPressed;
        public event Action<int> CommandListButtonEditPressed;
        public event Action<int, int> CommandListButtonsSwaped;

        public ContextWindow()
        {
            _textBoxes = new Dictionary<CommandEditorTextBox, LinearTextBox>();
            InitializeCommandEditor(out _commandEditorTexts, out _addingCommandButtons, out _editingCommandButtons);

            _commandsList = new ScrollableButtonList
            {
                Boundaries = new Rectangle(),
                ButtonHeight = CommandListLayoutCalculator.ButtonHeight,
                ButtonMargin = 4
            };
            _commandsList.ButtonPressed += CommandsList_ButtonPressed;
        }

        private void CommandsList_ButtonPressed(int buttonIndex)
        {
            CommandButtonLayout layout = CommandListLayoutCalculator.GetLayout(_commandsList.GetButtonBounds(buttonIndex), 
                                                                               _commandsList.RelativeMouseToHovered);

            if (layout == CommandButtonLayout.ReorderButton)
            {
                _inReorderMode = true;
                _reorderingButtonIndex = buttonIndex;
            }
            else if (layout == CommandButtonLayout.EditButton)
            {
                CommandListButtonEditPressed?.Invoke(buttonIndex);
            }
            else if (layout == CommandButtonLayout.Body)
            {
                CommandListButtonBodyPressed?.Invoke(buttonIndex);
            }
        }

        public void ShowCommand(SavedCommand? command)
        {
            if (command != null)
            {
                _textBoxes[CommandEditorTextBox.Name].Text = command.Name;
                _textBoxes[CommandEditorTextBox.PathToRunCommand].Text = command.PathToRunCommand;
                _textBoxes[CommandEditorTextBox.Command].Text = command.Command;
            }
            else
            {
                _textBoxes[CommandEditorTextBox.Name].Text = "";
                _textBoxes[CommandEditorTextBox.PathToRunCommand].Text = "";
                _textBoxes[CommandEditorTextBox.Command].Text = "";
            }
        }

        public SavedCommand CreateCommand()
        {
            return new SavedCommand
            {
                Name = _textBoxes[CommandEditorTextBox.Name].Text,
                PathToRunCommand = _textBoxes[CommandEditorTextBox.PathToRunCommand].Text,
                Command = _textBoxes[CommandEditorTextBox.Command].Text
            };
        }

        private void InitializeCommandEditor(out TextPlaque[] commandEditorTexts, out TextedButton[] addingCommandButtons, 
                                             out TextedButton[] editingCommandButtons)
        {
            _textBoxes.Add(CommandEditorTextBox.Name, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(),
                SelectableMargin = 4
            });

            _textBoxes.Add(CommandEditorTextBox.PathToRunCommand, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(),
                SelectableMargin = 4
            });

            _textBoxes.Add(CommandEditorTextBox.Command, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(),
                SelectableMargin = 4
            });

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
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Add))
                }
            };

            editingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Delete",
                    Boundaries = new Rectangle(),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Delete))
                },
                new TextedButton
                {
                    Text = "Save",
                    Boundaries = new Rectangle(),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Save))
                }
            };
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            _commandsList.MouseStateChangeUpdate(e);
            base.OnMouseButtonStateChange(e);
        }

        protected override void OnMousePositionChange(PopWindowMouseEventArgs e)
        {
            if (_inReorderMode)
            {
                int onButton = _commandsList.GetButtonOn(e.MousePosition - _commandsList.Boundaries.Location);
                if (onButton != -1 && onButton != _reorderingButtonIndex)
                {
                    CommandListButtonsSwaped?.Invoke(onButton, _reorderingButtonIndex);
                    _reorderingButtonIndex = onButton;
                }
            }
            else
            {
                _commandsList.MousePositionChanged(e);
            }

            base.OnMousePositionChange(e);
        }

        protected override void OnMouseVerticalScrollChange(PopWindowMouseEventArgs e, int change)
        {
            if (_commandsList.Boundaries.Contains(e.MousePosition))
            {
                _commandsList.Scroll(e, -change / 6);
            }

            base.OnMouseVerticalScrollChange(e, change);
        }
    }
}
