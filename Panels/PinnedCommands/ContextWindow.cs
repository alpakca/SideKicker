using PopUpUI.Elements;
using PopUpUI.Surface;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using PopUpUI.Input;
using PopUpUI.Toolkit;
using Microsoft.Xna.Framework.Input;
using System.Linq;

using Clipboard = System.Windows.Forms.Clipboard;

namespace SideKicker.Panels.PinnedCommands
{
    internal class ContextWindow : PopWindow
    {
        public const int PanelTextSize = 12;

        private float _remainingSaveTime;
        private readonly IReadOnlyList<SavedCommand> _savedCommands;
        private readonly ContextWindowRenderer _renderer;

        private bool _inEditingMode;
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
        public event Action SaveTimerElapsed;

        public ContextWindow(IReadOnlyList<SavedCommand> savedCommands)
        {
            _savedCommands = savedCommands;

            // occopies an area of (0, 0, 220, 250)
            _textBoxes = new Dictionary<CommandEditorTextBox, LinearTextBox>();
            InitializeCommandEditor(out _commandEditorTexts, out _addingCommandButtons, out _editingCommandButtons);

            _commandsList = new ScrollableButtonList
            {
                Boundaries = new Rectangle(270, 20, 250, 210),
                ButtonHeight = CommandListLayoutCalculator.ButtonHeight,
                ButtonMargin = 4
            };
            _commandsList.ButtonPressed += CommandsList_ButtonPressed;
            _commandsList.ButtonHeld += CommandsList_ButtonHeld;

            KeyboardShortcutManager shortcutManager = new KeyboardShortcutManager();
            KeyboardCombination pasteLeftCtrl = KeyboardCombination.GetSingleUse([Keys.LeftControl, Keys.V]);
            KeyboardCombination pasteRightCtrl = KeyboardCombination.GetSingleUse([Keys.RightControl, Keys.V]);
            pasteLeftCtrl.FirstTriggerAction = pasteRightCtrl.FirstTriggerAction = KeyboardPasteAction;
            shortcutManager.AddCombinations(pasteLeftCtrl, pasteRightCtrl);

            this.Elements.Add(shortcutManager);
            this.Elements.AddRange(_textBoxes.Values);

            this.Boundaries = new Rectangle(0, 0, 540, 250);
            _renderer = new ContextWindowRenderer(WindowTexture);
        }

        private void CommandsList_ButtonPressed(int buttonIndex)
        {
            CommandButtonLayout layout = CommandListLayoutCalculator.GetLayout(_commandsList.GetButtonBounds(buttonIndex).Size, 
                                                                               _commandsList.RelativeMouseToHovered);

            if (layout == CommandButtonLayout.EditButton)
            {
                CommandListButtonEditPressed?.Invoke(buttonIndex);
            }
            else if (layout == CommandButtonLayout.Body)
            {
                CommandListButtonBodyPressed?.Invoke(buttonIndex);
            }
        }

        private void CommandsList_ButtonHeld(int buttonIndex)
        {
            CommandButtonLayout layout = CommandListLayoutCalculator.GetLayout(_commandsList.GetButtonBounds(buttonIndex).Size,
                                                                   _commandsList.RelativeMouseToHovered);

            if (layout == CommandButtonLayout.ReorderButton)
            {
                _inReorderMode = true;
                _reorderingButtonIndex = buttonIndex;
            }
        }

        private void KeyboardPasteAction()
        {
            LinearTextBox activeBox = _textBoxes.Values.FirstOrDefault(box => box.ReceivingInput);
            if (activeBox == null)
            {
                return;
            }

            if (Clipboard.ContainsText())
            {
                activeBox.InsertText(Clipboard.GetText());
            }
        }

        public void ShowCommand(SavedCommand? command)
        {
            if (command != null)
            {
                _textBoxes[CommandEditorTextBox.Name].Text = command.Name;
                _textBoxes[CommandEditorTextBox.PathToRunCommand].Text = command.PathToRunCommand;
                _textBoxes[CommandEditorTextBox.FileName].Text = command.FileName;

                if (!_inEditingMode)
                {
                    foreach (var button in _addingCommandButtons) this.Elements.Remove(button);
                }

                _inEditingMode = true;
                this.Elements.AddRange(_editingCommandButtons);
            }
            else
            {
                _textBoxes[CommandEditorTextBox.Name].Text = "";
                _textBoxes[CommandEditorTextBox.PathToRunCommand].Text = "";
                _textBoxes[CommandEditorTextBox.FileName].Text = "";

                if (_inEditingMode)
                {
                    foreach (var button in _editingCommandButtons) this.Elements.Remove(button);
                }

                _inEditingMode = false;
                this.Elements.AddRange(_addingCommandButtons);
            }
        }

        public SavedCommand CreateCommand()
        {
            return new SavedCommand
            {
                Name = _textBoxes[CommandEditorTextBox.Name].Text,
                PathToRunCommand = _textBoxes[CommandEditorTextBox.PathToRunCommand].Text,
                FileName = _textBoxes[CommandEditorTextBox.FileName].Text
            };
        }

        public void SetSavedCommandCount(int count)
        {
            _commandsList.ButtonCount = count;
            _commandsList.UpdateScrolledLength();

            _renderer.RefreshIcons(_savedCommands);
        }

        public void SetSaveTimer(float milliseconds)
        {
            _remainingSaveTime = milliseconds;
        }

        private void InitializeCommandEditor(out TextPlaque[] commandEditorTexts, out TextedButton[] addingCommandButtons, 
                                             out TextedButton[] editingCommandButtons)
        {
            _textBoxes.Add(CommandEditorTextBox.Name, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 40, 200, 30),
                SelectableMargin = 4
            });

            _textBoxes.Add(CommandEditorTextBox.PathToRunCommand, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 100, 200, 30),
                SelectableMargin = 4
            });

            _textBoxes.Add(CommandEditorTextBox.FileName, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 160, 200, 30),
                SelectableMargin = 4
            });

            commandEditorTexts = new TextPlaque[]
            {
                new TextPlaque
                {
                    Text = "Visible Name Of Command:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 20)
                },
                new TextPlaque
                {
                    Text = "Directory To Open File:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 80)
                },
                new TextPlaque
                {
                    Text = "File Path:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 140)
                }
            };

            addingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Add",
                    Boundaries = new Rectangle(220 - 90, 200, 90, 30),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Add)),
                    SelectablePacket = new SelectablePacket(),
                    HoldPacket = new HoldablePacket()
                }
            };

            editingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Delete",
                    Boundaries = new Rectangle(20, 200, 90, 30),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Delete)),
                    SelectablePacket = new SelectablePacket(),
                    HoldPacket = new HoldablePacket()
                },
                new TextedButton
                {
                    Text = "Save",
                    Boundaries = new Rectangle(220 - 90, 200, 90, 30),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Save)),
                    SelectablePacket = new SelectablePacket(),
                    HoldPacket = new HoldablePacket()
                }
            };
        }

        protected override void Update()
        {
            if (_remainingSaveTime > 0)
            {
                _remainingSaveTime -= Time.DeltaTime;
                if (_remainingSaveTime <= 0)
                {
                    SaveTimerElapsed?.Invoke();
                }
            }

            base.Update();
        }

        protected override void RenderUpdate()
        {
            _renderer.RenderMainElements(_commandEditorTexts, _inEditingMode ? _editingCommandButtons : _addingCommandButtons);

            foreach (LinearTextBox textBox in _textBoxes.Values)
            {
                _renderer.RenderTextBox(textBox);
            }

            for (int i = 0; i < _savedCommands.Count; i++)
            {
                _renderer.RenderCommandListButtonText(_commandsList, i, _savedCommands[i].Name);
            }

            _renderer.RenderCommandListBackgrounds(_commandsList, _savedCommands);

            base.RenderUpdate();
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            if (e.ReleasedButtons.HasLeft)
            {
                _inReorderMode = false;
            }

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

            _commandsList.MousePositionChanged(e);
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
